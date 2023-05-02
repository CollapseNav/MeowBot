using EleCho.GoCqHttpSdk;
using EleCho.GoCqHttpSdk.Post;

namespace MeowBot;
internal class BlockCommand : AdminCommand
{
    public BlockCommand(AppConfig config, CqWsSession session) : base(config, session)
    {
        Prefix = "#block";
        Description = "添加黑名单";
        Help = "多个账号中间使用英文逗号\",\"隔开";
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
        var users = msg[Prefix.Length..].Trim();
        config.AddBlockList(users.Split(',').Select(long.Parse).ToArray());
        await session.SendGroupMsgAsync(context.GroupId, context.UserId, $"账号 {users} 加入黑名单");
        return true;
    }
}