using Microsoft.Extensions.Logging;
using TopModel.Core;
using TopModel.Core.Model.Implementation;
using TopModel.Generator.Core;
using TopModel.Utils;

namespace TopModel.Generator.Jpa.ClassGeneration;

/// <summary>
/// Générateur de fichiers de modèles JPA.
/// </summary>
public class JpaEntityGenerator(ILogger<JpaEntityGenerator> logger, IFileWriterProvider writerProvider)
    : JavaClassGeneratorBase(logger, writerProvider)
{
    public override string Name => "JpaEntityGen";

    protected override bool FilterClass(Class classe)
    {
        return !classe.Abstract && classe.IsPersistent && !Config.CanClassUseEnums(classe, Classes);
    }

    protected override string GetFileName(Class classe, string tag)
    {
        return Path.Combine(
            Config.OutputDirectory,
            Config.ResolveVariables(Config.EntitiesPath, tag, module: classe.Namespace.Module).ToFilePath(),
            $"{classe.NamePascal}.java");
    }

    protected virtual string GetterToCompareCompositePkPk(IProperty pk)
    {
        if (pk is AssociationProperty ap)
        {
            return $".{JpaModelPropertyGenerator.GetGetterName(ap.Property)}()";
        }
        else if (pk is AliasProperty al && al.Property is AssociationProperty asp)
        {
            return $".get{JpaModelPropertyGenerator.GetGetterName(asp.Property)}()";
        }

        return string.Empty;
    }

    protected override void HandleClass(string fileName, Class classe, string tag)
    {
        var packageName = Config.GetPackageName(classe, tag);
        using var fw = this.OpenJavaWriter(fileName, packageName, null);

        fw.WriteLine();

        WriteAnnotations(fw, classe, tag);

        var extends = Config.GetClassExtends(classe);
        if (classe.Extends is not null)
        {
            fw.AddImport($"{Config.GetPackageName(classe.Extends, tag)}.{classe.Extends.NamePascal}");
        }

        var implements = Config.GetClassImplements(classe).ToList();

        fw.WriteClassDeclaration(classe.NamePascal, null, extends, implements);

        JpaModelPropertyGenerator.WriteProperties(fw, classe, tag);
        WriteCompositePrimaryKeyClass(fw, classe, tag);

        WriteConstructors(classe, tag, fw);

        WriteGetters(fw, classe, tag);
        WriteSetters(fw, classe, tag);
        WriteAdders(fw, classe, tag);
        WriteRemovers(fw, classe, tag);

        if (Config.MappersInClass)
        {
            WriteToMappers(fw, classe, tag);
        }

        if ((Config.FieldsEnum & Target.Persisted) > 0)
        {
            WriteFieldsEnum(fw, classe, tag);
        }

        fw.WriteLine("}");
    }

    protected virtual void WriteAdders(JavaWriter fw, Class classe, string tag)
    {
        if (classe.IsPersistent && Config.AssociationAdders)
        {
            foreach (var ap in classe.GetProperties(Classes).OfType<AssociationProperty>().Where(t => t.Type.IsToMany()))
            {
                var reverse = ap is ReverseAssociationProperty rap ? rap.ReverseProperty : ap.Association.GetProperties(Classes).OfType<ReverseAssociationProperty>().FirstOrDefault(r => r.ReverseProperty == ap);
                if (reverse != null)
                {
                    var propertyName = ap.NameByClassCamel;
                    fw.WriteLine();
                    fw.WriteDocStart(1, $"Add a value to {{@link {classe.GetImport(Config, tag)}#{propertyName} {propertyName}}}");
                    fw.WriteLine(1, $" * @param {ap.Association.NameCamel} value to add");
                    fw.WriteDocEnd(1);
                    fw.WriteLine(1, @$"public void add{ap.Association.NamePascal}{ap.Role}({ap.Association.NamePascal} {ap.Association.NameCamel}) {{");
                    fw.WriteLine(2, @$"this.{propertyName}.add({ap.Association.NameCamel});");
                    if (reverse.Type.IsToMany())
                    {
                        fw.WriteLine(2, @$"{ap.Association.NameCamel}.get{reverse.NameByClassPascal}().add(this);");
                    }
                    else
                    {
                        fw.WriteLine(2, @$"{ap.Association.NameCamel}.set{reverse.NameByClassPascal}(this);");
                    }

                    fw.WriteLine(1, "}");
                }
            }
        }
    }

    protected override IEnumerable<JavaAnnotation> GetAnnotations(Class classe, string tag)
    {
        foreach (var a in base.GetAnnotations(classe, tag))
        {
            yield return a;
        }

        yield return new JavaAnnotation("Entity", imports: $"{JavaxOrJakarta}.persistence.Entity");
        if (Classes.Any(c => c.Extends == classe))
        {
            yield return new JavaAnnotation("Inheritance", imports: $"{JavaxOrJakarta}.persistence.Inheritance")
                .AddAttribute("strategy", "InheritanceType.JOINED", $"{JavaxOrJakarta}.persistence.InheritanceType");
        }

        var tableAnnotation = new JavaAnnotation("Table", imports: $"{JavaxOrJakarta}.persistence.Table")
            .AddAttribute("name", $@"""{classe.SqlName}""");
        if (classe.UniqueKeys.Count > 0)
        {
            var uks = classe.UniqueKeys.Select(uk => new JavaAnnotation(
                "UniqueConstraint",
                imports: $"{JavaxOrJakarta}.persistence.UniqueConstraint")
                .AddAttribute("columnNames", uk.Select(u => $@"""{u.SqlName}""").ToArray()));

            tableAnnotation.AddAttribute("uniqueConstraints", uks);
        }

        yield return tableAnnotation;
        if (classe.PrimaryKey.Count() > 1)
        {
            yield return new JavaAnnotation("IdClass", imports: $"{JavaxOrJakarta}.persistence.IdClass")
                .AddAttribute("value", $"{classe.NamePascal}.{classe.NamePascal}Id.class");
        }

        if (classe.Reference)
        {
            var cacheAnnotation = new JavaAnnotation("Cache", imports: "org.hibernate.annotations.Cache");
            if (Config.CanClassUseEnums(classe))
            {
                yield return new JavaAnnotation("Immutable", imports: "org.hibernate.annotations.Immutable");
                cacheAnnotation.AddAttribute("usage", "CacheConcurrencyStrategy.READ_ONLY", "org.hibernate.annotations.CacheConcurrencyStrategy");
            }
            else
            {
                cacheAnnotation.AddAttribute("usage", "CacheConcurrencyStrategy.READ_WRITE", "org.hibernate.annotations.CacheConcurrencyStrategy");
            }

            yield return cacheAnnotation;
        }
    }

    protected virtual void WriteCompositePrimaryKeyClass(JavaWriter fw, Class classe, string tag)
    {
        if (classe.PrimaryKey.Count() <= 1)
        {
            return;
        }

        fw.WriteLine();
        fw.WriteLine(1, @$"public static class {classe.NamePascal}Id {{");
        foreach (var pk in classe.PrimaryKey)
        {
            fw.WriteLine();
            var annotations = new List<JavaAnnotation>();
            annotations.AddRange(JpaModelPropertyGenerator.GetDomainAnnotations(pk, tag));
            if (pk is AssociationProperty ap)
            {
                annotations.AddRange(JpaModelPropertyGenerator.GetJpaAssociationAnnotations(ap, tag));
            }
            else
            {
                annotations.Add(JpaModelPropertyGenerator.GetColumnAnnotation(pk));
            }

            fw.WriteAnnotations(2, annotations);
            fw.WriteLine(2, $"private {JpaModelPropertyGenerator.GetPropertyType(pk)} {JpaModelPropertyGenerator.GetPropertyName(pk)};");
        }

        foreach (var pk in classe.PrimaryKey)
        {
            JpaModelPropertyGenerator.WriteGetter(fw, tag, pk, 2);
            JpaModelPropertyGenerator.WriteSetter(fw, tag, pk, 2);
        }

        fw.WriteLine();
        fw.WriteLine(2, "public boolean equals(Object o) {");
        fw.WriteLine(3, "if(o == this) {");
        fw.WriteLine(4, "return true;");
        fw.WriteLine(3, "}");
        fw.WriteLine();
        fw.WriteLine(3, "if(o == null) {");
        fw.WriteLine(4, "return false;");
        fw.WriteLine(3, "}");
        fw.WriteLine();
        fw.WriteLine(3, "if(this.getClass() != o.getClass()) {");
        fw.WriteLine(4, "return false;");
        fw.WriteLine(3, "}");
        fw.WriteLine();
        fw.WriteLine(3, $"{classe.NamePascal}Id oId = ({classe.NamePascal}Id) o;");
        var associations = classe.PrimaryKey.Where(p => p is AssociationProperty || p is AliasProperty ap && ap.Property is AssociationProperty);
        if (associations.Any())
        {
            fw.WriteLine();
            fw.WriteLine(3, @$"if({string.Join(" || ", associations.Select(pk => pk.NameByClassCamel).Select(pk => $"this.{pk} == null || oId.{pk} == null"))}) {{");
            fw.WriteLine(4, "return false;");
            fw.WriteLine(3, "}");
        }

        fw.WriteLine();
        fw.WriteLine(3, $@"return {string.Join("\n && ", classe.PrimaryKey.Select(pk => $@"Objects.equals(this.{pk.NameByClassCamel}{GetterToCompareCompositePkPk(pk)}, oId.{pk.NameByClassCamel}{GetterToCompareCompositePkPk(pk)})"))};");
        fw.WriteLine(2, "}");

        fw.WriteLine();
        fw.WriteLine(2, "@Override");
        fw.WriteLine(2, "public int hashCode() {");
        fw.WriteLine(3, $"return Objects.hash({string.Join(", ", classe.PrimaryKey.Select(pk => $"{(pk is AssociationProperty || pk is AliasProperty ap && ap.Property is AssociationProperty ? $"{pk.NameByClassCamel} == null ? null : " : string.Empty)}{pk.NameByClassCamel}{GetterToCompareCompositePkPk(pk)}"))});");
        fw.AddImport("java.util.Objects");
        fw.WriteLine(2, "}");
        fw.WriteLine(1, "}");
    }

    protected virtual void WriteConstructors(Class classe, string tag, JavaWriter fw)
    {
        if (Config.MappersInClass && classe.FromMappers.Any(c => c.ClassParams.All(p => Classes.Contains(p.Class)))
            || classe.Extends != null
            || Classes.Any(c => c.Extends == classe)
            || classe.Decorators.Any(d => Config.GetImplementation(d.Decorator)?.Extends is not null))
        {
            ConstructorGenerator.WriteNoArgConstructor(fw, classe);
        }

        if (Config.MappersInClass)
        {
            ConstructorGenerator.WriteFromMappers(fw, classe, Classes, tag);
        }
    }

    protected virtual void WriteRemovers(JavaWriter fw, Class classe, string tag)
    {
        if (classe.IsPersistent && Config.AssociationRemovers)
        {
            foreach (var ap in classe.GetProperties(Classes).OfType<AssociationProperty>().Where(t => t.Type.IsToMany()))
            {
                var reverse = ap is ReverseAssociationProperty rap ? rap.ReverseProperty : ap.Association.GetProperties(Classes).OfType<ReverseAssociationProperty>().FirstOrDefault(r => r.ReverseProperty == ap);
                if (reverse != null)
                {
                    var propertyName = ap.NameByClassCamel;
                    fw.WriteLine();
                    fw.WriteDocStart(1, $"Remove a value from {{@link {classe.GetImport(Config, tag)}#{propertyName} {propertyName}}}");
                    fw.WriteLine(1, $" * @param {ap.Association.NameCamel} value to remove");
                    fw.WriteDocEnd(1);
                    fw.WriteLine(1, @$"public void remove{ap.Association.NamePascal}{ap.Role}({ap.Association.NamePascal} {ap.Association.NameCamel}) {{");
                    fw.WriteLine(2, @$"this.{propertyName}.remove({ap.Association.NameCamel});");
                    if (reverse.Type.IsToMany())
                    {
                        fw.WriteLine(2, @$"{ap.Association.NameCamel}.get{reverse.NameByClassPascal}().remove(this);");
                    }
                    else
                    {
                        fw.WriteLine(2, @$"{ap.Association.NameCamel}.set{reverse.NameByClassPascal}(null);");
                    }

                    fw.WriteLine(1, "}");
                }
            }
        }
    }
}