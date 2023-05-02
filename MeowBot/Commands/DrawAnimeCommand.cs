using EleCho.GoCqHttpSdk;
using EleCho.GoCqHttpSdk.Post;
using RustSharp;

namespace MeowBot;

internal class DrawAnimeCommand : Command
{
    public DrawAnimeCommand(AppConfig config, CqWsSession session) : base(config, session)
    {
        Prefix = "#drawanime";
        Description = "画动漫人物";
        Help =
        """
        测试状态
        暂时不支持指定绘画风格
        """;
    }

    public override async Task<bool> ExecAsync(CqGroupMessagePostContext context, IOpenAiComplection aiSession)
    {
        var msg = context.Message.Text.Trim();
        if (!CheckPrefix(msg))
            return false;
        var desc = msg[Prefix.Length..];
        var drawContext = AiContext.Context["draw"];
        drawContext += $"\n{desc}";
        var result = await aiSession.AskAsync(drawContext);
        if (result is OkResult<string, string> ok)
        {
            await Console.Out.WriteLineAsync(ok.Value);
        }
        else
        {
            await session.SendGroupMsgAsync(context.GroupId, context.UserId, "失败");
        }
        return true;
    }
}