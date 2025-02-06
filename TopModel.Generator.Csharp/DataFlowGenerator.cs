using System.Data;
using Microsoft.Extensions.Logging;
using TopModel.Core;
using TopModel.Core.FileModel;
using TopModel.Generator.Core;
using TopModel.Utils;

namespace TopModel.Generator.Csharp;

public class DataFlowGenerator(ILogger<DataFlowGenerator> logger, IFileWriterProvider writerProvider)
    : GeneratorBase<CsharpConfig>(logger, writerProvider)
{
    public override IEnumerable<string> GeneratedFiles => Files.Values.SelectMany(f => f.DataFlows)
        .SelectMany(df => Config.Tags.Intersect(df.ModelFile.Tags)
            .SelectMany(tag => new[] { Config.GetDataFlowFilePath(df, tag), Config.GetDataFlowRegistrationFilePath(df, tag) }))
        .Distinct();

    public override string Name => "CSharpDataFlowGen";

    protected virtual void HandleDataFlow(string fileName, DataFlow dataFlow, string tag)
    {
        int GetSourceNumber(DataFlowSource source)
        {
            return dataFlow.Sources.OrderBy(s => s.Source).Where(s => s.Source == source.Source).ToList().IndexOf(source) + 1;
        }

        string GetConnectionName(DataFlowSource source)
        {
            return $"_{source.Source.ToCamelCase()}Connection{GetSourceNumber(source)}";
        }

        using var w = this.OpenCSharpWriter(fileName);

        var usings = new List<string>()
        {
            "Kinetix.Etl",
            "Microsoft.Extensions.Logging",
            Config.GetNamespace(dataFlow.Class, tag)
        };

        foreach (var source in dataFlow.Sources)
        {
            usings.Add(Config.GetNamespace(source.Class, GetBestClassTag(source.Class, tag)));
        }

        w.WriteUsings(usings.ToArray());
        w.WriteLine();
        w.WriteNamespace(Config.GetNamespace(dataFlow, tag));

        var name = $"{dataFlow.Name.ToPascalCase()}Flow";

        w.WriteClassDeclaration(name, $"DataFlow<{dataFlow.Class.NamePascal}>", false);

        foreach (var source in dataFlow.Sources.OrderBy(s => s.Source))
        {
            w.WriteLine(1, $"private IConnection {GetConnectionName(source)};");
        }

        w.WriteLine();

        w.WriteLine(1, $"public {name}(ILogger<{name}> logger, ConnectionPool connectionPool, EtlMonitor monitor)");
        w.WriteLine(2, ": base(logger, connectionPool, monitor)");
        w.WriteLine(1, "{");
        w.WriteLine(1, "}");

        w.WriteLine();
        w.WriteLine(1, $"public override string Name => \"{dataFlow.Name.ToPascalCase()}\";");
        w.WriteLine();
        w.WriteLine(1, $"protected override TargetMode TargetMode => TargetMode.{dataFlow.Type};");

        if (dataFlow.ActiveProperty != null)
        {
            w.WriteLine();
            w.WriteLine(1, $"protected override string ActiveProperty => nameof({dataFlow.Class.NamePascal}.{dataFlow.ActiveProperty.NamePascal});");
        }

        w.WriteLine();
        w.WriteLine(1, $"protected override string TargetName => \"{dataFlow.Target.ToCamelCase()}\";");

        if (dataFlow.DependsOn.Count > 0)
        {
            w.WriteLine();
            w.WriteLine(1, $"public override string[] DependsOn => new[] {{ {string.Join(", ", dataFlow.DependsOn.Select(d => $"\"{d.Name.ToPascalCase()}\""))} }};");
        }

        if (dataFlow.Hooks.Contains(FlowHook.AfterFlow))
        {
            w.WriteLine();
            w.WriteLine(1, $"protected override bool PostFlow => true;");
        }

        if (dataFlow.Hooks.Contains(FlowHook.BeforeFlow))
        {
            w.WriteLine();
            w.WriteLine(1, $"protected override bool PreFlow => true;");
        }

        w.WriteLine();
        w.WriteLine(1, "public override void Dispose()");
        w.WriteLine(1, "{");
        w.WriteLine(2, "base.Dispose();");

        foreach (var source in dataFlow.Sources.OrderBy(s => s.Source))
        {
            w.WriteLine(2, $"{GetConnectionName(source)}?.Dispose();");
        }

        w.WriteLine(1, "}");

        w.WriteLine();
        w.WriteLine(1, $"protected override async Task<IEnumerable<{dataFlow.Class.NamePascal}>> GetData()");
        w.WriteLine(1, "{");

        var firstSource = dataFlow.Sources.FirstOrDefault();
        foreach (var source in dataFlow.Sources.OrderBy(s => s.Source))
        {
            w.WriteLine(2, $"{GetConnectionName(source)} = ConnectionPool.GetConnection(\"{source.Source.ToCamelCase()}\");");
        }

        var hasCreateMapper = firstSource != null && firstSource.Class != dataFlow.Class;

        if (dataFlow.Sources.Count == 1)
        {
            var source = dataFlow.Sources.First();
            w.WriteLine();
            w.Write(2, $"return ");

            if (hasCreateMapper)
            {
                w.Write("(");
            }

            w.Write($"await Get{source.Source.ToPascalCase()}Source{GetSourceNumber(source)}({GetConnectionName(source)})");
            w.WriteLine(hasCreateMapper ? ")" : ";");
        }
        else if (dataFlow.Sources.Count > 1)
        {
            if (dataFlow.Sources.All(source => source.JoinProperties.Count == 0))
            {
                w.WriteLine();
                w.WriteLine(2, "return (await Task.WhenAll(");
                foreach (var source in dataFlow.Sources.OrderBy(s => s.Source))
                {
                    w.Write(3, $"Get{source.Source.ToPascalCase()}Source{GetSourceNumber(source)}({GetConnectionName(source)})");
                    if (dataFlow.Sources.OrderBy(s => s.Source).ToList().IndexOf(source) < dataFlow.Sources.Count - 1)
                    {
                        w.WriteLine(",");
                    }
                    else
                    {
                        w.WriteLine("))");
                    }
                }

                w.WriteLine(2, $".SelectMany(s => s){(hasCreateMapper ? string.Empty : ";")}");
            }
            else if (dataFlow.Sources.All(source => source.JoinProperties.Count > 0))
            {
                string GetVarName(DataFlowSource source)
                {
                    return $"{source.Source.ToCamelCase()}{GetSourceNumber(source)}";
                }

                string GetJoin(DataFlowSource source, DataFlowSource? targetSource = null)
                {
                    var joinProperties = source.JoinProperties.AsEnumerable();

                    if (targetSource != null && joinProperties.Count() > targetSource?.JoinProperties.Count)
                    {
                        foreach (var otherSource in dataFlow.Sources.Skip(1))
                        {
                            if (otherSource == targetSource)
                            {
                                joinProperties = joinProperties.Take(otherSource.JoinProperties.Count);
                                break;
                            }
                            else
                            {
                                joinProperties = joinProperties.Skip(otherSource.JoinProperties.Count);
                            }
                        }
                    }

                    if (joinProperties.Count() == 1)
                    {
                        return $"{GetVarName(source)}.{joinProperties.Single().NamePascal}";
                    }

                    return $"({string.Join(", ", joinProperties.Select(jp => $"{GetVarName(source)}.{jp.NamePascal}"))})";
                }

                foreach (var source in dataFlow.Sources.Skip(1))
                {
                    var varName = GetVarName(source);
                    w.WriteLine();
                    w.WriteLine(2, $"var {source.Source.ToCamelCase()}Source{GetSourceNumber(source)} = (await Get{source.Source.ToPascalCase()}Source{GetSourceNumber(source)}({GetConnectionName(source)}))");
                    w.WriteLine(3, $".ToDictionary({varName} => {GetJoin(source)}, {varName} => {varName});");
                }

                w.WriteLine();

                var mainSource = dataFlow.Sources.First();
                w.WriteLine(2, $"return (await Get{mainSource.Source.ToPascalCase()}Source{GetSourceNumber(mainSource)}({GetConnectionName(mainSource)}))");

                foreach (var source in dataFlow.Sources.Skip(1))
                {
                    var isLast = dataFlow.Sources.Skip(1).ToList().IndexOf(source) == dataFlow.Sources.Count - 2 && !hasCreateMapper;

                    w.WriteLine(3, $".Select({GetVarName(mainSource)} => {GetJoin(mainSource, source)} != default && {source.Source.ToCamelCase()}Source{GetSourceNumber(source)}.TryGetValue({GetJoin(mainSource, source)}, out var {GetVarName(source)})");
                    w.WriteLine(4, $"? {GetVarName(source)}.{source.FirstSourceToMapper?.Name.ToPascalCase() ?? "MissingToMapper"}({GetVarName(mainSource)})");
                    w.WriteLine(4, $": {(source.InnerJoin ? "null" : GetVarName(mainSource))}){(isLast && !source.InnerJoin ? ";" : string.Empty)}");

                    if (source.InnerJoin)
                    {
                        w.WriteLine(3, $".Where({GetVarName(mainSource)} => {GetVarName(mainSource)} != default){(isLast ? ";" : string.Empty)}");
                    }
                }
            }
        }

        if (hasCreateMapper)
        {
            if (firstSource?.TargetFromMapper != null)
            {
                var (ns, modelPath) = Config.GetMapperLocation((dataFlow.Class, firstSource.TargetFromMapper), GetBestClassTag(dataFlow.Class, tag));
                w.WriteLine(3, $".Select({Config.GetMapperName(ns, modelPath)}.Create{dataFlow.Class.NamePascal});");
            }
            else
            {
                w.WriteLine(3, $".Select(MissingCreateMapper.Create{dataFlow.Class.NamePascal});");
            }
        }

        w.WriteLine(1, "}");

        if (dataFlow.Hooks.Contains(FlowHook.AfterFlow))
        {
            w.WriteLine();
            w.WriteLine(1, $"protected override partial Task<int> ExecutePostFlow(IConnection connection);");
        }

        if (dataFlow.Hooks.Contains(FlowHook.BeforeFlow))
        {
            w.WriteLine();
            w.WriteLine(1, $"protected override partial Task<int> ExecutePreFlow(IConnection connection);");
        }

        foreach (var source in dataFlow.Sources.OrderBy(s => s.Source))
        {
            w.WriteLine();
            w.WriteLine(1, $"private static {(source.Mode == DataFlowSourceMode.Partial ? "partial" : "async")} Task<IEnumerable<{source.Class.NamePascal}>> Get{source.Source.ToPascalCase()}Source{GetSourceNumber(source)}(IConnection connection){(source.Mode == DataFlowSourceMode.Partial ? ";" : string.Empty)}");
            if (source.Mode == DataFlowSourceMode.QueryAll)
            {
                w.WriteLine(1, "{");
                w.WriteLine(2, $"return await connection.QueryAllAsync<{source.Class.NamePascal}>();");
                w.WriteLine(1, "}");
            }
        }

        w.WriteLine("}");
    }

    protected virtual void HandleDataFlowPartial(string fileName, DataFlow dataFlow, string tag)
    {
        if (!dataFlow.Sources.Any(s => s.Mode == DataFlowSourceMode.Partial) && !dataFlow.Hooks.Contains(FlowHook.AfterFlow) && !dataFlow.Hooks.Contains(FlowHook.BeforeFlow))
        {
            return;
        }

        if (File.Exists(fileName))
        {
            return;
        }

        using var w = this.OpenCSharpWriter(fileName);
        w.EnableHeader = false;

        w.WriteUsings(new[] { "Kinetix.Etl" }.Concat(dataFlow.Sources.Select(source => Config.GetNamespace(source.Class, GetBestClassTag(source.Class, tag)))).ToArray());
        w.WriteLine();
        w.WriteNamespace(Config.GetNamespace(dataFlow, tag));
        w.WriteClassDeclaration($"{dataFlow.Name.ToPascalCase()}Flow", null, false);

        if (dataFlow.Hooks.Contains(FlowHook.AfterFlow))
        {
            w.WriteLine(1, $"protected override partial async Task<int> ExecutePostFlow(IConnection connection)");
            w.WriteLine(1, "{");
            w.WriteLine(1, "}");
        }

        if (dataFlow.Hooks.Contains(FlowHook.BeforeFlow))
        {
            if (dataFlow.Hooks.Contains(FlowHook.AfterFlow))
            {
                w.WriteLine();
            }

            w.WriteLine(1, $"protected override partial async Task<int> ExecutePreFlow(IConnection connection)");
            w.WriteLine(1, "{");
            w.WriteLine(1, "}");
        }

        var partialSources = dataFlow.Sources.Where(d => d.Mode == DataFlowSourceMode.Partial).OrderBy(s => s.Source);
        foreach (var source in partialSources)
        {
            if (dataFlow.Hooks.Count > 0 || partialSources.ToList().IndexOf(source) > 0)
            {
                w.WriteLine();
            }

            w.WriteLine(1, $"private static partial async Task<IEnumerable<{source.Class.NamePascal}>> Get{source.Source.ToPascalCase()}Source{dataFlow.Sources.OrderBy(s => s.Source).Where(s => s.Source == source.Source).ToList().IndexOf(source) + 1}(IConnection connection)");
            w.WriteLine(1, "{");
            w.WriteLine(1, "}");
        }

        w.WriteLine("}");
    }

    protected override void HandleFiles(IEnumerable<ModelFile> files)
    {
        foreach (var file in files)
        {
            foreach (var dataFlow in file.DataFlows)
            {
                foreach (var (tag, fileName) in Config.Tags.Intersect(file.Tags)
                    .Select(tag => (tag, fileName: Config.GetDataFlowFilePath(dataFlow, tag)))
                    .DistinctBy(t => t.fileName))
                {
                    HandleDataFlow(fileName, dataFlow, tag);
                    HandleDataFlowPartial(fileName.Replace($"{Path.DirectorySeparatorChar}generated", string.Empty).Replace(".cs", ".partial.cs"), dataFlow, tag);
                }
            }
        }

        foreach (var g in Files.Values.SelectMany(f => f.DataFlows)
            .SelectMany(df => Config.Tags.Intersect(df.ModelFile.Tags)
                .Select(tag => (tag, df, fileName: Config.GetDataFlowRegistrationFilePath(df, tag))))
            .GroupBy(g => g.fileName))
        {
            HandleRegistrationFile(g.Key, g.Select(i => i.df), g.First().tag);
        }
    }

    protected virtual void HandleRegistrationFile(string fileName, IEnumerable<DataFlow> flows, string tag)
    {
        var firstFlow = flows.First();
        using var w = this.OpenCSharpWriter(fileName);

        w.WriteUsings("Kinetix.Etl", "Microsoft.Extensions.DependencyInjection");
        w.WriteLine();
        w.WriteNamespace(Config.GetNamespace(firstFlow, tag));
        w.WriteLine("public static class ServiceExtensions");
        w.WriteLine("{");
        w.WriteLine(1, $"public static IServiceCollection Add{firstFlow.ModelFile.Namespace.ModuleFlat}DataFlows(this IServiceCollection services)");
        w.WriteLine(1, "{");
        w.WriteLine(2, "return services");
        foreach (var flow in flows.OrderBy(f => f.Name))
        {
            w.WriteLine(3, $".AddSingleton<IDataFlow, {flow.Name.ToPascalCase()}Flow>(){(flows.OrderBy(f => f.Name).ToList().IndexOf(flow) == flows.Count() - 1 ? ";" : string.Empty)}");
        }

        w.WriteLine(1, "}");
        w.WriteLine("}");
    }
}
