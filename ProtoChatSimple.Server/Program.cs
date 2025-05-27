using Akka.Actor;
using Akka.Cluster.Tools.Singleton;
using Akka.Configuration;
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

Config akkaConfig = ConfigurationFactory.ParseString(File.ReadAllText("akka.conf")); 

ActorSystem actorSystem = ActorSystem.Create("ChatSystem", akkaConfig);
IActorRef clientManagerActor = actorSystem.ActorOf(ClientManagerActor.Create());

(IActorRef? chatRoomProxy, IActorRef? chatHistoryProxy) = InitClusterSingletons(actorSystem);

builder.Services.AddSingleton(actorSystem);
builder.Services.AddSingleton(chatRoomProxy);
builder.Services.AddSingleton(chatHistoryProxy);
builder.Services.AddSingleton(clientManagerActor);


builder.Services.AddSingleton<ChatServiceImpl>(provider =>
    new ChatServiceImpl(
        chatRoomProxy,
        clientManagerActor,
        provider.GetRequiredService<ILogger<ChatServiceImpl>>()
    ));

builder.Services.AddGrpc();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<GreeterService>();
app.MapGrpcService<ChatServiceImpl>();
app.MapGet("/",
    () =>
        "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();

static (IActorRef chatRoomProxy, IActorRef chatHistoryProxy) InitClusterSingletons(ActorSystem system)
{
    Props historySingletonProps = ClusterSingletonManager.Props(
        ChatHistoryActor.Create(),
        PoisonPill.Instance,
        ClusterSingletonManagerSettings.Create(system).WithRole("chat-server")
    );
    
    system.ActorOf(historySingletonProps, "chatHistorySingleton");
    
    IActorRef chatHistoryProxy = system.ActorOf(
        ClusterSingletonProxy.Props(
            "/user/chatHistorySingleton",
            ClusterSingletonProxySettings.Create(system).WithRole("chat-server")
        ),
        "chatHistoryProxy"
    );

    Props roomSingletonProps = ClusterSingletonManager.Props(
        ChatRoomActor.Create(chatHistoryProxy),  // proxy
        PoisonPill.Instance,
        ClusterSingletonManagerSettings.Create(system).WithRole("chat-server")
    );
    
    system.ActorOf(roomSingletonProps, "chatRoomSingleton");

    IActorRef chatRoomProxy = system.ActorOf(
        ClusterSingletonProxy.Props(
            "/user/chatRoomSingleton",
            ClusterSingletonProxySettings.Create(system).WithRole("chat-server")
        ),
        "chatRoomProxy"
    );

    return (chatRoomProxy, chatHistoryProxy);
}
