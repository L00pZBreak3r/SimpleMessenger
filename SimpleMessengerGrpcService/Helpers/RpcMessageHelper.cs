using SimpleMessengerModelLibrary.Models;
using SimpleMessengerGrpcService.Protos;
using Google.Protobuf.WellKnownTypes;

namespace SimpleMessengerGrpcService.Helpers;

internal static class RpcMessageHelper
{
    public static UserReply CreateUserReply(User aItem)
    {
        var aUserReply = new UserReply()
        {
            Id = aItem.Id,
            Name = aItem.Name,
        };
        return aUserReply;
    }

    public static TextMessageReply CreateTextMessageReply(TextMessage aItem, long aUserId)
    {
        long OtherUserId;
        string OtherUserName;
        bool aIsMine = aUserId == aItem.SenderId;
        if (aIsMine)
        {
            OtherUserId = aItem.ReceiverId;
            OtherUserName = aItem.Receiver?.Name ?? string.Empty;
        }
        else
        {
            OtherUserId = aItem.SenderId;
            OtherUserName = aItem.Sender?.Name ?? string.Empty;
        }

        var aTextMessageReply = new TextMessageReply()
        {
            Id = aItem.Id,
            TimeStamp = Timestamp.FromDateTime(DateTime.SpecifyKind(aItem.TimeStamp, DateTimeKind.Utc)),
            Text = aItem.Text,
            Number = aItem.Number,
            OtherUserId = OtherUserId,
            OtherUserName = OtherUserName,
            IsMine = aIsMine
        };
        return aTextMessageReply;
    }

    public static TextMessageRawReply CreateTextMessageRawReply(TextMessage aItem)
    {
        var aTextMessageRawReply = new TextMessageRawReply()
        {
            Id = aItem.Id,
            TimeStamp = Timestamp.FromDateTime(DateTime.SpecifyKind(aItem.TimeStamp, DateTimeKind.Utc)),
            Text = aItem.Text,
            Number = aItem.Number,
            SenderId = aItem.SenderId,
            ReceiverId = aItem.ReceiverId
        };
        return aTextMessageRawReply;
    }
}
