using Akka.Actor;
using Grpc.Core;
using ProtoChatSimple.Proto;

namespace ProtoChatSimple.Domain.Actors;

public class ClientManagerActor : ReceiveActor
{
    private readonly Dictionary<IActorRef, string> _clients = new();

    public ClientManagerActor()
    {
        Receive<RegisterClient>(msg =>
        {
            IActorRef clientActor = Context.ActorOf(ClientActor.Create(msg.Stream), msg.User);
            
            Context.Watch(clientActor);
            _clients[clientActor] = msg.User;
            
            Sender.Tell(new ClientRegistered(clientActor));
        });
        Receive<Terminated>(t =>
        {
            if (_clients.Remove(t.ActorRef))
            {
                Context.Parent.Tell(new ChatRoomActor.ClientDisconnected(t.ActorRef));
            }
        });
    }

    public static Props Create() => 
        Akka.Actor.Props.Create(() => new ClientManagerActor());
    
    public sealed record RegisterClient(string User, IServerStreamWriter<ChatMessage> Stream);
    public sealed record ClientRegistered(IActorRef Actor);

}