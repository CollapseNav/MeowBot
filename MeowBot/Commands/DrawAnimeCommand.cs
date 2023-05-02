using System.Net.Http.Json;
using Collapsenav.Net.Tool;
using EleCho.GoCqHttpSdk;
using EleCho.GoCqHttpSdk.Message;
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

    public class ImageResult
    {
        public string[] Images { get; set; }
    }
    public override async Task<bool> ExecAsync(CqGroupMessagePostContext context, IOpenAiComplection aiSession)
    {
        var msg = context.Message.Text.Trim();
        if (!CheckPrefix(msg))
            return false;
        var desc = msg[Prefix.Length..];
        var drawContext = AiContext.Context["draw"];
        drawContext += $"\n{desc}";
        aiSession.Reset();
        var result = await aiSession.AskAsync(drawContext);
        if (result is OkResult<string, string> ok)
        {
            var temp = ok.Value[5..].Trim();
            var prompts = temp.Split(',').Select(item => item.Trim());
            await Console.Out.WriteLineAsync(prompts.ToJson());
            prompts = prompts.Where(item => item.Length < 50).ToArray();
            HttpClient client = new HttpClient();
            string url = "http://49.235.67.56:7503/sdapi/v1/txt2img";

            var res = await client.PostAsJsonAsync(url, new
            {
                prompt = prompts.Join(","),
                negative_prompt = "(worst quality, low quality:1.4), monochrome, zombie,",
                steps = 20
            });
            var str = await res.Content.ReadAsStringAsync();
            var bytes = str.ToObj<ImageResult>().Images.First().FromBase64();
            var guid = Guid.NewGuid().ToString();
            var filePath = $"/data/images/{guid}.png";
            bytes.SaveTo(filePath);
            await session.SendGroupMessageAsync(context.GroupId, new CqMessage {
                new CqImageMsg($"{guid}.png","")
            });
        }
        else
        {
            await session.SendGroupMsgAsync(context.GroupId, context.UserId, "失败");
        }
        return true;
    }
}