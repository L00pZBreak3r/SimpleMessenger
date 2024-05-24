using SimpleMessengerGrpcService.Protos;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

using SimpleMessengerGrpcService.Helpers;

namespace SimpleMessengerGrpcService.Services;

public class SimpleMessengerService : SimpleMessenger.SimpleMessengerBase
{
    private readonly ILogger<SimpleMessengerService> _logger;
    public SimpleMessengerService(ILogger<SimpleMessengerService> logger)
    {
        _logger = logger;
    }


    public override Task<UsersReply> GetUsers(Empty request, ServerCallContext context)
    {
        return Task.FromResult(DatabaseHelper.GetUsers());
    }

    public override Task<Int64Value> GetUserId(StringValue request, ServerCallContext context)
    {
        return Task.FromResult(DatabaseHelper.GetUserId(request));
    }

    public override Task<SendTextMessageReply> SendTextMessage(SendTextMessageRequest request, ServerCallContext context)
    {
        return Task.FromResult(DatabaseHelper.AddTextMessage(request));
    }

    public override Task<TextMessageRangeReply> GetTextMessagesFromRange(TextMessageRangeRequest request, ServerCallContext context)
    {
        return Task.FromResult(DatabaseHelper.GetTextMessagesFromRange(request));
    }

    public override Task<FullDumpReply> GetFullDump(Empty request, ServerCallContext context)
    {
        return Task.FromResult(DatabaseHelper.GetFullDump());
    }
}
