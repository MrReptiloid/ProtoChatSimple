using System.Security.Cryptography;
using Grpc.Core;
using Grpc.Net.Client;
using ProtoChatSimple.Proto;

var httpHandler = new HttpClientHandler
{
    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
};

using var channel = GrpcChannel.ForAddress("https://localhost:5001", new GrpcChannelOptions
{
    HttpHandler = httpHandler
});
var client = new Chat.ChatClient(channel);
using var stream = client.ChatStream();

_ = Task.Run(async () =>
{
    await foreach (var msg in stream.ResponseStream.ReadAllAsync())
    {
        Console.WriteLine($"[{msg.User}]: {msg.Text}");
    }
});

string userId = "3";
if (args.Length > 0)
{
    userId = args[0];
}

while (true)
{
    string text = RandomNumberGenerator.GetInt32(0, 100).ToString();

    await stream.RequestStream.WriteAsync(new ChatMessage
    {
        User = userId,
        Text = text,
        Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
    });
    
    await Task.Delay(3000);
}    