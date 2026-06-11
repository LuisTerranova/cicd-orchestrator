using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Orchestrator.Runner.Agent;
using Orchestrator.Runner.Configuration;

namespace Orchestrator.Runner.Messaging;

public static class MassTransitSetup
{
    public static void Configure(IServiceCollection services, RunnerOptions options)
    {
        services.AddMassTransit(bus =>
        {
            bus.SetKebabCaseEndpointNameFormatter();

            bus.AddConsumer<JobConsumer>();
            bus.AddConsumer<CancellationConsumer>();

            bus.UsingRabbitMq(
                (ctx, cfg) =>
                {
                    cfg.Host(
                        new Uri($"rabbitmq://{options.RabbitMqHost}:5672"),
                        h =>
                        {
                            h.Username(options.RabbitMqUser);
                            h.Password(options.RabbitMqPass);
                        }
                    );

                    cfg.ReceiveEndpoint(
                        $"jobs.runner.{options.Name}",
                        e =>
                        {
                            e.PrefetchCount = (ushort)options.Concurrency;
                            e.ConfigureConsumeTopology = false;

                            e.Consumer<JobConsumer>(ctx);
                            e.Consumer<CancellationConsumer>(ctx);

                            e.Bind(
                                "jobs",
                                x =>
                                {
                                    x.RoutingKey = $"job.{options.Name}";
                                    x.ExchangeType = "topic";
                                }
                            );
                        }
                    );
                }
            );
        });
    }
}
