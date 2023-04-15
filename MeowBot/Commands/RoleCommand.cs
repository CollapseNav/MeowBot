using Collapsenav.Net.Tool;
using EleCho.GoCqHttpSdk;
using EleCho.GoCqHttpSdk.Post;

namespace MeowBot;

internal class RoleCommand : Command
{
    public RoleCommand(AppConfig config, CqWsSession session) : base(config, session)
    {
        Prefix = "#role";
        Description = "设置当前会话角色";
        Help = @$"输入角色名称即可进行切换
当前支持 {AiContext.Context.Keys.Join(",")}
示例: #role {{角色名称}}";
    }

    public override async Task<bool> ExecAsync(CqGroupMessagePostContext context, IOpenAiComplection aiSession)
    {
        var msg = context.Message.Text.Trim();
        if (!CheckPrefix(msg))
            return false;

        string role = msg[Prefix.Length..].Trim();
        string? initText = AiContext.GetFromName(role);
        if (initText != null)
        {
            aiSession.InitWithText(initText);
            await session.SendGroupMsgAsync(context.GroupId, context.UserId, $"角色已更新:\n{initText}");
        }
        else
            await session.SendGroupMsgAsync(context.GroupId, context.UserId, $"找不到执行的角色");
        return true;
    }
}