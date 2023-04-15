using Collapsenav.Net.Tool;
using EleCho.GoCqHttpSdk;
using EleCho.GoCqHttpSdk.Post;

namespace MeowBot;

internal class CommandManager
{
    public IEnumerable<Command> Commands { get; private set; }
    public CommandManager(AppConfig config, CqWsSession session)
    {
        var commandTypes = AppDomain.CurrentDomain.GetCustomerTypes<Command>();
        if (commandTypes.NotEmpty())
            Commands = commandTypes!.Select(type => (Activator.CreateInstance(type, config, session) as Command)!);
    }

    public async Task ExecAsync(CqGroupMessagePostContext context, IOpenAiComplection session)
    {
        foreach (var cmd in Commands)
        {
            if (await cmd.ExecAsync(context, session))
                break;
        }
    }
}