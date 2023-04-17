using System.Text;
using EleCho.GoCqHttpSdk;
using EleCho.GoCqHttpSdk.Post;

namespace MeowBot;

internal class ListRoleCommand : Command
{
    public ListRoleCommand(AppConfig config, CqWsSession session) : base(config, session)
    {
        Prefix = "#listrole";
        Description = "列出角色";
        Help = "列出系统中预设的角色上下文设置，不包括用户自己的设置";
    }

    public override async Task<bool> ExecAsync(CqGroupMessagePostContext context, IOpenAiComplection aiSession)
    {
        var msg = context.Message.Text.Trim();
        if (!CheckPrefix(msg))
            return false;
        StringBuilder sb = new();
        foreach (var role in AiContext.Context)
            sb.AppendLine($"{role.Key}:{role.Value}");
        await session.SendGroupMsgAsync(context.GroupId, context.UserId, sb.ToString());
        return true;
    }
}