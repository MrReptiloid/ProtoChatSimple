using Akka.Actor;
using Akka.Cluster;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ProtoChatSimple.Domain;

public class AkkaClusterHealthCheck : IHealthCheck
{
    private readonly ActorSystem _actorSystem;

    public AkkaClusterHealthCheck(ActorSystem actorSystem)
    {
        _actorSystem = actorSystem;
    }
    
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        Cluster cluster = Cluster.Get(_actorSystem);
        MemberStatus status = cluster.SelfMember.Status;

        return status == MemberStatus.Up
            ? Task.FromResult(HealthCheckResult.Healthy("Cluster node is UP"))
            : Task.FromResult(HealthCheckResult.Unhealthy($"Cluster node is status: {status}"));
    }
}