using Collapsenav.Net.Tool;
using EleCho.GoCqHttpSdk;
using EleCho.GoCqHttpSdk.Message;
using EleCho.GoCqHttpSdk.Post;

namespace MeowBot;

public static class MsgExt
{
    public static async Task SendGroupMsgAsync(this CqWsSession session, CqGroupMessagePostContext context, params string[] msgs)
    {
        await session.SendGroupMsgAsync(context.GroupId, context.UserId, msgs);
    }
    public static async Task SendGroupMsgAsync(this CqWsSession session, long groupId, long userId, params string[] msgs)
    {
        CqMessage message = new() { new CqAtMsg(userId) };
        msgs.ForEach(msg => message.Add(new CqTextMsg(msg)));
        await session.SendGroupMessageAsync(groupId, message);
    }
}