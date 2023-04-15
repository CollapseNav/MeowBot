using RustSharp;

namespace MeowBot
{
    public interface IOpenAiComplection
    {
        void InitWithText(string text);
        void Reset();
        Queue<KeyValuePair<string, string>> History { get; }
        Task<Result<string, string>> AskAsync(string content);
    }
}