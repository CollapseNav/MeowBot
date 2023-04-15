using EleCho.GoCqHttpSdk;
using EleCho.GoCqHttpSdk.Message;
using EleCho.GoCqHttpSdk.Post;

namespace MeowBot;

internal class CustomRoleCommand : Command
{
    public CustomRoleCommand(AppConfig config, CqWsSession session) : base(config, session)
    {
        Prefix = "#custom-role";
        Description = "自定义角色";
        Help = "@机器人之后直接输入你想要的角色设定";
    }

    public override async Task<bool> ExecAsync(CqGroupMessagePostContext context, IOpenAiComplection aiSession)
    {
        var msg = context.Message.Text.Trim();
        if (!CheckPrefix(msg))
            return false;
        string initText = msg[Prefix.Length..];
        aiSession.InitWithText(initText);
        await session.SendGroupMessageAsync(context.GroupId, new CqMessage()
        {
            new CqAtMsg(context.UserId),
            new CqTextMsg($"角色已更新:\n{initText}")
        });
        aiSession.Reset();
        return true;
    }
}