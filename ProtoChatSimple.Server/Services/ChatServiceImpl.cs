using Akka.Actor;
using Grpc.Core;
using Microsoft.AspNetCore.Connections;
using ProtoChatSimple.Domain.Actors;
using ProtoChatSimple.Proto;

namespace ProtoChatSimple.Server.Services;

public class ChatServiceImpl : Chat.ChatBase
{
    private readonly IActorRef _chatRoomActor;
    private readonly IActorRef _clientMangerActor;
    
    private readonly ILogger<ChatServiceImpl> _logger;

    public ChatServiceImpl(IActorRef chatRoomActor, IActorRef clientManagerActor, ILogger<ChatServiceImpl> logger)
    {
        _chatRoomActor = chatRoomActor;
        _clientMangerActor = clientManagerActor;

        _logger = logger;
    }

    public override async Task ChatStream(
        IAsyncStreamReader<ChatMessage> requestStream,
        IServerStreamWriter<ChatMessage> responseStream,
        ServerCallContext context)
    {
        var clientRegistered = await _clientMangerActor
            .Ask<ClientManagerActor.ClientRegistered>(
                new ClientManagerActor.RegisterClient(Guid.NewGuid().ToString(), responseStream));
        
        IActorRef clientActor = clientRegistered.Actor;
        _chatRoomActor.Tell(new ChatRoomActor.Join(clientActor));
        
        try
        {
            await foreach (ChatMessage request in requestStream.ReadAllAsync(context.CancellationToken))
            {
                _chatRoomActor.Tell(new ChatRoomActor.Broadcast(request));
            }
        }
        catch (IOException ex) when (ex.InnerException is ConnectionAbortedException)
        {
            _logger.LogInformation("HTTP/2 connection aborted: {Message}", ex.InnerException.Message);
        }
        catch (IOException ex)
        {
            _logger.LogWarning("Client closed the connection (IO): {Message}", ex.Message);
        }
        catch (ConnectionAbortedException ex)
        {
            _logger.LogInformation("HTTP/2 connection aborted: {Message}", ex.Message);
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.Cancelled)
        {
            _logger.LogInformation("gRPC call was cancelled (client disconnect)");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in ChatStream");
        }
        finally
        {
            _logger.LogInformation("Connection closed");
        }
    }
}