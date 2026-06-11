namespace Orchestrator.Domain.Interfaces;

public interface IDagEngine
{
    DagResult BuildLayers(List<StageDefinition> stages);
}

public sealed record DagResult(
    List<List<StageDefinition>> Layers,
    List<StageDefinition> TopologicalOrder
);
