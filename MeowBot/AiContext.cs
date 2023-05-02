using Collapsenav.Net.Tool;

namespace MeowBot;

/// <summary>
/// 角色上下文
/// </summary>
public class AiContext
{
    public static string ContextPath = "AiContext.json";
    static AiContext()
    {
        if (File.Exists(ContextPath))
        {
            var dict = File.ReadAllText(ContextPath).ToObj<Dictionary<string, string>>();
            foreach (var kv in dict.Where(item => item.Key.NotEmpty()))
                Context.AddOrUpdate(kv);
        }
    }
    /// <summary>
    /// Role集合
    /// </summary>
    public static Dictionary<string, string> Context = new();

    public static string? GetFromName(string name)
    {
        if (!name.ToLower().In(Context.Keys))
            return null;
        return Context[name.ToLower()];
    }
}