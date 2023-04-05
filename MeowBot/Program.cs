﻿using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Collapsenav.Net.Tool;
using EleCho.GoCqHttpSdk;
using EleCho.GoCqHttpSdk.Message;
using MeowBot;
using RustSharp;

internal partial class Program
{

    /// <summary>
    /// 读个配置, 好凉凉~
    /// </summary>
    /// <param name="appConfig"></param>
    /// <returns></returns>
    private static bool TryLoadConfig([NotNullWhen(true)] out AppConfig? appConfig)
    {
        if (!File.Exists(AppConfig.Filename))
        {
            JsonExt.DefaultJsonSerializerOption.WriteIndented = true;
            new AppConfig().ToJson().ToBytes().SaveTo(AppConfig.Filename);

            Console.WriteLine("配置文件已生成, 请编辑后启动程序");
            Utils.PressAnyKeyToContinue();

            appConfig = null;
            return false;
        }

        appConfig = File.ReadAllText(AppConfig.Filename).ToObj<AppConfig>();

        if (appConfig == null)
        {
            Console.WriteLine("配置文件是空的, 请确认配置文件正确, 或者删除配置文件并重启本程序以重新生成");
            Utils.PressAnyKeyToContinue();
            return false;
        }

        return true;
    }

    /// <summary>
    /// 代码过于屎山, 容易引起不适
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    private static async Task Main(string[] args)
    {
        if (!TryLoadConfig(out var appConfig))
            return;
        if (appConfig.BotWebSocketUri.IsEmpty())
        {
            Console.WriteLine("请指定机器人 WebSocket URI");
            Utils.PressAnyKeyToContinue();
            return;
        }

        if (appConfig.ApiKey.IsEmpty())
        {
            Console.WriteLine("请指定机器人 API Key");
            Utils.PressAnyKeyToContinue();
            return;
        }

        CqWsSession session = new CqWsSession(new CqWsSessionOptions()
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


        Dictionary<long, AiComplectionSessionStorage> aiSessions = new Dictionary<long, AiComplectionSessionStorage>();
        session.UseGroupMessage(async context =>
        {
            int maxHistoryCount = 50;

            try
            {
                if (context.Message.Any(msg => msg is CqAtMsg atMsg && atMsg.Target == context.SelfId))
                {
                    string msgTxt = context.Message.Text.Trim();

                    if (!aiSessions.TryGetValue(context.UserId, out AiComplectionSessionStorage? aiSession))
                    {
                        aiSessions[context.UserId] = aiSession = new(
                            new OpenAiChatCompletionSession(
                                appConfig.ApiKey,
                                appConfig.ChatCompletionApiUrl ?? AppConfig.DefaultChatCompletionApiUrl,
                                appConfig.GptModel ?? AppConfig.DefaultGptModel));

                        if (aiSession.Session is OpenAiChatCompletionSession chatCompletionSession)
                            chatCompletionSession.InitCatGirl();
                    }

                    if (!appConfig.AllowList.Contains(context.UserId) && aiSession.GetUsageCountInLastDuration(TimeSpan.FromMinutes(5)) >= 5)
                    {
                        string helpText =
                            $"""
                            (你不在机器人白名单内, {appConfig.UsageLimitTime}秒内仅允许使用{appConfig.UsageLimitCount}次.)
                            """;

                        await session.SendGroupMessageAsync(context.GroupId, new CqMessage()
                        {
                            new CqAtMsg(context.UserId),
                            new CqTextMsg(helpText)
                        });

                        return;
                    }

                    if (msgTxt.StartsWith("#help", StringComparison.OrdinalIgnoreCase))
                    {
                        string helpText =
                            $"""
                            (机器人指令)

                            #role:角色           切换角色, 目前支持 CatGirl(猫娘), NewBing(嘴臭必应)
                            #custom-role:内容    自定义角色
                            #reset               重置聊天记录
                            {(!context.UserId.In(appConfig.AdminList) ? "" :
                            """
                            
                            """
                            )}

                            注意, 普通用户最多保留 {maxHistoryCount} 条聊天记录, 多的会被删除, 也就是说, 机器人会逐渐忘记你
                            """;
                        await session.SendGroupMsgAsync(context.GroupId, context.UserId, helpText);
                        // await session.SendGroupMessageAsync(context.GroupId, new CqMessage()
                        // {
                        //     new CqAtMsg(context.UserId),
                        //     new CqTextMsg(helpText)
                        // });
                    }
                    else if (msgTxt.StartsWith("#reset", StringComparison.OrdinalIgnoreCase))
                    {
                        aiSession.Session.Reset();
                        await session.SendGroupMessageAsync(context.GroupId, new CqMessage()
                        {
                            new CqAtMsg(context.UserId),
                            new CqTextMsg("> 会话已重置")
                        });

                        return;
                    }
                    else if (msgTxt.StartsWith("#role:", StringComparison.OrdinalIgnoreCase) ||
                             msgTxt.StartsWith("#role ", StringComparison.OrdinalIgnoreCase))
                    {
                        string role = msgTxt.Substring(6).Trim();

                        string? initText = OpenAiCompletionInitTexts.GetFromName(role);
                        if (initText != null)
                        {
                            aiSession.Session.InitWithText(initText);
                            await session.SendGroupMessageAsync(context.GroupId, new CqMessage()
                            {
                                new CqAtMsg(context.UserId),
                                new CqTextMsg($"> 角色已更新:\n{initText}")
                            });
                        }
                        else
                        {
                            await session.SendGroupMessageAsync(context.GroupId, new CqMessage()
                            {
                                new CqAtMsg(context.UserId),
                                new CqTextMsg($"> 找不到执行的角色")
                            });
                        }
                    }
                    else if (msgTxt.StartsWith("#custom-role:", StringComparison.OrdinalIgnoreCase) ||
                             msgTxt.StartsWith("#custom-role ", StringComparison.OrdinalIgnoreCase))
                    {
                        string initText = msgTxt.Substring(13);
                        aiSession.Session.InitWithText(initText);
                        await session.SendGroupMessageAsync(context.GroupId, new CqMessage()
                            {
                                new CqAtMsg(context.UserId),
                                new CqTextMsg($"> 角色已更新:\n{initText}")
                            });
                        aiSession.Session.Reset();
                    }
                    else if (msgTxt.StartsWith("#history", StringComparison.OrdinalIgnoreCase))
                    {
                        CqMessage message = new CqMessage()
                        {
                            new CqAtMsg(context.UserId),
                            new CqTextMsg($"> 历史记录: {aiSession.Session.History.Count}条")
                        };

                        bool inWhiteList = false;
                        if (appConfig.AllowList != null && appConfig.AllowList.Contains(context.UserId))
                            inWhiteList = true;

                        if (!inWhiteList)
                            message.Add(new CqTextMsg($"(您的聊天会话最多保留 {maxHistoryCount} 条消息)"));

                        await session.SendGroupMessageAsync(context.GroupId, message);
                    }
                    else
                    {
                        bool dequeue = false;
                        if (aiSession.Session.History.Count > maxHistoryCount && (appConfig.AllowList == null || !appConfig.AllowList.Contains(context.UserId)))
                            dequeue = true;

                        if (dequeue)
                            while (aiSession.Session.History.Count > maxHistoryCount)
                                aiSession.Session.History.Dequeue();

                        try
                        {
                            Result<string, string> result =
                                await aiSession.Session.AskAsync(context.Message.Text);

                            switch (result)
                            {
                                case OkResult<string, string> ok:
                                    CqMessage message = new CqMessage()
                                    {
                                        new CqAtMsg(context.UserId),
                                        new CqTextMsg(ok.Value),
                                    };

                                    if (dequeue)
                                        message.WithTail($"(消息历史记录超过 {maxHistoryCount} 条, 已删去多余的历史记录)");

                                    await session.SendGroupMessageAsync(context.GroupId, message);

                                    aiSession.AddUsage();
                                    break;
                                case ErrResult<string, string> err:
                                    await session.SendGroupMessageAsync(context.GroupId, new CqMessage()
                                    {
                                        new CqAtMsg(context.UserId),
                                        new CqTextMsg($"(请求失败, 请重新尝试, 你也可以使用 #reset 重置机器人. {err.Value})")
                                    });
                                    break;

                            }
                        }
                        catch (Exception ex)
                        {
                            await session.SendGroupMessageAsync(context.GroupId, new CqMessage()
                                {
                                    new CqAtMsg(context.UserId),
                                    new CqTextMsg("(请求失败, 请重新尝试, 你也可以使用 #reset 重置机器人)")
                                });

                            await Console.Out.WriteLineAsync($"Exception: {ex}");
                        }
                    }
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
    }


    /// <summary>
    /// 异常了捏~
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        Console.WriteLine("出现了不可预知的异常");
        if (e.ExceptionObject is Exception ex)
        {
            Console.WriteLine(ex);
        }
    }
}