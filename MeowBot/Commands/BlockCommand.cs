using EleCho.GoCqHttpSdk;
using EleCho.GoCqHttpSdk.Post;

namespace MeowBot;
internal class BlockCommand : Command
{
    public BlockCommand(AppConfig config, CqWsSession session) : base(config, session)
    {
        Prefix = "#block";
        Description = "添加黑名单";
        Help = "多个账号中间使用英文逗号\",\"隔开";
    }

    public override async Task<bool> ExecAsync(CqGroupMessagePostContext context, IOpenAiComplection session)
    {
        var msg = context.Message.Text.Trim();
        if (!CheckPrefix(msg))
            return false;
        var users = msg[Prefix.Length..].Trim();
        config.AddBlockList(users.Split(',').Select(long.Parse).ToArray());
        await Task.FromResult(0);
        return true;
    }
}