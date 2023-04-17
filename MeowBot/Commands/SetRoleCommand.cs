using EleCho.GoCqHttpSdk;
using EleCho.GoCqHttpSdk.Post;

namespace MeowBot;

internal class SetRoleCommand : AdminCommand
{
    public SetRoleCommand(AppConfig config, CqWsSession session) : base(config, session)
    {
        Prefix = "#setrole";
        Description = "设置群角色";
        Help = "通过 {群号}:{角色} 的格式给某个群设置角色\n如果不输入角色名，则设置当前群角色";
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
        var groupId = grouprole.Length > 1 ? long.Parse(grouprole[0]) : context.GroupId;
        var role = grouprole.Last();
        config.SetGroupRole(groupId, role);
        await session.SendGroupMsgAsync(context.GroupId, context.UserId, $"群号 {groupId} 角色设置为 {role}");
        aiSession.InitWithText(AiContext.GetFromName(role));
        return true;
    }
}