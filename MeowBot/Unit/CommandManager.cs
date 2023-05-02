using System.Text;
using Collapsenav.Net.Tool;
using EleCho.GoCqHttpSdk;
using EleCho.GoCqHttpSdk.Message;
using EleCho.GoCqHttpSdk.Post;
using RustSharp;

namespace MeowBot;

internal class CommandManager
{
    private readonly AppConfig config;
    private readonly CqWsSession session;
    public IEnumerable<Command> Commands { get; private set; }
    public CommandManager(AppConfig config, CqWsSession session)
    {
        var commandTypes = AppDomain.CurrentDomain.GetCustomerTypes<Command>().Where(item => !item.IsAbstract);
        if (commandTypes.NotEmpty())
        {
            var normalCmds = commandTypes.Where(type => type != typeof(HelpCommand));
            Commands = normalCmds!.Select(type =>
            {
                var cmd = (Activator.CreateInstance(type, config, session) as Command)!;
                return cmd;
            });
            if (commandTypes.Any(type => type == typeof(HelpCommand)))
            {
                Commands = Commands.Append(new HelpCommand(config, session, Commands.ToList()));
            }
        }
        this.config = config;
        this.session = session;
    }

    public async Task ExecAsync(CqGroupMessagePostContext context, IOpenAiComplection aiSession)
    {
        // 执行设置的command命令
        foreach (var cmd in Commands)
        {
            if (await cmd.ExecAsync(context, aiSession))
                return;
        }
    }
}