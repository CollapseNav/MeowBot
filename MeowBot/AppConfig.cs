using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Collapsenav.Net.Tool;

namespace MeowBot
{
    internal class AppConfig
    {
        public const string Filename = "AppConfig.json";

        public string? ApiKey { get; set; } = string.Empty;
        public string? BotWebSocketUri { get; set; } = string.Empty;

        public string? ChatCompletionApiUrl { get; set; } = null;
        public string? TextCompletionApiUrl { get; set; } = null;

        public string GptModel { get; set; } = "gpt-3.5-turbo";

        public int UsageLimitTime { get; set; } = 300;
        public int UsageLimitCount { get; set; } = 5;

        public long[] AllowList { get; set; } = Array.Empty<long>();
        public long[] BlockList { get; set; } = Array.Empty<long>();
        public long[] GroupWhiteList { get; set; } = Array.Empty<long>();
        public long[] AdminList { get; set; } = Array.Empty<long>();
        public GroupConfig[] GroupConfigs { get; set; } = Array.Empty<GroupConfig>();

        public static string DefaultChatCompletionApiUrl { get; } = "https://api.openai.com/v1/chat/completions";
        public static string DefaultGptModel { get; } = "gpt-3.5-turbo";



        public void SaveConfig()
        {
            this.ToJson().ToBytes().SaveTo(Filename);
        }
        public void AddAllowList(params long[] userIds)
        {
            var list = AllowList.ToList();
            list.AddRange(userIds);
            AllowList = list.Unique().ToArray();
            SaveConfig();
        }

        public void AddBlockList(params long[] userIds)
        {
            var list = BlockList.ToList();
            list.AddRange(userIds);
            BlockList = list.Unique().ToArray();
            SaveConfig();
        }

        public void AddAdminList(params long[] userIds)
        {
            var list = AdminList.ToList();
            list.AddRange(userIds);
            AdminList = list.Unique().ToArray();
            SaveConfig();
        }
    }
}
