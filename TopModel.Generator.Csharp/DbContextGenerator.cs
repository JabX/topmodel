using Microsoft.Extensions.Logging;
using TopModel.Core;
using TopModel.Generator.Core;
using TopModel.Utils;

namespace TopModel.Generator.Csharp;

public class DbContextGenerator(ILogger<DbContextGenerator> logger, IFileWriterProvider writerProvider)
    : ClassGroupGeneratorBase<CsharpConfig>(logger, writerProvider)
{
    public override string Name => "CSharpDbContextGen";

    protected virtual IEnumerable<(IProperty Property, AssociationProperty AssociationProperty)> GetAssociationProperties(IEnumerable<Class> classes, string tag)
    {
        return classes
            .Distinct()
            .OrderBy(c => c.NamePascal)
            .SelectMany(c => c.Properties)
            .Where(p => p is AssociationProperty { Association.IsPersistent: true } || p is AliasProperty { Property: AssociationProperty { Association.IsPersistent: true } })
            .Select(p => p switch
            {
                AssociationProperty ap => (p, ap),
                AliasProperty { Property: AssociationProperty ap } => (p, ap),
                _ => (null!, null!)
            })
            .Where(p => p.ap.Type == AssociationType.ManyToOne || p.ap.Type == AssociationType.OneToOne)
            .Where(p => Classes.Contains(p.ap.Association) && Config.IsPersistent(p.ap.Association, GetBestClassTag(p.ap.Association, tag)));
    }

    protected override IEnumerable<(string FileType, string FileName)> GetFileNames(Class classe, string tag)
    {
        if (classe.IsPersistent && !classe.Abstract && !Config.NoPersistence(tag))
        {
            yield return ("main", Config.GetDbContextFilePath(tag));

            if (Config.UseEFComments)
            {
                yield return ("comments", Config.GetDbContextFilePath(tag).Replace(".cs", ".comments.cs"));
            }
        }
    }

    protected virtual void HandleCommentsFile(string fileName, string tag, string dbContextName, string contextNs, IList<string> usings, IList<Class> classes)
    {
        using var cw = this.OpenCSharpWriter(fileName);

        cw.WriteUsings([.. usings]);

        cw.WriteLine();
        cw.WriteNamespace(contextNs);

        cw.WriteSummary("Partial pour ajouter les commentaires EF.");
        cw.WriteLine($"public partial class {dbContextName} : DbContext");
        cw.WriteLine("{");
        cw.WriteLine(1, "partial void AddComments(ModelBuilder modelBuilder)");
        cw.WriteLine(1, "{");

        foreach (var classe in classes)
        {
            cw.WriteLine(2, $"var {classe.NameCamel} = modelBuilder.Entity<{classe.NamePascal}>();");
            cw.WriteLine(2, $"{classe.NameCamel}.ToTable(t => t.HasComment(\"{classe.Comment.Replace("\"", "\\\"")}\"));");

            foreach (var property in classe.Properties)
            {
                cw.WriteLine(2, $"{classe.NameCamel}.Property(p => p.{property.NamePascal}).HasComment(\"{property.Comment.Replace("\"", "\\\"")}\");");
            }

            if (classes.IndexOf(classe) < classes.Count - 1)
            {
                cw.WriteLine();
            }
        }

        cw.WriteLine(1, "}");
        cw.WriteLine("}");
    }

    protected override void HandleFile(string fileType, string fileName, string tag, IEnumerable<Class> classes)
    {
        var dbContextName = Config.GetDbContextName(tag);
        var usings = new List<string> { "Microsoft.EntityFrameworkCore" };
        var contextNs = Config.GetDbContextNamespace(tag);

        foreach (var ns in classes
            .Concat(GetAssociationProperties(classes, tag).Select(ap => ap.AssociationProperty.Association))
            .Select(c => Config.GetNamespace(c, GetBestClassTag(c, tag)))
            .Distinct())
        {
            usings.Add(ns);
        }

        var classList = classes.OrderBy(c => c.NamePascal).ToList();

        if (fileType == "main")
        {
            HandleMainFile(fileName, tag, dbContextName, contextNs, usings, classList);
        }
        else
        {
            HandleCommentsFile(fileName, tag, dbContextName, contextNs, usings, classList);
        }
    }

    protected virtual void HandleMainFile(string fileName, string tag, string dbContextName, string contextNs, List<string> usings, List<Class> classes)
    {
        using var w = this.OpenCSharpWriter(fileName);

        foreach (var value in classes.SelectMany(c => c.Values.SelectMany(v => v.Value)))
        {
            usings.AddRange(Config.GetValueImports(value.Key, value.Value));
        }

        w.WriteUsings(usings.Distinct().ToArray());
        w.WriteLine();
        w.WriteNamespace(contextNs);

        w.WriteSummary("DbContext généré pour Entity Framework Core.");
        w.WriteLine($"public partial class {dbContextName} : DbContext");
        w.WriteLine("{");

        w.WriteSummary(1, "Constructeur par défaut.");
        w.WriteParam("options", "Options du DbContext.");
        w.WriteLine(1, $"public {dbContextName}(DbContextOptions<{dbContextName}> options)");
        w.WriteLine(2, ": base(options)");
        w.WriteLine(1, "{");
        w.WriteLine(1, "}");

        foreach (var classe in classes)
        {
            w.WriteLine();
            w.WriteSummary(1, "Accès à l'entité " + classe.NamePascal);
            w.WriteLine(1, "public DbSet<" + classe.NamePascal + "> " + classe.PluralNamePascal + " { get; set; }");
        }

        w.WriteLine();
        w.WriteSummary(1, "Personalisation du modèle.");
        w.WriteParam("modelBuilder", "L'objet de construction du modèle.");
        w.WriteLine(1, "protected override void OnModelCreating(ModelBuilder modelBuilder)");
        w.WriteLine(1, "{");

        var hasPropConfig = false;
        foreach (var fp in classes.Distinct().OrderBy(c => c.NamePascal).SelectMany(c => c.Properties))
        {
            var prop = fp is AliasProperty alp ? alp.Property : fp;
            var ap = prop as AssociationProperty;

            var classe = ap != null ? ap.Association : prop.Class;
            var targetProp = ap != null ? ap.Property : prop;

            if (Config.CanClassUseEnums(classe, Classes, targetProp))
            {
                hasPropConfig = true;
                w.WriteLine(2, $"modelBuilder.Entity<{fp.Class}>().Property(p => p.{fp.NamePascal}).HasConversion<{Config.GetImplementation(fp.Domain)?.Type ?? string.Empty}>(){(fp.Domain?.Length != null ? $".HasMaxLength({fp.Domain.Length})" : string.Empty)};");
            }

            if (fp.Domain?.Length != null && fp.Domain?.Scale != null)
            {
                hasPropConfig = true;
                w.WriteLine(2, $"modelBuilder.Entity<{fp.Class}>().Property(x => x.{fp.NamePascal}).HasPrecision({fp.Domain.Length}, {fp.Domain.Scale});");
            }
        }

        if (hasPropConfig)
        {
            w.WriteLine();
        }

        var hasPk = false;
        foreach (var classe in classes.Distinct().OrderBy(c => c.NamePascal).Where(c => c.PrimaryKey.Count() > 1))
        {
            hasPk = true;
            w.WriteLine(2, $"modelBuilder.Entity<{classe}>().HasKey(p => new {{ {string.Join(", ", classe.PrimaryKey.Select(pk => $"p.{pk.NamePascal}"))} }});");
        }

        if (hasPk)
        {
            w.WriteLine();
        }

        var hasJson = false;
        foreach (var cp in classes.Distinct().SelectMany(c => c.Properties.Where(p => p is CompositionProperty)))
        {
            hasJson = true;
            var sqlName = Config.UseLowerCaseSqlNames ? cp.SqlName.ToLower() : cp.SqlName;
            w.WriteLine(2, $@"modelBuilder.Entity<{cp.Class}>().Owns{(cp.Domain == null ? "One" : "Many")}(p => p.{cp.NamePascal}, p => p.ToJson(""{sqlName}""));");
        }

        if (hasJson)
        {
            w.WriteLine();
        }

        if (Config.UseEFMigrations)
        {
            var hasFk = false;
            foreach (var (prop, ap) in GetAssociationProperties(classes, tag))
            {
                hasFk = true;
                w.WriteLine(2, $"modelBuilder.Entity<{prop.Class}>().HasOne<{ap.Association}>().With{(ap.Type == AssociationType.ManyToOne ? "Many" : "One")}().HasForeignKey{(ap.Type == AssociationType.ManyToOne ? string.Empty : $"<{prop.Class}>")}(p => p.{prop.NamePascal}).OnDelete(DeleteBehavior.Restrict);");
            }

            if (hasFk)
            {
                w.WriteLine();
            }

            var hasUk = false;
            foreach (var uk in classes.Distinct().OrderBy(c => c.NamePascal).SelectMany(c => c.UniqueKeys))
            {
                hasUk = true;
                var expr = uk.Count == 1 ? $"p.{uk.Single().NamePascal}" : $"new {{ {string.Join(", ", uk.Select(p => $"p.{p.NamePascal}"))} }}";
                w.WriteLine(2, $"modelBuilder.Entity<{uk.First().Class}>().HasIndex(p => {expr}).IsUnique();");
            }

            if (hasUk)
            {
                w.WriteLine();
            }

            var hasData = false;
            foreach (var classe in classes.Distinct().Where(c => c.Values.Count > 0).OrderBy(c => c.NamePascal))
            {
                hasData = true;
                w.WriteLine(2, $"modelBuilder.Entity<{classe.NamePascal}>().HasData(");
                foreach (var refValue in classe.Values)
                {
                    w.Write($"            new {classe.NamePascal} {{");

                    foreach (var refProp in refValue.Value.ToList())
                    {
                        var prop = refProp.Key is AliasProperty alp ? alp.Property : refProp.Key;
                        var ap = prop as AssociationProperty;

                        var targetClass = ap != null ? ap.Association : prop.Class;
                        var targetProp = ap != null ? ap.Property : prop;

                        var value = Config.GetValue(refProp.Key, Classes, refProp.Value);
                        if (value.StartsWith(targetClass.PluralNamePascal))
                        {
                            var targetNs = Config.GetNamespace(targetClass, tag);
                            var contextNsSplit = contextNs.Split('.');
                            var targetNsSplit = targetNs.Split('.');
                            targetNs = string.Join(".", targetNsSplit.SkipWhile((spl, i) => spl == contextNsSplit.ElementAtOrDefault(i)));
                            value = $"{targetNs}.{value}";
                        }

                        w.Write($" {refProp.Key.NamePascal} = {value}");
                        if (refValue.Value.ToList().IndexOf(refProp) < refValue.Value.Count - 1)
                        {
                            w.Write(",");
                        }
                    }

                    w.Write(" }");
                    if (classe.Values.IndexOf(refValue) < classe.Values.Count - 1)
                    {
                        w.Write(",\r\n");
                    }
                }

                w.Write(");\r\n");
            }

            if (hasData)
            {
                w.WriteLine();
            }

            if (Config.UseEFComments)
            {
                w.WriteLine(2, "AddComments(modelBuilder);");
            }
        }

        w.WriteLine(2, "OnModelCreatingPartial(modelBuilder);");
        w.WriteLine(1, "}");

        if (Config.UseEFMigrations && Config.UseEFComments)
        {
            w.WriteLine();
            w.WriteLine(1, "partial void AddComments(ModelBuilder modelBuilder);");
        }

        w.WriteLine();
        w.WriteLine(1, "partial void OnModelCreatingPartial(ModelBuilder modelBuilder);");

        w.WriteLine("}");
    }
}