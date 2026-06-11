using Orchestrator.Domain.Exceptions;
using Orchestrator.Domain.Interfaces;

namespace Orchestrator.Domain.Services;

public sealed class DagEngine : IDagEngine
{
    public DagResult BuildLayers(List<StageDefinition> stages)
    {
        var stageNames = stages.Select(s => s.Name).ToHashSet();
        var adjacency = stages.ToDictionary(s => s.Name, s => s.DependsOn.ToList());

        foreach (var (name, deps) in adjacency)
        {
            foreach (var dep in deps)
            {
                if (!stageNames.Contains(dep))
                    throw new DomainException(
                        $"Stage '{name}' depends on unknown stage '{dep}'."
                    );
            }
        }

        var topoOrder = TopologicalSort(adjacency);
        var layers = ComputeLayers(topoOrder, adjacency);
        var orderedStages = topoOrder
            .Select(name => stages.First(s => s.Name == name))
            .ToList();

        var stageLayers = layers
            .Select(layer => layer.Select(name => stages.First(s => s.Name == name)).ToList())
            .ToList();

        return new DagResult(stageLayers, orderedStages);
    }

    private static List<string> TopologicalSort(Dictionary<string, List<string>> graph)
    {
        var inDegree = graph.ToDictionary(kv => kv.Key, kv => kv.Value.Count);

        var dependents = new Dictionary<string, List<string>>();
        foreach (var (node, deps) in graph)
        {
            foreach (var dep in deps)
            {
                if (!dependents.ContainsKey(dep))
                    dependents[dep] = new List<string>();
                dependents[dep].Add(node);
            }
        }

        var queue = new Queue<string>(
            inDegree.Where(kv => kv.Value == 0).Select(kv => kv.Key)
        );
        var result = new List<string>();

        while (queue.Count > 0)
        {
            var node = queue.Dequeue();
            result.Add(node);

            if (dependents.TryGetValue(node, out var affected))
            {
                foreach (var dependent in affected)
                {
                    inDegree[dependent]--;
                    if (inDegree[dependent] == 0)
                        queue.Enqueue(dependent);
                }
            }
        }

        if (result.Count != graph.Count)
        {
            var cycle = string.Join(" → ", graph.Keys.Except(result));
            throw new DomainException($"Circular dependency detected: {cycle}");
        }

        return result;
    }

    private static List<List<string>> ComputeLayers(
        List<string> topoOrder,
        Dictionary<string, List<string>> graph
    )
    {
        var depth = new Dictionary<string, int>();
        var layers = new List<List<string>>();

        foreach (var node in topoOrder)
        {
            if (graph[node].Count == 0)
            {
                depth[node] = 0;
            }
            else
            {
                depth[node] = graph[node].Max(d => depth[d]) + 1;
            }

            while (layers.Count <= depth[node])
                layers.Add([]);

            layers[depth[node]].Add(node);
        }

        return layers;
    }
}
