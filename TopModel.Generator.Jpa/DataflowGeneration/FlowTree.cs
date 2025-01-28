using TopModel.Core;

namespace TopModel.Generator.Jpa;

public class FlowTree
{
    public FlowTree(List<DataFlow> flows)
    {
        var hasIndependantFlow = Graps(flows).Count() > 1;
        RootFlows = hasIndependantFlow ? new() : flows.Where(f => !flows.Intersect(f.DependsOn).Any()).ToList();
        while (flows.Any(f => !Flows.Contains(f)))
        {
            var f = flows.Where(f => !Flows.Contains(f)).First();
            var subFlowDataFlows = new List<DataFlow>();
            var stack = new Queue<DataFlow>();
            stack.Enqueue(f);
            var flowsToAdd = new List<DataFlow>();
            while (stack.TryDequeue(out var s))
            {
                subFlowDataFlows.Add(s);
                foreach (var depFlow in flows.Where(f => !Flows.Concat(subFlowDataFlows).Contains(f)).Where(f => s.DependsOn.Contains(f) || f.DependsOn.Contains(s)))
                {
                    stack.Enqueue(depFlow);
                }
            }

            Subflows.Add(new FlowTree(subFlowDataFlows));
        }
    }

    // Flows dont toutes les dépendances sont déjà passées
    public List<DataFlow> RootFlows { get; set; } = new();

    // Arbres de dépendances
    public List<FlowTree> Subflows { get; set; } = new();

    // Tous les flows de l'arbre
    public List<DataFlow> Flows => RootFlows.Concat(Subflows.SelectMany(s => s.Flows)).Distinct().ToList();

    public string ToFlow(int indentLevel)
    {
        var baseIndent = "  ";
        var indent = baseIndent;
        for (int i = 0; i < indentLevel; i++)
        {
            indent += baseIndent;
        }

        if (Flows.Count == 1)
        {
            return $"{Flows.First().Name.ToCamelCase()}Flow";
        }

        var result = $" //\n{indent}new FlowBuilder<Flow>(\"{string.Join('-', Flows.Select(r => r.Name))}\")";
        var next = "start";
        if (RootFlows.Count == 1)
        {
            result += $" //\n{indent}{baseIndent}.start({RootFlows.First().Name.ToCamelCase()}Flow)";
            next = "next";
        }
        else if (RootFlows.Count > 1)
        {
            result += $" //\n{indent}{baseIndent}.start(new FlowBuilder<Flow>(\"{string.Join('-', RootFlows.Select(r => r.Name))}\")";
            next = "next";
            result += $" //\n{indent}{baseIndent}.split(taskExecutor)";
            result += $" //\n{indent}{baseIndent}.add(";
            foreach (var s in RootFlows)
            {
                result += $" //\n{indent}{baseIndent}{baseIndent}{s.Name.ToCamelCase()}Flow";
                if (RootFlows.IndexOf(s) != RootFlows.Count - 1)
                {
                    result += ",";
                }
            }

            result += $" //\n{indent}{baseIndent})";
            result += $" //\n{indent}.end()";
            result += $" //\n{indent})";
        }

        if (Subflows.Count == 1)
        {
            result += $"//\n{indent}{baseIndent}.{next}({Subflows.First().ToFlow(indentLevel + 2)})";
        }
        else
        {
            if (RootFlows.Any())
            {
                result += $" //\n{indent}.{next}(";
                result += $" //\n{indent}{baseIndent}new FlowBuilder<Flow>(\"{string.Join('-', Subflows.SelectMany(s => s.Flows).Select(r => r.Name))}\")";
            }

            result += $" //\n{indent}{baseIndent}.split(taskExecutor)";
            result += $" //\n{indent}{baseIndent}.add(";
            foreach (var subflow in Subflows)
            {
                result += $" //\n{indent}{baseIndent}{baseIndent}{subflow.ToFlow(indentLevel + 2)}";
                if (Subflows.IndexOf(subflow) != Subflows.Count - 1)
                {
                    result += ",";
                }
            }

            if (RootFlows.Any())
            {
                result += $" //\n{indent}{baseIndent})";
                result += $" //\n{indent}{baseIndent}.end()";
            }

            result += $" //\n{indent})";
        }

        result += $" //\n{indent}.build()";
        return result;
    }

    protected List<List<DataFlow>> Graps(List<DataFlow> flows)
    {
        var graps = new List<List<DataFlow>>();
        while (flows.Any(f => !graps.SelectMany(t => t).Contains(f)))
        {
            var f = flows.Where(f => !graps.SelectMany(t => t).Contains(f)).First();
            var subFlowDataFlows = new List<DataFlow>();
            var stack = new Queue<DataFlow>();
            stack.Enqueue(f);
            var flowsToAdd = new List<DataFlow>();
            while (stack.TryDequeue(out var s))
            {
                subFlowDataFlows.Add(s);
                foreach (var depFlow in flows.Where(f => !graps.SelectMany(t => t).Concat(subFlowDataFlows).Contains(f)).Where(f => s.DependsOn.Contains(f) || f.DependsOn.Contains(s)))
                {
                    stack.Enqueue(depFlow);
                }
            }

            graps.Add(subFlowDataFlows);
        }

        return graps;
    }
}