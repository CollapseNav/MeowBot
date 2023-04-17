using Collapsenav.Net.Tool;
using EleCho.GoCqHttpSdk;
using EleCho.GoCqHttpSdk.Message;
using EleCho.GoCqHttpSdk.Post;
using RustSharp;

namespace MeowBot;
/// <summary>
/// 普通消息, 非命令
/// </summary>
internal class NoCommand : Command
{
    public NoCommand(AppConfig config, CqWsSession session) : base(config, session) { }

    public override async Task<bool> ExecAsync(CqGroupMessagePostContext context, IOpenAiComplection aiSession)
    {
        var msg = context.Message.Text.Trim();
        if (msg.StartsWith('#'))
            return false;
        var groupConfig = config.GetGroupConfig(context.GroupId);

        bool dequeue = false;
        if (aiSession.History.Count > (groupConfig?.MaxHistory ?? AppConfig.MaxHistory) && (config.AllowList == null || !config.AllowList.Contains(context.UserId)))
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
                    CqMessage message = new()
                    {
                        new CqAtMsg(context.UserId),
                        new CqTextMsg(ok.Value),
                    };
                    await session.SendGroupMessageAsync(context.GroupId, message);
                    break;
                case ErrResult<string, string> err:
                    await session.SendGroupMsgAsync(context.GroupId, context.UserId, $"请求失败, 请重新尝试, 你也可以使用 #reset 重置上下文. {err.Value}");
                    break;
            }
        }
        catch (Exception ex)
        {
            await session.SendGroupMsgAsync(context.GroupId, context.UserId, "请求失败, 请重新尝试, 你也可以使用 #reset 重置机器人");
            await Console.Out.WriteLineAsync($"Exception: {ex}");
        }
        return true;
    }
}