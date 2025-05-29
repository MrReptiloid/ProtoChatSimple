using Akka.Actor;
using Akka.Event;
using Akka.Persistence;
using ProtoChatSimple.Proto;

namespace ProtoChatSimple.Domain.Actors;

public class ChatHistoryActor : ReceivePersistentActor
{
    private readonly List<ChatMessage> _chatHistory = new();
    public override string PersistenceId { get; } = "chat-history-actor";

    public ChatHistoryActor()
    {
        Recover<ChatMessage>(msg =>
        {
            _chatHistory.Add(msg);
        });
        
        Command<SaveMessage>(cmd =>
        {
            Persist(cmd.Message, msg =>
            {
                _chatHistory.Add(msg);
                SaveSnapshot(_chatHistory);
            });
        });

        Command<GetHistory>(_ =>
        {
            Sender.Tell(_chatHistory);
        });
        
        Command<SaveSnapshotSuccess>(msg =>
        {
            Context.GetLogger().Info($"Snapshot saved successfully: {msg.Metadata}");
        });

        Command<SaveSnapshotFailure>(msg =>
        {
            Context.GetLogger().Warning($"Snapshot failed: {msg.Metadata}, reason: {msg.Cause.Message}");
        });
        
        Recover<SnapshotOffer>(offer =>
        {
            if (offer.Snapshot is List<ChatMessage> snapshot)
            {
                _chatHistory.Clear();
                _chatHistory.AddRange(snapshot);
            }
        });
    }
    
    public static Props Create() => 
        Akka.Actor.Props.Create(() => new ChatHistoryActor());
    
    public record SaveMessage(ChatMessage Message);
    public class GetHistory { }
}