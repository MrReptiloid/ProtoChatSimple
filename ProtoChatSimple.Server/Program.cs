using Akka.Actor;
using ProtoChatSimple.Domain.Actors;
using ProtoChatSimple.Server.Services;


var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5001, listenOptions =>
    {
        listenOptions.UseHttps();
    });
});

ActorSystem actorSystem = ActorSystem.Create("ChatSystem");
IActorRef chatHistoryActor = actorSystem.ActorOf(ChatHistoryActor.Create());
IActorRef chatRoomActor = actorSystem.ActorOf(ChatRoomActor.Create(chatHistoryActor));
IActorRef clientManagerActor = actorSystem.ActorOf(ClientManagerActor.Create());

builder.Services.AddSingleton(actorSystem);
builder.Services.AddSingleton(chatRoomActor);
builder.Services.AddSingleton(chatHistoryActor);
builder.Services.AddSingleton(clientManagerActor);

// Add services to the container.
builder.Services.AddGrpc();
builder.Services.AddSingleton<ChatServiceImpl>(provider =>
    new ChatServiceImpl(
        chatRoomActor,
        clientManagerActor,
        provider.GetRequiredService<ILogger<ChatServiceImpl>>()
));

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<GreeterService>();
app.MapGrpcService<ChatServiceImpl>();
app.MapGet("/",
    () =>
        "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run(); 