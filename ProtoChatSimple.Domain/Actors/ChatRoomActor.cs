using Akka.Actor;
using ProtoChatSimple.Proto;

namespace ProtoChatSimple.Domain.Actors;

public class ChatRoomActor : ReceiveActor
{
    private readonly HashSet<IActorRef> _clients = new();
    private IActorRef _chatHistoryActor;
    
    public ChatRoomActor(IActorRef chatHistoryActor)
    {
        _chatHistoryActor = chatHistoryActor;
        
        Receive<Join>(async msg =>
        {
            _clients.Add(msg.Client);
            Context.Watch(msg.Client);
            
            List<ChatMessage> history = await _chatHistoryActor.Ask<List<ChatMessage>>(new ChatHistoryActor.GetHistory());

            foreach (ChatMessage message in history)
            {
                msg.Client.Tell(message);
            }
        });

        Receive<Broadcast>(msg =>
        {
            _chatHistoryActor.Tell(new ChatHistoryActor.SaveMessage(msg.Message));
            
            foreach (IActorRef client in _clients)
            {
                client.Tell(msg.Message);
            }
        });
        
        Receive<ClientDisconnected>(msg =>
        {
            _clients.Remove(msg.Client);
        });
    }

    protected override SupervisorStrategy SupervisorStrategy()
    {
        return new OneForOneStrategy(
            maxNrOfRetries: 5,
            withinTimeRange: TimeSpan.FromMinutes(1),
            decider: Decider.From(exception =>
            {
                switch (exception)
                {
                    case IOException:
                        Console.WriteLine("[Supervisor] Restarting ChatHistoryActor due to IOException.");
                        return Directive.Restart;

                    case TimeoutException:
                        Console.WriteLine("[Supervisor] Resuming after TimeoutException.");
                        return Directive.Resume;

                    default:
                        Console.WriteLine("[Supervisor] Stopping actor due to unknown error: " + exception.Message);
                        return Directive.Stop;
                }
            })
        );
    }

    public static Props Create(IActorRef chatHistoryActor) =>
        Akka.Actor.Props.Create(() => new ChatRoomActor(chatHistoryActor));
    
    public sealed record Join(IActorRef Client);
    public sealed record Broadcast(ChatMessage Message);
    public sealed record ClientDisconnected(IActorRef Client);
}