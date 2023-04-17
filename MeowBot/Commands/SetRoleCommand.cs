using EleCho.GoCqHttpSdk;
using EleCho.GoCqHttpSdk.Post;

namespace MeowBot;

internal class SetRoleCommand : AdminCommand
{
    public SetRoleCommand(AppConfig config, CqWsSession session) : base(config, session)
    {
        Prefix = "#setrole";
        Description = "设置群角色";
        Help = "通过 {群号}:{角色} 的格式给某个群设置角色";
    }

    public override async Task<bool> ExecAsync(CqGroupMessagePostContext context, IOpenAiComplection aiSession)
    {
        var msg = context.Message.Text.Trim();
        if (!CheckPrefix(msg))
            return false;
        if (!config.IsAdmin(context.UserId))
        {
            await session.SendGroupMsgAsync(context.GroupId, context.UserId, $"只有管理员才可以使用该命令");
            return true;
        }
        var grouprole = msg[Prefix.Length..].Trim().Split(":");
        config.SetGroupRole(long.Parse(grouprole[0]), grouprole[1]);
        await session.SendGroupMsgAsync(context.GroupId, context.UserId, $"群号 {grouprole[0]} 角色设置为 {grouprole[1]}");
        aiSession.InitWithText(AiContext.GetFromName(grouprole[1]));
        return true;
    }
}