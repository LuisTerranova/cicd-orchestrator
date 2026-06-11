using Orchestrator.Application.Common;
using Orchestrator.Domain.Interfaces;

namespace Orchestrator.Application.Pipelines;

public sealed class DeletePipelineHandler(
    IPipelineRepository pipelines
) : ICommandHandler<DeletePipelineCommand>
{
    public async Task HandleAsync(
        DeletePipelineCommand command,
        CancellationToken ct = default
    )
    {
        var pipeline = await pipelines.GetByIdAsync(command.Id, ct)
            ?? throw new InvalidOperationException($"Pipeline {command.Id} not found");

        await pipelines.DeleteAsync(pipeline, ct);
    }
}
