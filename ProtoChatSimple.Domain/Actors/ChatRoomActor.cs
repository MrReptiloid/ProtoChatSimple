using Akka.Actor;

namespace ProtoChatSimple.Domain.Actors;

public class ChatRoomActor : ReceiveActor
{
    private readonly HashSet<IActorRef> _clients = new();
    private readonly IActorRef _chatHistoryActor;
    
    public ChatRoomActor(IActorRef chatHistoryActor)
    {
        _chatHistoryActor = chatHistoryActor;
        
        Receive<Join>(async msg =>
        {
            _clients.Add(msg.Client);

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
    }

    public record Join(IActorRef Client);
    public record Broadcast(ChatMessage Message);
}