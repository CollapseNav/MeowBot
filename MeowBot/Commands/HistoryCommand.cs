using EleCho.GoCqHttpSdk;
using EleCho.GoCqHttpSdk.Message;
using EleCho.GoCqHttpSdk.Post;

namespace MeowBot;

internal class HistoryCommand : Command
{
    public HistoryCommand(AppConfig config, CqWsSession session) : base(config, session)
    {
        Prefix = "#history";
        Description = "历史信息";
    }

    public override async Task<bool> ExecAsync(CqGroupMessagePostContext context, IOpenAiComplection aiSession)
    {
        var msg = context.Message.Text.Trim();
        if (!CheckPrefix(msg))
            return false;
        CqMessage message = new CqMessage()
        {
            new CqAtMsg(context.UserId),
            new CqTextMsg($"历史记录: {aiSession.History.Count}条")
        };

        bool inWhiteList = false;
        if (config.AllowList != null && config.AllowList.Contains(context.UserId))
            inWhiteList = true;

        if (!inWhiteList)
            message.Add(new CqTextMsg($"(您的聊天会话最多保留 {AppConfig.MaxHistory} 条消息)"));

        await session.SendGroupMessageAsync(context.GroupId, message);
        return true;
    }
}