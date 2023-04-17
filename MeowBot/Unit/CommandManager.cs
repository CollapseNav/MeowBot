using System.Text;
using Collapsenav.Net.Tool;
using EleCho.GoCqHttpSdk;
using EleCho.GoCqHttpSdk.Message;
using EleCho.GoCqHttpSdk.Post;
using RustSharp;

namespace MeowBot;

internal class CommandManager
{
    private readonly AppConfig config;
    private readonly CqWsSession session;
    public IEnumerable<Command> Commands { get; private set; }
    public CommandManager(AppConfig config, CqWsSession session)
    {
        var commandTypes = AppDomain.CurrentDomain.GetCustomerTypes<Command>().Where(item => !item.IsAbstract);
        if (commandTypes.NotEmpty())
        {
            var normalCmds = commandTypes.Where(type => type != typeof(HelpCommand));
            Commands = normalCmds!.Select(type =>
            {
                var cmd = (Activator.CreateInstance(type, config, session) as Command)!;
                return cmd;
            });
            if (commandTypes.Any(type => type == typeof(HelpCommand)))
            {
                Commands = Commands.Append(new HelpCommand(config, session, Commands.ToList()));
            }
        }
        this.config = config;
        this.session = session;
    }

    public async Task ExecAsync(CqGroupMessagePostContext context, IOpenAiComplection aiSession)
    {
        // 执行设置的command命令
        foreach (var cmd in Commands)
        {
            if (await cmd.ExecAsync(context, aiSession))
                return;
        }
        bool dequeue = false;
        if (aiSession.History.Count > AppConfig.MaxHistory && (config.AllowList == null || !config.AllowList.Contains(context.UserId)))
            dequeue = true;

        if (dequeue)
            while (aiSession.History.Count > AppConfig.MaxHistory)
                aiSession.History.Dequeue();

        try
        {
            Result<string, string> result = await aiSession.AskAsync(context.Message.Text);
            switch (result)
            {
                case OkResult<string, string> ok:
                    CqMessage message = new CqMessage()
                    {
                        new CqAtMsg(context.UserId),
                        new CqTextMsg(ok.Value),
                    };
                    if (dequeue)
                        message.WithTail($"消息历史记录超过 {AppConfig.MaxHistory} 条, 已删去多余的历史记录");
                    await session.SendGroupMessageAsync(context.GroupId, message);
                    break;
                case ErrResult<string, string> err:
                    await session.SendGroupMsgAsync(context.GroupId, context.UserId, $"请求失败, 请重新尝试, 你也可以使用 #reset 重置机器人. {err.Value}");
                    break;
            }
        }
        catch (Exception ex)
        {
            await session.SendGroupMsgAsync(context.GroupId, context.UserId, "请求失败, 请重新尝试, 你也可以使用 #reset 重置机器人");
            await Console.Out.WriteLineAsync($"Exception: {ex}");
        }
    }
}