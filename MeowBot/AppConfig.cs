using Collapsenav.Net.Tool;

namespace MeowBot
{
    internal class AppConfig
    {
        public static AppConfig? CurrentConfig;
        /// <summary>
        /// 默认的配置文件名称
        /// </summary>
        public const string Filename = "AppConfig.json";
        /// <summary>
        /// openai 的 apikey
        /// </summary>
        public string? ApiKey { get; set; } = string.Empty;
        /// <summary>
        /// bot机器人的uri
        /// </summary>
        public string? BotWebSocketUri { get; set; } = string.Empty;

        public string? ChatCompletionApiUrl { get; set; } = null;
        public string? TextCompletionApiUrl { get; set; } = null;
        public string GptModel { get; set; } = "gpt-3.5-turbo";
        public int UsageLimitTime { get; set; } = 300;
        public int UsageLimitCount { get; set; } = 5;
        /// <summary>
        /// 白名单
        /// </summary>
        public long[] AllowList { get; set; } = Array.Empty<long>();
        /// <summary>
        /// 黑名单
        /// </summary>
        public long[] BlockList { get; set; } = Array.Empty<long>();
        /// <summary>
        /// 管理员名单
        /// </summary>
        public long[] AdminList { get; set; } = Array.Empty<long>();
        /// <summary>
        /// 群设置
        /// </summary>
        public GroupConfig[] GroupConfigs { get; set; } = Array.Empty<GroupConfig>();
        public static string DefaultChatCompletionApiUrl { get; } = "https://api.openai.com/v1/chat/completions";
        public static string DefaultGptModel { get; } = "gpt-3.5-turbo";

        public bool CheckBotSocket()
        {
            if (BotWebSocketUri.NotEmpty())
                return true;
            Console.WriteLine("请指定机器人 WebSocket URI");
            Utils.PressAnyKeyToContinue();
            return false;
        }

        public bool CheckApiKey()
        {
            if (ApiKey.NotEmpty())
                return true;
            Console.WriteLine("请指定机器人 API Key");
            Utils.PressAnyKeyToContinue();
            return false;
        }


        /// <summary>
        /// 将设置保存到配置文件中
        /// </summary>
        public void SaveConfig()
        {
            this.ToJson().ToBytes().SaveTo(Filename);
        }
        /// <summary>
        /// 加白名单
        /// </summary>
        public void AddAllowList(params long[] userIds)
        {
            var list = AllowList.ToList();
            list.AddRange(userIds);
            AllowList = list.Unique().ToArray();
            SaveConfig();
        }
        /// <summary>
        /// 加黑名单
        /// </summary>
        public void AddBlockList(params long[] userIds)
        {
            var list = BlockList.ToList();
            list.AddRange(userIds);
            BlockList = list.Unique().ToArray();
            SaveConfig();
        }
        /// <summary>
        /// 加管理员名单
        /// </summary>
        public void AddAdminList(params long[] userIds)
        {
            var list = AdminList.ToList();
            list.AddRange(userIds);
            AdminList = list.Unique().ToArray();
            SaveConfig();
        }
        /// <summary>
        /// 设置群bot角色
        /// </summary>
        public void SetGroupRole(long groupId, string role)
        {
            var groups = GroupConfigs.ToList();
            if (groups.Any(item => item.GroupId == groupId))
                groups.FirstOrDefault(item => item.GroupId == groupId)!.Role = role;
            else
                groups.Add(new GroupConfig { GroupId = groupId, Role = role });
            GroupConfigs = groups.ToArray();
            SaveConfig();
        }

        public OpenAiChatCompletionSession CreateNewSession()
        {
            return new OpenAiChatCompletionSession(
                ApiKey!,
                ChatCompletionApiUrl ?? DefaultChatCompletionApiUrl,
                GptModel ?? DefaultGptModel
            );
        }

        public GroupConfig? GetGroupConfig(long groupId)
        {
            return GroupConfigs.FirstOrDefault(item => item.GroupId == groupId);
        }
    }
}
