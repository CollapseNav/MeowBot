// using System.Net.Http.Json;
// using Collapsenav.Net.Tool;
// using RustSharp;

// namespace MeowBot;

// internal class ChatCompletionSession : IOpenAiComplection
// {
//     private string initText;
//     private string davinciRole;
//     public AppConfig Config { get; private set; }
//     public ChatCompletionSession(AppConfig config)
//     {
//         Config = config;
//     }
//     public Queue<KeyValuePair<string, string>> History => new();

//     public async Task<Result<string, string>> AskAsync(string content)
//     {
//         List<ChatMessage> messages = new();
//         if (initText.NotEmpty())
//         {
//             messages.Add(new ChatMessage("system", initText));
//         }

//         if (History.NotEmpty())
//         {
//             History.ForEach(h =>
//             {
//                 messages.Add(new("user", h.Key));
//                 messages.Add(new(davinciRole.IsEmpty("assistant"), h.Value));
//             });
//         }

//         messages.Add(new("user", content));
//         Utils.GlobalHttpClient.PostAsJsonAsync(Config.ChatCompletionApiUrl, new { });
//     }

//     public void InitWithText(string text) => initText = text;

//     public void Reset() => History.Clear();
// }

// internal class ChatMessage
// {
//     public ChatMessage() { }

//     public ChatMessage(string role, string content)
//     {
//         this.role = role;
//         this.content = content;
//     }

//     public string role { get; set; }
//     public string content { get; set; }
// }