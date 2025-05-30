using Akka.Actor;
using Grpc.Core;
using ProtoChatSimple.Proto;

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
    
    public static Props Create(IServerStreamWriter<ChatMessage> stream) => 
        Akka.Actor.Props.Create(() => new ClientActor(stream));
}