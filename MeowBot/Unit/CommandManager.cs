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
            Commands = commandTypes!.Select(type => (Activator.CreateInstance(type, config, session) as Command)!);
        this.config = config;
        this.session = session;
    }

    public async Task ExecAsync(CqGroupMessagePostContext context, IOpenAiComplection aiSession)
    {
        // 对帮助命令做特殊处理
        if (context.Message.Text.HasStartsWith(new[] { "#help" }, true))
        {
            StringBuilder helpText = new("(机器人指令)\n\n");
            Commands.ForEach(cmd =>
            {
                helpText.AppendLine($"{cmd.Prefix.PadRight(12)}{cmd.Description}");
                if (cmd.Help.NotEmpty())
                    helpText.AppendLine("".PadRight(12) + cmd.Help);
            });
            helpText.AppendLine($"注意, 普通用户最多保留 {AppConfig.MaxHistory} 条聊天记录, 多的会被删除, 也就是说, 机器人会逐渐忘记你");
            await Console.Out.WriteLineAsync(helpText.ToString());
            return;
        }
        // 执行设置的command命令
        foreach (var cmd in Commands)
        {
            if (await cmd.ExecAsync(context, aiSession))
                break;
        }

        bool dequeue = false;
        if (aiSession.History.Count > AppConfig.MaxHistory && (config.AllowList == null || !config.AllowList.Contains(context.UserId)))
            dequeue = true;

        if (dequeue)
            while (aiSession.History.Count > AppConfig.MaxHistory)
                aiSession.History.Dequeue();

        try
        {
            Result<string, string> result =
                await aiSession.AskAsync(context.Message.Text);

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