using Collapsenav.Net.Tool;
using EleCho.GoCqHttpSdk.Post;

namespace MeowBot;

/// <summary>
/// 命令
/// </summary>
internal abstract class Command
{
    /// <summary>
    /// 前缀
    /// </summary>
    public string Prefix { get; set; }
    /// <summary>
    /// 描述
    /// </summary>
    public string Description { get; set; }
    /// <summary>
    /// 帮助
    /// </summary>
    public string Help { get; set; }
    public abstract Task ExecAsync(CqGroupMessagePostContext context, IOpenAiComplection session);
    public bool CheckPrefix(string msg)
    {
        return msg.HasStartsWith(new[] { Prefix }, true);
    }
}