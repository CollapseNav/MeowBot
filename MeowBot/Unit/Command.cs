using Collapsenav.Net.Tool;
using EleCho.GoCqHttpSdk;
using EleCho.GoCqHttpSdk.Post;

namespace MeowBot;

/// <summary>
/// 命令
/// </summary>
internal abstract class Command
{
    protected readonly AppConfig config;
    protected readonly CqWsSession session;
    protected Command(AppConfig config, CqWsSession session)
    {
        this.config = config;
        this.session = session;
    }

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
    public abstract Task<bool> ExecAsync(CqGroupMessagePostContext context, IOpenAiComplection aiSession);
    public bool CheckPrefix(string msg)
    {
        return msg.HasStartsWith(new[] { Prefix }, true);
    }
}