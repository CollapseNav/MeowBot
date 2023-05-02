using System.Text;
using Collapsenav.Net.Tool;
using EleCho.GoCqHttpSdk;
using EleCho.GoCqHttpSdk.Post;

namespace MeowBot;

internal class HelpCommand : Command
{
    private readonly IEnumerable<Command> cmds;

    public HelpCommand(AppConfig config, CqWsSession session, List<Command> cmds) : base(config, session)
    {
        Prefix = "#help";
        Description = "帮助, 可查看其他命令具体用法, 例: #help role";
        StringBuilder helpText = new("(机器人指令)\n\n");
        this.cmds = cmds.AsEnumerable();
        cmds.Reverse();
        cmds.Add(this);
        cmds.Reverse();
        cmds.ForEach(cmd =>
        {
            helpText.AppendLine($"{cmd.Prefix.PadRight(12)}{cmd.Description}");
        });
        helpText.AppendLine($"注意, 普通用户最多保留 {AppConfig.MaxHistory} 条聊天记录, 多的会被删除, 也就是说, 机器人会逐渐忘记你");
        Help = helpText.ToString();
    }

    public override async Task<bool> ExecAsync(CqGroupMessagePostContext context, IOpenAiComplection aiSession)
    {
        var msg = context.Message.Text.Trim();
        if (!CheckPrefix(msg))
            return false;
        if (msg.Split(' ').Length == 2)
        {
            var cmd = msg.Split(' ').Last();
            var existedCmd = cmds.FirstOrDefault(c => c.Prefix == $"#{cmd}");
            if (existedCmd != null)
            {
                await session.SendGroupMsgAsync(context.GroupId, context.UserId, existedCmd.Help);
            }
            else
            {
                await session.SendGroupMsgAsync(context.GroupId, context.UserId, $"不存在的命令 {cmd}");
            }
        }
        else
            await session.SendGroupMsgAsync(context.GroupId, context.UserId, Help);
        return true;
    }
}