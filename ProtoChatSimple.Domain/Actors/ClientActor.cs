using Akka.Actor;
using Grpc.Core;

namespace ProtoChatSimple.Domain.Actors;

public class ClientActor : ReceiveActor
{
    private readonly IServerStreamWriter<ChatMessage> _stream;

    public ClientActor(IServerStreamWriter<ChatMessage> stream)
    {
        _stream = stream;

        Receive<ChatMessage>(async msg =>
        {
            try
            {
                await _stream.WriteAsync(msg);
            }
            catch (Exception ex)
            {
                Context.Stop(Self);
            }
        });
    }
}