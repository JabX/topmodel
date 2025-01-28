using System.Data;
using Microsoft.Extensions.Logging;
using TopModel.Core;
using TopModel.Core.FileModel;
using TopModel.Generator.Core;
using TopModel.Utils;

namespace TopModel.Generator.Jpa;

public class SpringDataFlowGenerator : GeneratorBase<JpaConfig>
{
    private readonly ILogger<SpringDataFlowGenerator> _logger;

    public SpringDataFlowGenerator(ILogger<SpringDataFlowGenerator> logger)
        : base(logger)
    {
        _logger = logger;
    }

    public override IEnumerable<string> GeneratedFiles =>
    Files.Values.SelectMany(f => f.DataFlows)
        .SelectMany(df => Config.Tags.Intersect(df.ModelFile.Tags)
            .SelectMany(tag => new[] { Config.GetDataFlowFilePath(df, tag) }))
        .Distinct()
        .Concat(Files.Values.Where(f => f.DataFlows.Any()).Select(f => Config.GetDataFlowConfigFilePath(f.Namespace.Module)))
        .Concat(
        Files.Values.SelectMany(f => f.DataFlows)
            .Where(df => df.Hooks.Any() || df.Sources.Any(source => source.Mode == DataFlowSourceMode.Partial))
            .SelectMany(df => Config.Tags.Intersect(df.ModelFile.Tags)
            .SelectMany(tag => new[] { Config.GetDataFlowPartialFilePath(df, tag) })));

    public override string Name => "SpringDataFlowGen";

    protected static void WriteBeanFlow(JavaWriter fw, DataFlow dataFlow)
    {
        fw.AddImports([
            "org.springframework.context.annotation.Bean",
            "org.springframework.batch.core.job.flow.Flow",
            "org.springframework.beans.factory.annotation.Qualifier",
            "org.springframework.batch.core.Step"
        ]);

        fw.WriteLine();
        fw.WriteLine(1, @$"@Bean(""{dataFlow.Name.ToPascalCase()}Flow"")");
        fw.WriteLine(1, @$"public static Flow {dataFlow.Name.ToCamelCase()}Flow(");
        if (dataFlow.Type == DataFlowType.Replace || dataFlow.Type == DataFlowType.HardReplace)
        {
            fw.WriteLine(1, @$"			@Qualifier(""{dataFlow.Name.ToPascalCase()}TruncateStep"") Step {dataFlow.Name.ToCamelCase()}TruncateStep,");
        }

        if (dataFlow.Sources.Any(s => s.Mode == DataFlowSourceMode.Partial))
        {
            fw.WriteLine(1, @$"			{dataFlow.Name.ToPascalCase()}PartialFlow {dataFlow.Name.ToCamelCase()}PartialFlow,");
        }

        fw.WriteLine(1, @$"			@Qualifier(""{dataFlow.Name.ToPascalCase()}Step"") Step {dataFlow.Name.ToCamelCase()}Step) {{");
        fw.AddImport("org.springframework.batch.core.job.builder.FlowBuilder");
        fw.WriteLine(2, @$"return new FlowBuilder<Flow>(""{dataFlow.Name.ToPascalCase()}Flow"") //");
        var isFirst = true;
        if (dataFlow.Hooks.Contains(FlowHook.BeforeFlow))
        {
            fw.WriteLine(3, @$".start({dataFlow.Name.ToCamelCase()}PartialFlow.beforeFlowStep()) //");
            isFirst = false;
        }

        if (dataFlow.Type == DataFlowType.Replace || dataFlow.Type == DataFlowType.HardReplace)
        {
            fw.WriteLine(3, @$".{(isFirst ? "start" : "next")}({dataFlow.Name.ToCamelCase()}TruncateStep) //");
            fw.WriteLine(3, @$".next({dataFlow.Name.ToCamelCase()}Step) //");
        }
        else
        {
            fw.WriteLine(3, @$".{(isFirst ? "start" : "next")}({dataFlow.Name.ToCamelCase()}Step) //");
        }

        if (dataFlow.Hooks.Contains(FlowHook.AfterFlow))
        {
            fw.WriteLine(3, @$".next({dataFlow.Name.ToCamelCase()}PartialFlow.afterFlowStep()) //");
        }

        fw.WriteLine(3, ".build();");
        fw.WriteLine(1, "}");
    }

    protected static void WriteBeanTruncateStep(JavaWriter fw, DataFlow dataFlow)
    {
        fw.WriteLine();
        fw.AddImport("io.github.kleecontrib.spring.batch.tasklet.QueryTasklet");
        fw.WriteLine(1, @$"@Bean(""{dataFlow.Name.ToPascalCase()}TruncateStep"")");
        fw.WriteLine(1, @$"public static Step {dataFlow.Name.ToCamelCase()}TruncateStep( //");
        fw.WriteLine(1, @$"		JobRepository jobRepository, //");
        fw.WriteLine(1, @$"		PlatformTransactionManager transactionManager, //");
        fw.AddImport("javax.sql.DataSource");
        fw.WriteLine(1, @$"		@Qualifier(""{dataFlow.Target}"") DataSource dataSource) {{");
        fw.WriteLine(1, @$"return new StepBuilder(""{dataFlow.Name.ToPascalCase()}TruncateStep"", jobRepository) //");
        fw.WriteLine(2, @$"		.tasklet(new QueryTasklet(dataSource, ""truncate table {dataFlow.Class.SqlName}{(dataFlow.Type == DataFlowType.HardReplace ? " cascade" : string.Empty)}""), transactionManager) //");

        fw.WriteLine(3, ".build();");
        fw.WriteLine(1, "}");
    }

    protected string GetProcessorName(DataFlow dataFlow, FlowHook flowHook, int index)
    {
        var suffix = string.Empty;
        if (dataFlow.Hooks.Where(h => h == flowHook).Count() > 1)
        {
            suffix = "_" + dataFlow.Hooks.Take(index + 1).Where(h => h == flowHook).Count();
        }

        return $@"{flowHook}{suffix}";
    }

    protected Class? GetProcessorSourceClass(FlowHook flowHook, DataFlow flow)
    {
        return flowHook switch
        {
            FlowHook.AfterSource or FlowHook.Map => flow.Sources[0].Class,
            FlowHook.BeforeTarget => flow.Class,
            _ => null,
        };
    }

    protected Class? GetProcessorTargetClass(FlowHook flowHook, DataFlow flow)
    {
        return flowHook switch
        {
            FlowHook.AfterSource => flow.Sources[0].Class,
            FlowHook.BeforeTarget or FlowHook.Map => flow.Class,
            _ => null,
        };
    }

    protected void HandleDataFlow(string fileName, DataFlow dataFlow, string tag)
    {
        WriteClassFlow(fileName, dataFlow, tag);
        WritePartialInterface(dataFlow, tag);
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
                }
            }
        }

        foreach (var module in files.Where(f => f.DataFlows.Any()).Select(f => f.Namespace.Module))
        {
            WriteModuleConfig(module, files.Where(f => f.Namespace.Module == module).SelectMany(f => f.DataFlows));
        }
    }

    protected void WriteBeanReader(JavaWriter fw, DataFlow dataFlow, string tag)
    {
        fw.WriteLine();
        var tagToUse = tag;
        if (!dataFlow.Class.ModelFile.Tags.Contains(tag))
        {
            tagToUse = Config.Tags.Intersect(dataFlow.Sources.First().Class.ModelFile.Tags).First();
        }

        var query = $"select * from {(Config.ResolveVariables(Config.DbSchema!, tag: tagToUse) == null ? string.Empty : $"{Config.ResolveVariables(Config.DbSchema!, tag: tagToUse)}.")}{dataFlow.Sources.First().Class.SqlName}";
        fw.AddImport("org.springframework.batch.item.database.builder.JdbcCursorItemReaderBuilder");
        fw.AddImport("io.github.kleecontrib.spring.batch.bulk.mapping.JdbcEntityRowMapper");
        fw.AddImport("org.springframework.batch.item.ItemReader");
        fw.WriteLine(1, @$"@Bean(""{dataFlow.Name.ToPascalCase()}Reader"")");
        fw.WriteLine(1, @$"public static ItemReader<{dataFlow.Sources.First().Class.NamePascal}> {dataFlow.Name.ToCamelCase()}Reader( //");
        fw.AddImport("javax.sql.DataSource");
        fw.WriteLine(1, @$"		@Qualifier(""{dataFlow.Sources.First().Source}"") DataSource datasource) {{");
        fw.WriteLine(2, $"return new JdbcCursorItemReaderBuilder<{dataFlow.Sources.First().Class.NamePascal}>() //");
        fw.WriteLine(2, @$"		.name(""{dataFlow.Name.ToPascalCase()}Reader"") //");
        fw.WriteLine(2, @$"		.rowMapper(new JdbcEntityRowMapper<>({dataFlow.Sources.First().Class.NamePascal}.class)) //");
        fw.WriteLine(2, @$"		.sql(""{query}"") //");
        fw.WriteLine(2, @$"		.fetchSize({Config.DataFlowsBulkSize}) //");
        fw.WriteLine(2, @$"		.dataSource(datasource) //");
        fw.WriteLine(2, @$"		.build();");
        fw.WriteLine(1, "}");
    }

    protected void WriteBeanStep(JavaWriter fw, DataFlow dataFlow, string tag)
    {
        fw.AddImport("org.springframework.batch.core.step.builder.StepBuilder");
        fw.AddImport("org.springframework.batch.core.repository.JobRepository");
        fw.AddImport("org.springframework.transaction.PlatformTransactionManager");
        fw.AddImport("org.springframework.batch.item.ItemWriter");
        fw.AddImport(dataFlow.Sources.First().Class.GetImport(Config, tag));
        fw.AddImport(dataFlow.Class.GetImport(Config, tag));

        fw.WriteLine();
        fw.WriteLine(1, @$"@Bean(""{dataFlow.Name.ToPascalCase()}Step"")");
        fw.WriteLine(1, @$"public static Step {dataFlow.Name.ToCamelCase()}Step(");
        fw.WriteLine(1, @$"		JobRepository jobRepository, //");
        fw.WriteLine(1, @$"		PlatformTransactionManager transactionManager, //");
        foreach (var listener in Config.DataFlowsListeners)
        {
            fw.AddImport("org.springframework.batch.core.StepListener");
            fw.WriteLine(1, @$"		@Qualifier(""{listener}"") StepListener {listener.ToCamelCase()}, //");
        }

        foreach (var source in dataFlow.Sources.Where(s => s.Mode == DataFlowSourceMode.QueryAll))
        {
            fw.AddImport("org.springframework.batch.item.ItemReader");
            fw.WriteLine(1, @$"		@Qualifier(""{dataFlow.Name.ToPascalCase()}Reader"") ItemReader<{dataFlow.Sources.First().Class.NamePascal}> reader, //");
        }

        var hookStep = dataFlow.Hooks.Where(h => h != FlowHook.BeforeFlow && h != FlowHook.AfterFlow);
        if (hookStep.Any() || dataFlow.Sources.Any(s => s.Mode == DataFlowSourceMode.Partial))
        {
            fw.WriteLine(1, @$"		{dataFlow.Name.ToPascalCase()}PartialFlow {dataFlow.Name.ToCamelCase()}PartialFlow,");
        }

        fw.WriteLine(1, @$"		@Qualifier(""{dataFlow.Name.ToPascalCase()}Writer"") ItemWriter<{dataFlow.Class.NamePascal}> writer //");

        var processors = new List<string>();
        fw.WriteLine(1, ") {");
        fw.WriteLine(2, @$"return new StepBuilder(""{dataFlow.Name.ToPascalCase()}Step"", jobRepository) //");
        fw.WriteLine(3, @$".<{dataFlow.Sources.First().Class.NamePascal}, {dataFlow.Class.NamePascal}>chunk({Config.DataFlowsBulkSize}, transactionManager) //");
        foreach (var source in dataFlow.Sources)
        {
            if (source.Mode == DataFlowSourceMode.QueryAll)
            {
                fw.WriteLine(3, ".reader(reader) //");
            }
            else
            {
                fw.WriteLine(3, $".reader({dataFlow.Name.ToCamelCase()}PartialFlow.{source.Class.NameCamel}Reader())");
            }
        }

        var i = 0;
        foreach (var t in dataFlow.Hooks.Where(h => h == FlowHook.AfterSource))
        {
            processors.Add($"{dataFlow.Name.ToCamelCase()}PartialFlow.{GetProcessorName(dataFlow, FlowHook.AfterSource, i++).ToCamelCase()}()");
        }

        if (dataFlow.Hooks.Contains(FlowHook.Map))
        {
            foreach (var t in dataFlow.Hooks.Where(h => h == FlowHook.Map))
            {
                processors.Add($"{dataFlow.Name.ToCamelCase()}PartialFlow.{GetProcessorName(dataFlow, FlowHook.Map, 0).ToCamelCase()}()");
            }
        }
        else if (dataFlow.Sources.First().Class != dataFlow.Class)
        {
            processors.Add($"({dataFlow.Sources[0].Class.NamePascal} item) -> new {dataFlow.Class.NamePascal}(item)");
        }

        i = 0;
        foreach (var t in dataFlow.Hooks.Where(h => h == FlowHook.BeforeTarget))
        {
            processors.Add($"{dataFlow.Name.ToCamelCase()}PartialFlow.{GetProcessorName(dataFlow, FlowHook.BeforeTarget, i++).ToCamelCase()}()");
        }

        if (processors.Count() == 1)
        {
            fw.WriteLine(3, $".processor({processors[0]}) //");
        }
        else if (processors.Count() > 1)
        {
            fw.AddImport("org.springframework.batch.item.support.CompositeItemProcessor");
            fw.WriteLine(3, $".processor(new CompositeItemProcessor<>({string.Join(", ", processors)})) //");
        }

        foreach (var listener in Config.DataFlowsListeners)
        {
            fw.WriteLine(3, $".listener({listener.ToCamelCase()}) //");
        }

        fw.WriteLine(3, ".faultTolerant() //");
        fw.AddImport("org.springframework.batch.core.step.skip.AlwaysSkipItemSkipPolicy");
        fw.WriteLine(3, ".skipPolicy(new AlwaysSkipItemSkipPolicy()) //");
        fw.WriteLine(3, ".writer(writer) //");
        fw.WriteLine(3, ".build();");
        fw.WriteLine(1, "}");
    }

    protected void WriteBeanWriter(JavaWriter fw, DataFlow dataFlow, string tag)
    {
        fw.AddImport("io.github.kleecontrib.spring.batch.bulk.upsert.BulkItemWriter");

        fw.WriteLine();
        fw.WriteLine(1, @$"@Bean(""{dataFlow.Name.ToPascalCase()}Writer"")");
        fw.AddImport("com.zaxxer.hikari.HikariDataSource");
        fw.WriteLine(1, @$"public static ItemWriter<{dataFlow.Class.NamePascal}> {dataFlow.Name.ToCamelCase()}Writer(@Qualifier(""{dataFlow.Target}"") HikariDataSource targetDataSource) {{");
        fw.WriteLine(2, @$"return new BulkItemWriter<>(targetDataSource, new {dataFlow.Class.NamePascal}Mapping(targetDataSource.getSchema()));");
        fw.WriteLine(1, "}");
        WriteWriterMapper(fw, dataFlow, tag);
    }

    protected void WriteClassFlow(string fileName, DataFlow dataFlow, string tag)
    {
        var packageName = Config.ResolveVariables(
            Config.DataFlowsPath!,
            tag,
            module: dataFlow.ModelFile.Namespace.Module).ToPackageName();

        using var fw = new JavaWriter(fileName, _logger, packageName);
        fw.AddImport("org.springframework.context.annotation.Configuration");
        fw.WriteLine();
        fw.WriteLine("@Configuration");
        var javaxOrJakarta = Config.PersistenceMode.ToString().ToLower();
        if (Config.GeneratedHint)
        {
            fw.WriteLine(0, Config.GeneratedAnnotation);
        }

        fw.WriteClassDeclaration($"{dataFlow.Name}Flow", null);
        fw.WriteLine();
        fw.WriteLine(1, $@"protected {dataFlow.Name}Flow() {{");
        fw.WriteLine(2, "// protected constructor to hide implicite public one");
        fw.WriteLine(1, "}");

        WriteBeanFlow(fw, dataFlow);
        WriteBeanStep(fw, dataFlow, tag);

        if (dataFlow.Type == DataFlowType.Replace || dataFlow.Type == DataFlowType.HardReplace)
        {
            WriteBeanTruncateStep(fw, dataFlow);
        }

        if (dataFlow.Sources[0].Mode == DataFlowSourceMode.QueryAll)
        {
            WriteBeanReader(fw, dataFlow, tag);
        }

        WriteBeanWriter(fw, dataFlow, tag);
        fw.WriteLine("}");
    }

    protected void WriteModuleConfig(string module, IEnumerable<DataFlow> flows)
    {
        var configFilePath = Config.GetDataFlowConfigFilePath(module);
        var packageName = Config.ResolveVariables(
            Config.DataFlowsPath!,
            module: module).ToPackageName();
        using var fw = new JavaWriter(configFilePath, _logger, packageName);
        fw.AddImports([
            "org.springframework.context.annotation.Configuration",
            "org.springframework.context.annotation.Bean",
            "org.springframework.batch.core.Job",
            "org.springframework.batch.core.repository.JobRepository",
            "org.springframework.beans.factory.annotation.Qualifier",
            "org.springframework.batch.core.job.flow.Flow",
            "org.springframework.batch.core.job.builder.JobBuilder",
            "org.springframework.batch.core.launch.support.RunIdIncrementer",
            "org.springframework.core.task.TaskExecutor",
            "org.springframework.context.annotation.Import"
        ]);
        fw.WriteLine();
        fw.WriteLine("@Configuration");
        var javaxOrJakarta = Config.PersistenceMode.ToString().ToLower();
        if (Config.GeneratedHint)
        {
            fw.WriteLine(0, Config.GeneratedAnnotation);
        }

        fw.WriteLine(@$"@Import({{{string.Join(", ", flows.Select(f => $@"{f.Name.ToPascalCase()}Flow.class"))}}})");

        var className = configFilePath.Split("\\").Last().Split('.').First();
        fw.WriteClassDeclaration($"{className}", null);
        fw.WriteLine(1, @$"@Bean(""{module.ToPascalCase()}Job"")");
        fw.WriteLine(1, @$"public Job {module.ToCamelCase()}Job( //");
        fw.WriteLine(1, @$"			JobRepository jobRepository, //");
        fw.WriteLine(1, @$"			TaskExecutor taskExecutor, //");
        fw.WriteLine("			" + string.Join(", //\n			", flows.Select(f => $@"@Qualifier(""{f.Name}Flow"") Flow {f.Name.ToCamelCase()}Flow")));
        fw.WriteLine(1, ") {");
        fw.WriteLine(2, $@"return new JobBuilder(""{module.ToPascalCase()}Job"", jobRepository) //");
        fw.WriteLine(2, $@"		.incrementer(new RunIdIncrementer()) //");

        var flowTree = new FlowTree(flows.ToList());
        var toFlow = flowTree.ToFlow(4);
        if (toFlow.Contains("FlowBuilder"))
        {
            fw.AddImport("org.springframework.batch.core.job.builder.FlowBuilder");
        }

        fw.WriteLine(2, $"		.start({toFlow})");
        fw.WriteLine(2, "		.end() //");
        fw.WriteLine(2, "		.build();");
        fw.WriteLine(1, "}");
        fw.WriteLine("}");
    }

    protected void WritePartialInterface(DataFlow dataFlow, string tag)
    {
        if (dataFlow.Hooks.Any() || dataFlow.Sources.Any(source => source.Mode == DataFlowSourceMode.Partial))
        {
            var packageName = Config.ResolveVariables(
                Config.DataFlowsPath!,
                tag,
                module: dataFlow.ModelFile.Namespace.Module).ToPackageName();
            var fileName = Config.GetDataFlowPartialFilePath(dataFlow, tag);
            using var fw = new JavaWriter($"{fileName}", _logger, packageName, null);
            fw.WriteLine();
            fw.WriteLine(@$"public interface {dataFlow.Name.ToPascalCase()}PartialFlow {{");
            foreach (var source in dataFlow.Sources.Where(s => s.Mode == DataFlowSourceMode.Partial))
            {
                fw.AddImport("org.springframework.batch.item.ItemReader");
                fw.WriteDocStart(1, "Remplacer l'implémentation du reader par défaut");
                fw.WriteReturns(1, "ItemReader reader au sens de spring-batch");
                fw.WriteDocEnd(1);
                fw.AddImport(source.Class.GetImport(Config, tag));
                fw.WriteLine(1, @$"ItemReader<{source.Class.NamePascal}> {source.Class.NameCamel}Reader();");
            }

            if (dataFlow.Hooks.Contains(FlowHook.BeforeFlow))
            {
                fw.AddImport("org.springframework.batch.core.Step");
                fw.WriteLine();
                fw.WriteDocStart(1, "Etape positionnée avant toute autre opération dans ce flux (avant la première lecture ou le truncate)");
                fw.WriteReturns(1, "Step étape au sens de spring-batch");
                fw.WriteDocEnd(1);
                fw.WriteLine(1, @$" Step beforeFlowStep();");
            }

            if (dataFlow.Hooks.Contains(FlowHook.AfterFlow))
            {
                fw.AddImport("org.springframework.batch.core.Step");
                fw.WriteLine();
                fw.WriteDocStart(1, "Etape après toutes les opérations de ce flux (après la dernière écriture)");
                fw.WriteReturns(1, "Step étape au sens de spring-batch");
                fw.WriteDocEnd(1);
                fw.WriteLine(1, @$"Step afterFlowStep();");
            }

            var hookStep = dataFlow.Hooks.Where(h => h != FlowHook.BeforeFlow && h != FlowHook.AfterFlow);
            var index = 0;
            foreach (var hook in hookStep)
            {
                var isLast = index == hookStep.Count() - 1;
                var processorName = GetProcessorName(dataFlow, hook, index).ToCamelCase();
                var processorSourceClass = GetProcessorSourceClass(hook, dataFlow)!;
                var processorTargetClass = GetProcessorTargetClass(hook, dataFlow)!;
                fw.AddImport("org.springframework.batch.item.ItemProcessor");
                fw.AddImport(processorSourceClass.GetImport(Config, tag));
                fw.AddImport(processorTargetClass.GetImport(Config, tag));
                fw.WriteLine();
                fw.WriteDocStart(1, "Processus de transformation de la donnée dans le flow. Les étapes sont ordonnées comme suit :");
                fw.WriteLine(1, " * Read - AfterSource - Map - BeforeWrite - Write");
                fw.WriteLine(1, " * Le map remplace le mapping par défaut de TopModel");
                fw.WriteReturns(1, "ItemProcessor étape au sens de spring-batch");
                fw.WriteDocEnd(1);
                fw.WriteLine(1, @$"ItemProcessor<{processorSourceClass.NamePascal}, {processorTargetClass.NamePascal}> {processorName}();");
                index++;
            }

            fw.WriteLine("}");
        }
    }

    protected void WriteWriterMapper(JavaWriter fw, DataFlow dataFlow, string tag)
    {
        fw.WriteLine();
        if (dataFlow.Type != DataFlowType.Merge)
        {
            fw.AddImport("de.bytefish.pgbulkinsert.mapping.AbstractMapping");
            fw.WriteLine(1, @$"private static class {dataFlow.Class.NamePascal}Mapping extends AbstractMapping<{dataFlow.Class.NamePascal}> {{");
        }
        else
        {
            fw.AddImport("io.github.kleecontrib.spring.batch.bulk.mapping.AbstractUpsertMapping");
            fw.WriteLine(1, @$"private static class {dataFlow.Class.NamePascal}Mapping extends AbstractUpsertMapping<{dataFlow.Class.NamePascal}> {{");
        }

        fw.WriteLine(2, @$"public {dataFlow.Class.NamePascal}Mapping(String schema) {{");

        var tagToUse = tag;

        if (!dataFlow.Class.ModelFile.Tags.Contains(tag))
        {
            tagToUse = Config.Tags.Intersect(dataFlow.Class.ModelFile.Tags).First();
        }

        if (dataFlow.Type != DataFlowType.Merge)
        {
            fw.WriteLine(3, @$"super(schema, ""{dataFlow.Class.SqlName}"");");
        }
        else
        {
            var primaryKey = dataFlow.Class.Extends != null ? dataFlow.Class.Extends.PrimaryKey : dataFlow.Class.PrimaryKey;
            fw.WriteLine(3, @$"super(schema, ""{dataFlow.Class.SqlName}"", ""{string.Join(',', primaryKey.Select(pk => pk.SqlName))}"");");
        }

        fw.AddImport("de.bytefish.pgbulkinsert.pgsql.constants.DataType");
        var mapper = dataFlow.Class.FromMappers.Where(m =>
            {
                var result = true;
                for (int i = 0; i < m.ClassParams.Count(); i++)
                {
                    result = result && m.ClassParams.ElementAt(i).Class == dataFlow.Sources[i].Class;
                }

                return result;
            }).FirstOrDefault();

        if (dataFlow.Hooks.Contains(FlowHook.Map))
        {
            mapper = null;
        }

        foreach (var property in dataFlow.Class.ExtendedProperties
            .Where(p => !(p is AssociationProperty ap && (ap.Type == AssociationType.OneToMany || ap.Type == AssociationType.OneToMany)))
            .Where(p => mapper == null || mapper.ClassParams.SelectMany(pa => pa.Mappings).Select(mapping => mapping.Key).Contains(p)))
        {
            var sqlType = property.Domain.Implementations["sql"].Type ?? string.Empty;
            string dataType = sqlType.ToUpper() switch
            {
                "VARCHAR" => "VarChar",
                _ => sqlType.ToPascalCase()
            };
            fw.WriteLine(3, $@"map(""{property.SqlName}"", DataType.{dataType}, {dataFlow.Class.NamePascal}::{(Config.GetType(property) == "boolean" ? "is" : "get")}{property.NamePascal.ToFirstUpper()});");
        }

        fw.WriteLine(2, "}");
        fw.WriteLine(1, "}");
    }
}
