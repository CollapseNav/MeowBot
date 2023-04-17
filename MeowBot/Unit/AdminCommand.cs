using EleCho.GoCqHttpSdk;

namespace MeowBot;

internal abstract class AdminCommand : Command
{
    protected AdminCommand(AppConfig config, CqWsSession session) : base(config, session) { }
}