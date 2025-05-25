using System.Text.Json;
using Akka.Actor;
using ProtoChatSimple.Proto;

namespace ProtoChatSimple.Domain.Actors;

public class ChatHistoryActor : ReceiveActor
{
    private readonly List<ChatMessage> _chatHistory = new();
    private readonly string _filepath = "chat_history.json";

    public ChatHistoryActor()
    {
        Console.WriteLine(Path.GetFullPath(_filepath));
        if (File.Exists(_filepath))
        {
            string json = File.ReadAllText(_filepath);
            _chatHistory = JsonSerializer.Deserialize<List<ChatMessage>>(json) ?? new List<ChatMessage>();
        }

        Receive<GetHistory>(_ => Sender.Tell(_chatHistory.ToList()));

        Receive<SaveMessage>(msg =>
        {
            _chatHistory.Add(msg.Message);
            SaveToFile();
        });
    }

    public void SaveToFile()
    {
        try
        {
            string json = JsonSerializer.Serialize(_chatHistory, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_filepath, json);
        }
        catch (UnauthorizedAccessException)
        {
            Console.WriteLine("Error: Access to the file is denied.");
        }
        catch (ArgumentException)
        {
            Console.WriteLine("Error: The file path is invalid.");
        }
        catch (PathTooLongException)
        {
            Console.WriteLine("Error: The file path is too long.");
        }
        catch (DirectoryNotFoundException)
        {
            Console.WriteLine("Error: The directory was not found.");
        }
        catch (IOException ex)
        {
            Console.WriteLine($"Error: An I/O error occurred. Details: {ex.Message}");
        }
        catch (NotSupportedException)
        {
            Console.WriteLine("Error: The file path format is not supported.");
        }
        catch (System.Security.SecurityException)
        {
            Console.WriteLine("Error: You do not have the required permissions.");
        }
    }
    
    public static Props Create() => 
        Akka.Actor.Props.Create(() => new ChatHistoryActor());
    
    public record GetHistory();
    public record SaveMessage(ChatMessage Message);
}