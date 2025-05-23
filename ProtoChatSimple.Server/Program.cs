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
IActorRef chatHistoryActor = actorSystem.ActorOf(Props.Create(() => new ChatHistoryActor()));
IActorRef chatRoomActor = actorSystem.ActorOf(Props.Create(() => new ChatRoomActor(chatHistoryActor)));

builder.Services.AddSingleton(actorSystem);
builder.Services.AddSingleton(chatRoomActor);

// Add services to the container.
builder.Services.AddGrpc();
builder.Services.AddSingleton<ChatServiceImpl>(provider =>
    new ChatServiceImpl(
        provider.GetRequiredService<ActorSystem>(),
        provider.GetRequiredService<IActorRef>(),
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