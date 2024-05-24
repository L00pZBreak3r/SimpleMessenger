using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Collections.Concurrent;
using SimpleMessengerModelLibrary.Models;
using SimpleMessengerDataLibrary;
using SimpleMessengerDataLibrary.Database;
using SimpleMessengerGrpcService.Protos;
using Google.Protobuf.WellKnownTypes;

using Dapper;


namespace SimpleMessengerGrpcService.Helpers;

internal static class DatabaseHelper
{
    private static readonly SimpleMessengerDBConfiguration DatabaseConfiguration = new SimpleMessengerDBConfiguration();
    private static SimpleMessengerDBContext DatabaseContext => DatabaseConfiguration.SimpleMessengerDB;

    private static readonly ConcurrentDictionary<WebSocket, long> mUserWebSockets = new();

    static DatabaseHelper()
    {
        DatabaseConfiguration.AddRandomUsers();
    }

    public static Int64Value GetUserId(StringValue aRequest)
    {
        var aReply = new Int64Value()
        {
            Value = DatabaseContext.TableUsers.FirstOrDefault(a => a.Name.Equals(aRequest.Value, StringComparison.CurrentCultureIgnoreCase))?.Id ?? 0L
        };
        return aReply;
    }

    public static UsersReply GetUsers()
    {
        var aReply = new UsersReply();
        aReply.Users.AddRange(DatabaseContext.TableUsers.Select(RpcMessageHelper.CreateUserReply));
        return aReply;
    }

    public static SendTextMessageReply AddTextMessage(SendTextMessageRequest aRequest)
    {
        string aText = aRequest.Text;
        int aNumber = aRequest.Number;
        long aId = 0L;
        DateTime aTimeStamp = default;

        string aError = string.Empty;
        if (string.IsNullOrWhiteSpace(aText))
            aError = "Message is empty";
        else
        {
            var aSender = DatabaseContext.TableUsers.FirstOrDefault(a => a.Id == aRequest.SenderId);
            if (aSender == null)
                aError = "Sender is not found";
            else
            {
                var aReceiver = DatabaseContext.TableUsers.FirstOrDefault(a => a.Id == aRequest.ReceiverId);
                if (aReceiver == null)
                    aError = "Receiver is not found";
                else
                {
                    aTimeStamp = DateTime.UtcNow;
                    var aTextMessage = new TextMessage()
                    {
                        Text = aText,
                        Number = aNumber,
                        TimeStamp = aTimeStamp,
                        Sender = aSender,
                        Receiver = aReceiver,
                        SenderId = aSender.Id,
                        ReceiverId = aReceiver.Id
                    };
                    try
                    {
                        DatabaseContext.AddTextMessage(aTextMessage);
                        aId = aTextMessage.Id;

                        foreach (var aWebSocketPair in mUserWebSockets.Where(a => a.Value == aRequest.SenderId))
                        {
                            var aReply = RpcMessageHelper.CreateTextMessageReply(aTextMessage, aRequest.SenderId);
                            var aReplyBytes = new ArraySegment<byte>(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(aReply)));
                            aWebSocketPair.Key.SendAsync(aReplyBytes, WebSocketMessageType.Text, true, CancellationToken.None);
                        }
                        foreach (var aWebSocketPair in mUserWebSockets.Where(a => a.Value == aRequest.ReceiverId))
                        {
                            var aReply = RpcMessageHelper.CreateTextMessageReply(aTextMessage, aRequest.ReceiverId);
                            var aReplyBytes = new ArraySegment<byte>(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(aReply)));
                            aWebSocketPair.Key.SendAsync(aReplyBytes, WebSocketMessageType.Text, true, CancellationToken.None);
                        }
                    }
                    catch (Exception e)
                    {
                        aError = (e.InnerException != null) ? e.InnerException.Message : e.Message;
                        DatabaseContext.RemoveTextMessage(aTextMessage);
                    }
                }
            }
        }

        var aSendTextMessageReply = new SendTextMessageReply()
        {
            Id = aId,
            TimeStamp = Timestamp.FromDateTime(DateTime.SpecifyKind(aTimeStamp, DateTimeKind.Utc)),
            Error = aError
        };
        return aSendTextMessageReply;
    }

    public static TextMessageRangeReply GetTextMessagesFromRange(TextMessageRangeRequest aRequest)
    {
        long aUserId = (DatabaseContext.TableUsers.Any(a => a.Id == aRequest.UserId)) ? aRequest.UserId : 0L;

        var aReply = new TextMessageRangeReply();
        if (aUserId > 0L)
        {
            DateTime aStartTimeStamp = aRequest.Start.ToDateTime();
            DateTime aEndTimeStamp = aRequest.End.ToDateTime();
            aReply.Messages.AddRange(DatabaseContext.TableTextMessages.Where(a => ((a.SenderId == aUserId) || (a.ReceiverId == aUserId)) && (a.TimeStamp >= aStartTimeStamp) && (a.TimeStamp < aEndTimeStamp)).Select(a => RpcMessageHelper.CreateTextMessageReply(a, aUserId)));
        }
        return aReply;
    }

    public static FullDumpReply GetFullDump()
    {
        var aReply = new FullDumpReply();
        var connection = DatabaseContext.ConnectionInstance;

        var res1 = connection.Query<User>("SELECT * FROM Users");
        User[] aUserList = res1.ToArray();
        var res2 = connection.Query<TextMessage>("SELECT * FROM TextMessages");
        TextMessage[] aMessageList = res2.ToArray();

        aReply.Users.AddRange(aUserList.Select(RpcMessageHelper.CreateUserReply));
        aReply.Messages.AddRange(aMessageList.Select(RpcMessageHelper.CreateTextMessageRawReply));
        return aReply;
    }

    private static async Task ProcessWebSocket(WebSocket webSocket)
    {
        var buffer = new byte[128];
        var receiveResult = await webSocket.ReceiveAsync(
            new ArraySegment<byte>(buffer), CancellationToken.None);

        while (!receiveResult.CloseStatus.HasValue)
        {
            string aUserIdString = Encoding.UTF8.GetString(buffer, 0, receiveResult.Count);
            if (long.TryParse(aUserIdString, out long aUserId))
            {
                mUserWebSockets[webSocket] = aUserId;
            }

            receiveResult = await webSocket.ReceiveAsync(
                new ArraySegment<byte>(buffer), CancellationToken.None);
        }

        await webSocket.CloseAsync(
            receiveResult.CloseStatus.Value,
            receiveResult.CloseStatusDescription,
            CancellationToken.None);
    }

    public static async Task WebSocketHandler(HttpContext aContext, RequestDelegate aNext)
    {
        if (aContext.Request.Path == "/ws")
        {
            if (aContext.WebSockets.IsWebSocketRequest)
            {
                using var aNewWebSocket = await aContext.WebSockets.AcceptWebSocketAsync();
                await ProcessWebSocket(aNewWebSocket);
                mUserWebSockets.TryRemove(aNewWebSocket, out _);
            }
            else
            {
                aContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            }
        }
        else
        {
            await aNext(aContext);
        }

    }
}
