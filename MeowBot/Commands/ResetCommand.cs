using Collapsenav.Net.Tool;
using EleCho.GoCqHttpSdk;
using EleCho.GoCqHttpSdk.Post;

namespace MeowBot;

internal class ResetCommand : Command
{
    public ResetCommand(AppConfig config, CqWsSession session) : base(config, session)
    {
        Prefix = "#reset";
        Description = "重置会话";
        Help = "@机器人之后使用 #reset 即可重置当前会话";
    }

    public override async Task<bool> ExecAsync(CqGroupMessagePostContext context, IOpenAiComplection aiSession)
    {
        var msg = context.Message.Text.Trim();
        if (!CheckPrefix(msg))
            return false;
        aiSession.Reset();
        var groupConfig = config.GetGroupConfig(context.GroupId);
        if (groupConfig != null)
        {
            var aiContext = AiContext.GetFromName(groupConfig.Role);
            if (aiContext.NotEmpty())
                aiSession.InitWithText(aiContext);
        }
        await session.SendGroupMsgAsync(context.GroupId, context.UserId, "会话已重置");
        return true;
    }
}