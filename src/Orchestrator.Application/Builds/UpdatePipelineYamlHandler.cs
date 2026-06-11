using Orchestrator.Application.Common;
using Orchestrator.Domain.Interfaces;

namespace Orchestrator.Application.Builds;

public sealed class UpdatePipelineYamlHandler(
    IPipelineRepository pipelines,
    IPipelineYamlParser yamlParser
) : ICommandHandler<UpdatePipelineYamlCommand>
{
    public async Task HandleAsync(
        UpdatePipelineYamlCommand command,
        CancellationToken ct = default
    )
    {
        var pipeline =
            await pipelines.GetByIdAsync(command.PipelineId, ct)
            ?? throw new InvalidOperationException($"Pipeline {command.PipelineId} not found");

        yamlParser.Parse(command.YamlContent);
        pipeline.UpdateYaml(command.YamlContent);
        await pipelines.UpdateAsync(pipeline, ct);
    }
}
