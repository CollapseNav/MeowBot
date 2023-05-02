using System.Diagnostics.CodeAnalysis;
using Collapsenav.Net.Tool;
using EleCho.GoCqHttpSdk;
using EleCho.GoCqHttpSdk.Message;
using MeowBot;

// 设置默认json序列化带缩进
JsonExt.DefaultJsonSerializerOption.WriteIndented = true;
if (!TryLoadConfig(out var appConfig))
    return;
AppConfig.CurrentConfig = appConfig;
CqWsSession session = new(new CqWsSessionOptions()
{
    BaseUri = new Uri(appConfig.BotWebSocketUri!)
});
// 拦截黑名单
session.UseGroupMessage(async (context, next) =>
{
    if (appConfig.BlockList.Contains(context.UserId))
        return;
    await next.Invoke();
});
CommandManager manager = new(appConfig, session);

Dictionary<(long, long), AiComplectionSessionStorage> aiSessions = new Dictionary<(long, long), AiComplectionSessionStorage>();
session.UseGroupMessage(async context =>
{
    try
    {
        if (context.Message.Any(msg => msg is CqAtMsg atMsg && atMsg.Target == context.SelfId))
        {
            string msgTxt = context.Message.Text.Trim();
        
            if (!aiSessions.TryGetValue((context.GroupId, context.UserId), out AiComplectionSessionStorage? aiSession))
            {
                aiSessions[(context.GroupId, context.UserId)] = aiSession = new(
                    appConfig.CreateNewSession());

                if (aiSession.Session is OpenAiChatCompletionSession chatCompletionSession)
                {
                    if (context.GroupId.In(appConfig.GroupConfigs?.Select(item => item.GroupId).ToArray()))
                    {
                        var role = appConfig.GroupConfigs.FirstOrDefault(item => item.GroupId == context.GroupId)?.Role;
                        if (DefaultAiContext.GetFromName(role).IsEmpty())
                            chatCompletionSession.InitCatGirl();
                        else
                            chatCompletionSession.InitWithText(DefaultAiContext.GetFromName(role));
                    }
                    else
                        chatCompletionSession.InitCatGirl();
                }
            }

            if (!appConfig.AllowList.Contains(context.UserId) && aiSession.GetUsageCountInLastDuration(TimeSpan.FromSeconds(appConfig.UsageLimitTime)) >= appConfig.UsageLimitCount)
            {
                string helpText =
                    $"""
                            (你不在机器人白名单内, {appConfig.UsageLimitTime}秒内仅允许使用{appConfig.UsageLimitCount}次.)
                            """;
                await session.SendGroupMsgAsync(context.GroupId, context.UserId, helpText);

                return;
            }

            await manager.ExecAsync(context, aiSession.Session);
        }
    }
    catch (Exception ex)
    {
        await Console.Out.WriteLineAsync($"{ex}");
    }
});

session.UseGroupMessage(async context =>
{
    if (context.Message.Text.StartsWith("echo ", StringComparison.OrdinalIgnoreCase))
    {
        await session.SendGroupMessageAsync(context.GroupId, new CqMessage(context.Message.Text.Substring(5)));
    }
    return;
});

// 群邀请
session.UseGroupRequest(async context =>
{
    if (appConfig.AllowList.Contains(context.UserId))
        await session.ApproveGroupRequestAsync(context.Flag, context.GroupRequestType);
    else
        await session.RejectGroupRequestAsync(context.Flag, context.GroupRequestType, "只有白名单用户才可以邀请小猫到群聊");
});

AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

while (true)
{

    try
    {
        await session.StartAsync();
        await Console.Out.WriteLineAsync("连接完毕啦 ヾ(≧▽≦*)o");

        await Console.Out.WriteLineAsync($"模型: {appConfig.GptModel ?? AppConfig.DefaultGptModel}");
        await Console.Out.WriteLineAsync($"聊天 API: {appConfig.ChatCompletionApiUrl ?? AppConfig.DefaultChatCompletionApiUrl}");
        await Console.Out.WriteLineAsync($"UsageLimitTime: {appConfig.UsageLimitTime}");
        await Console.Out.WriteLineAsync($"UsageLimitCount: {appConfig.UsageLimitCount}");

        await Console.Out.WriteLineAsync("白名单:");
        foreach (var item in appConfig.AllowList)
            await Console.Out.WriteLineAsync($" | {item}");

        await Console.Out.WriteLineAsync("黑名单:");
        foreach (var item in appConfig.BlockList)
            await Console.Out.WriteLineAsync($" | {item}");

        await Console.Out.WriteLineAsync("管理员:");
        foreach (var item in appConfig.AdminList)
            await Console.Out.WriteLineAsync($" | {item}");

        await session.WaitForShutdownAsync();

        await Console.Out.WriteLineAsync("连接已结束... 5s 后重连");
        await Task.Delay(5000);
    }
    catch (Exception ex)
    {
        await Console.Out.WriteLineAsync($"{ex}");
        await Console.Out.WriteLineAsync("连接已结束... 2s 后重连");
        await Task.Delay(2000);
    }
}
/// <summary>
/// 读个配置, 好凉凉~
/// </summary>
/// <param name="appConfig"></param>
/// <returns></returns>
static bool TryLoadConfig([NotNullWhen(true)] out AppConfig? appConfig)
{
    if (!File.Exists(AppConfig.Filename))
    {
        new AppConfig().SaveConfig();
        Console.WriteLine("配置文件已生成, 请编辑后启动程序");
        Utils.PressAnyKeyToContinue();
        appConfig = null;
        return false;
    }
    appConfig = AppConfig.LoadFromDefault();
    if (appConfig == null)
    {
        Console.WriteLine("配置文件是空的, 请确认配置文件正确, 或者删除配置文件并重启本程序以重新生成");
        Utils.PressAnyKeyToContinue();
        return false;
    }
    var path = AppDomain.CurrentDomain.BaseDirectory;
    Directory.GetFiles(path).ForEach(Console.WriteLine);
    return appConfig.CheckBotSocket() && appConfig.CheckApiKey();
}
/// <summary>
/// 异常了捏~
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>
static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
{
    Console.WriteLine("出现了不可预知的异常");
    if (e.ExceptionObject is Exception ex)
    {
        Console.WriteLine(ex);
    }
}
