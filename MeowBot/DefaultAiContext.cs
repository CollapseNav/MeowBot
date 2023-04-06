using Collapsenav.Net.Tool;

namespace MeowBot;

public class DefaultAiContext
{
    static DefaultAiContext()
    {
        AiContext.AddOrUpdate("CatGirl".ToLower(), CatGirl);
        AiContext.AddOrUpdate("NewBing".ToLower(), NewBing);
    }
    public static string CatGirl = OpenAiCompletionInitTexts.CatGirl;
    public static string NewBing = OpenAiCompletionInitTexts.NewBing;
    public static Dictionary<string, string> AiContext = new();

    public static string? GetFromName(string name)
    {
        if (!name.ToLower().In(AiContext.Keys))
            return null;
        return AiContext[name.ToLower()];
    }
}