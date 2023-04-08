using Collapsenav.Net.Tool;

namespace MeowBot;

public class DefaultAiContext
{
    public static string ContextPath = "AiContext.json";
    static DefaultAiContext()
    {
        AiContext.AddOrUpdate("CatGirl".ToLower(), CatGirl);
        AiContext.AddOrUpdate("NewBing".ToLower(), NewBing);
        if (File.Exists(ContextPath))
        {
            var dict = File.ReadAllText(ContextPath).ToObj<Dictionary<string, string>>();
            foreach (var kv in dict)
                AiContext.AddOrUpdate(kv);
        }
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