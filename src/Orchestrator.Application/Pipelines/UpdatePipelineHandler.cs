using Orchestrator.Application.Common;
using Orchestrator.Domain.Interfaces;

namespace Orchestrator.Application.Pipelines;

public sealed class UpdatePipelineHandler(
    IPipelineRepository pipelines
) : ICommandHandler<UpdatePipelineCommand>
{
    public async Task HandleAsync(
        UpdatePipelineCommand command,
        CancellationToken ct = default
    )
    {
        var pipeline = await pipelines.GetByIdAsync(command.Id, ct)
            ?? throw new InvalidOperationException($"Pipeline {command.Id} not found");

        pipeline.UpdateDetails(command.Name, command.Repo, command.Branch, command.YamlPath);
        await pipelines.UpdateAsync(pipeline, ct);
    }
}
