using Collapsenav.Net.Tool;
using EleCho.GoCqHttpSdk;
using EleCho.GoCqHttpSdk.Message;

namespace MeowBot;

public static class MsgExt
{
    public static async Task SendGroupMsgAsync(this CqWsSession session, long groupId, long userId, params string[] msgs)
    {
        CqMessage message = new() { new CqAtMsg(userId) };
        msgs.ForEach(msg => message.Add(new CqTextMsg(msg)));
        await session.SendGroupMessageAsync(groupId, message);
    }
}