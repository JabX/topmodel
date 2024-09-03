using TopModel.Core;
using TopModel.Core.Model.Implementation;
using TopModel.Generator.Core;
using TopModel.Utils;

namespace TopModel.Generator.Jpa;

/// <summary>
/// Générateur de fichiers de modèles JPA.
/// </summary>
public class JpaModelPropertyGenerator
{
    private readonly IEnumerable<Class> _classes;
    private readonly JpaConfig _config;
    private readonly Dictionary<string, string> _newableTypes;

    public JpaModelPropertyGenerator(JpaConfig config, IEnumerable<Class> classes, Dictionary<string, string> newableTypes)
    {
        _classes = classes;
        _config = config;
        _newableTypes = newableTypes;
    }

    public void WriteCompositePrimaryKeyClass(JavaWriter fw, Class classe, string tag)
    {
        if (classe.PrimaryKey.Count() <= 1 || !classe.IsPersistent)
        {
            return;
        }

        fw.WriteLine();
        fw.WriteLine(1, @$"public static class {classe.NamePascal}Id {{");
        foreach (var pk in classe.PrimaryKey)
        {
            fw.WriteLine();
            if (pk is AssociationProperty ap)
            {
                WriteAssociationAnnotations(fw, classe, ap, 2);
            }
            else if (ShouldWriteColumnAnnotation(classe, pk))
            {
                WriteColumnAnnotation(fw, pk, 2);
            }

            WriteDomainAnnotations(fw, pk, tag, 2);
            fw.WriteLine(2, $"private {_config.GetType(pk, _classes, true)} {pk.NameByClassCamel};");
        }

        foreach (var pk in classe.PrimaryKey)
        {
            WriteGetter(fw, classe, tag, pk, 2);
            WriteSetter(fw, classe, tag, pk, 2);
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

    public void WriteCompositionProperty(JavaWriter fw, CompositionProperty property, string tag)
    {
        fw.WriteDocEnd(1);
        if (property.Class.IsPersistent)
        {
            WriteConvertAnnotation(fw, property, 1, tag);
            WriteColumnAnnotation(fw, property, 1);
        }

        fw.AddImport(property.Composition.GetImport(_config, tag));
        fw.WriteLine(1, $"private {_config.GetType(property)} {property.NameCamel};");
    }

    public void WriteGetter(JavaWriter fw, Class classe, string tag, IProperty property, int indentLevel = 1)
    {
        var isAssociationNotPersistent = property is AssociationProperty apr && !apr.Association.IsPersistent;
        var propertyName = _config.UseJdbc || isAssociationNotPersistent ? property.NameCamel : property.NameByClassCamel;
        fw.WriteLine();
        fw.WriteDocStart(indentLevel, $"Getter for {propertyName}");
        fw.WriteReturns(indentLevel, $"value of {{@link {classe.GetImport(_config, tag)}#{propertyName} {propertyName}}}");
        fw.WriteDocEnd(indentLevel);

        var getterPrefix = _config.GetType(property, _classes, true) == "boolean" ? "is" : "get";
        var getterName = propertyName.ToPascalCase().WithPrefix(getterPrefix);
        if (property.Class.PreservePropertyCasing)
        {
            getterName = propertyName.ToFirstUpper().WithPrefix(getterPrefix);
        }

        var useClassForAssociation = classe.IsPersistent && !isAssociationNotPersistent && !_config.UseJdbc;
        fw.WriteLine(indentLevel, @$"public {_config.GetType(property, useClassForAssociation: useClassForAssociation)} {getterName}() {{");
        if (property is AssociationProperty ap && ap.Type.IsToMany())
        {
            var type = _config.GetType(ap, _classes, useClassForAssociation: useClassForAssociation).Split('<').First();
            if (_newableTypes.TryGetValue(type, out var newableType))
            {
                fw.WriteLine(indentLevel + 1, $"if(this.{propertyName} == null) {{");
                fw.AddImport($"java.util.{newableType}");
                fw.WriteLine(indentLevel + 2, $"this.{propertyName} = new {newableType}<>();");
                fw.WriteLine(indentLevel + 1, $"}}");
            }
        }

        fw.WriteLine(indentLevel + 1, @$"return this.{propertyName};");
        fw.WriteLine(indentLevel, "}");
    }

    public void WriteProperties(JavaWriter fw, Class classe, string tag)
    {
        var properties = _config.UseJdbc ? classe.Properties.Where(p => !(p is AssociationProperty ap && (ap.Type == AssociationType.OneToMany || ap.Type == AssociationType.ManyToMany))) : classe.GetProperties(_classes);
        foreach (var property in properties)
        {
            WriteProperty(fw, classe, property, tag);
        }
    }

    public void WriteProperty(JavaWriter fw, Class classe, IProperty property, string tag)
    {
        fw.WriteLine();
        fw.WriteDocStart(1, property.Comment);
        switch (property)
        {
            case CompositionProperty cp:
                WriteCompositionProperty(fw, cp, tag);
                break;
            case AssociationProperty { Association.IsPersistent: true } ap when !(_config.EnumsAsEnum && _config.CanClassUseEnums(ap.Association, _classes)):
                WriteAssociationProperty(fw, classe, ap, tag);
                break;
            case AliasProperty alp:
                WriteAliasProperty(fw, classe, alp, tag);
                break;
            default:
                WriteIFieldProperty(fw, classe, property, tag);
                break;
        }
    }

    public void WriteSetter(JavaWriter fw, Class classe, string tag, IProperty property, int indentLevel = 1)
    {
        var isAssociationNotPersistent = property is AssociationProperty apr && !apr.Association.IsPersistent;
        var propertyName = _config.UseJdbc || isAssociationNotPersistent ? property.NameCamel : property.NameByClassCamel;
        fw.WriteLine();
        fw.WriteDocStart(indentLevel, $"Set the value of {{@link {classe.GetImport(_config, tag)}#{propertyName} {propertyName}}}");
        fw.WriteLine(indentLevel, $" * @param {propertyName} value to set");
        fw.WriteDocEnd(indentLevel);
        var useClassForAssociation = classe.IsPersistent && !isAssociationNotPersistent && !_config.UseJdbc;
        fw.WriteLine(indentLevel, @$"public void {propertyName.WithPrefix("set")}({_config.GetType(property, useClassForAssociation: useClassForAssociation)} {propertyName}) {{");
        fw.WriteLine(indentLevel + 1, @$"this.{propertyName} = {propertyName};");
        fw.WriteLine(indentLevel, "}");
    }

    private static void WriteEnumAnnotation(JavaWriter fw, string javaOrJakarta)
    {
        fw.AddImports(new List<string>
            {
                $"{javaOrJakarta}.persistence.Enumerated",
                $"{javaOrJakarta}.persistence.EnumType",
            });
        fw.WriteLine(1, "@Enumerated(EnumType.STRING)");
    }

    private static void WriteValidationAnnotations(JavaWriter fw, string javaOrJakarta)
    {
        fw.WriteLine(1, @$"@NotNull");
        fw.AddImport($"{javaOrJakarta}.validation.constraints.NotNull");
    }

    private string GetterToCompareCompositePkPk(IProperty pk)
    {
        if (pk is AssociationProperty ap)
        {
            return $".get{ap.Property.NamePascal}()";
        }
        else if (pk is AliasProperty al && al.Property is AssociationProperty asp)
        {
            return $".get{asp.Property.NamePascal}()";
        }

        return string.Empty;
    }

    private bool ShouldWriteColumnAnnotation(Class classe, IProperty property)
    {
        return !(_config.EnumsAsEnum && _config.CanClassUseEnums(classe, _classes)) && (classe.IsPersistent || _config.UseJdbc) && (property.Domain is null || !_config.GetImplementation(property.Domain)!.Annotations
                .Where(i =>
                        classe.IsPersistent && (Target.Persisted & i.Target) > 0
                    || !classe.IsPersistent && (Target.Dto & i.Target) > 0)
                    .Any(a => a.Text.Replace("@", string.Empty).StartsWith("Column")));
    }

    private void WriteAliasProperty(JavaWriter fw, Class classe, AliasProperty property, string tag)
    {
        if (_classes.Contains(property.Property.Class))
        {
            fw.WriteLine(1, $" * Alias of {{@link {property.Property.Class.GetImport(_config, tag)}#get{property.Property.NameCamel.ToFirstUpper()}() {property.Property.Class.NamePascal}#get{property.Property.NameCamel.ToFirstUpper()}()}} ");
        }

        fw.WriteDocEnd(1);
        var javaOrJakarta = _config.PersistenceMode.ToString().ToLower();
        var shouldWriteAssociation = classe.IsPersistent && property.Property is AssociationProperty ap && !(_config.EnumsAsEnum && _config.CanClassUseEnums(ap.Class, _classes));

        if (ShouldWriteIdAnnotation(classe, property))
        {
            WriteIdAnnotation(fw, classe, property);
        }

        if (!shouldWriteAssociation && ShouldWriteColumnAnnotation(classe, property) && (_config.UseJdbc || !(property.PrimaryKey && classe.PrimaryKey.Count() > 1)))
        {
            WriteColumnAnnotation(fw, property, 1);
        }

        if (shouldWriteAssociation)
        {
            WriteAssociationAnnotations(fw, classe, (AssociationProperty)property.Property, 1);
        }

        if (property.Property is CompositionProperty cp)
        {
            fw.AddImport(cp.Composition.GetImport(_config, tag));
            if (classe.IsPersistent)
            {
                WriteConvertAnnotation(fw, cp, 1, tag);
            }
        }

        if (property.Required && !property.PrimaryKey && (!classe.IsPersistent || _config.UseJdbc))
        {
            WriteValidationAnnotations(fw, javaOrJakarta);
        }

        if (_config.CanClassUseEnums(property.Property.Class) && !_config.EnumsAsEnum && property.Property.PrimaryKey && classe.IsPersistent && !_config.UseJdbc)
        {
            WriteEnumAnnotation(fw, javaOrJakarta);
        }

        if (property.Domain is not null && (!property.PrimaryKey || classe.PrimaryKey.Count() <= 1))
        {
            WriteDomainAnnotations(fw, property, tag, 1);
        }

        var defaultValue = _config.GetValue(property, _classes);
        var suffix = defaultValue != "null" ? $" = {defaultValue}" : string.Empty;
        var isAssociationNotPersistent = property.Property is AssociationProperty asp && !asp.Association.IsPersistent;
        var useClassForAssociation = classe.IsPersistent && !isAssociationNotPersistent && !_config.UseJdbc;
        fw.WriteLine(1, $"private {_config.GetType(property, useClassForAssociation: useClassForAssociation)} {(isAssociationNotPersistent && !shouldWriteAssociation ? property.NameCamel : property.NameByClassCamel)}{suffix};");
    }

    private void WriteAssociationAnnotations(JavaWriter fw, Class classe, AssociationProperty property, int indentLevel)
    {
        switch (property.Type)
        {
            case AssociationType.ManyToOne:
                WriteManyToOneAnnotations(fw, property, indentLevel);
                break;
            case AssociationType.OneToMany:
                WriteOneToManyAnnotations(fw, classe, property, indentLevel);
                break;
            case AssociationType.ManyToMany:
                WriteManyToManyAnnotations(fw, classe, property, indentLevel);
                break;
            case AssociationType.OneToOne:
                WriteOneToOneAnnotations(fw, property, indentLevel);
                break;
        }
    }

    private void WriteAssociationProperty(JavaWriter fw, Class classe, AssociationProperty property, string tag)
    {
        fw.WriteDocEnd(1);
        if (!_config.UseJdbc)
        {
            var javaOrJakarta = _config.PersistenceMode.ToString().ToLower();
            fw.AddImport($"{javaOrJakarta}.persistence.FetchType");
            fw.AddImport($"{javaOrJakarta}.persistence.{property.Type}");

            if ((!property.PrimaryKey || classe.PrimaryKey.Count() <= 1) && !(_config.EnumsAsEnum && _config.CanClassUseEnums(property.Class, _classes)))
            {
                WriteAssociationAnnotations(fw, classe, property, 1);
            }

            if (property.Type == AssociationType.ManyToMany || property.Type == AssociationType.OneToMany)
            {
                if (property.Association.OrderProperty != null && _config.GetType(property, _classes, classe.IsPersistent).Contains("List"))
                {
                    fw.WriteLine(1, @$"@OrderBy(""{property.Association.OrderProperty.NameByClassCamel} ASC"")");
                    fw.AddImport($"{javaOrJakarta}.persistence.OrderBy");
                }
            }

            var suffix = string.Empty;
            if (property.Association.PrimaryKey.Count() == 1 && _config.CanClassUseEnums(property.Association, _classes, prop: property.Association.PrimaryKey.Single()))
            {
                var defaultValue = _config.GetValue(property, _classes);
                if (defaultValue != "null")
                {
                    fw.AddImport($"{_config.GetEnumPackageName(classe, _config.GetBestClassTag(classe, tag))}.{_config.GetType(property.Association.PrimaryKey.Single())}");
                    suffix = $" = new {property.Association.NamePascal}({defaultValue})";
                }
            }

            if (property.PrimaryKey && !(_config.EnumsAsEnum && _config.CanClassUseEnums(property.Class, _classes)))
            {
                fw.AddImport($"{javaOrJakarta}.persistence.Id");
                fw.WriteLine(1, "@Id");
            }

            var isAssociationNotPersistent = !property.Association.IsPersistent;
            var useClassForAssociation = classe.IsPersistent && !isAssociationNotPersistent && !_config.UseJdbc;
            fw.WriteLine(1, $"private {_config.GetType(property, useClassForAssociation: useClassForAssociation)} {property.NameByClassCamel}{suffix};");
        }
        else
        {
            if (property.PrimaryKey && classe.PrimaryKey.Count() <= 1)
            {
                fw.AddImport("org.springframework.data.annotation.Id");
                fw.WriteLine(1, "@Id");
            }

            fw.AddImport("org.springframework.data.relational.core.mapping.Column");
            fw.WriteLine(1, $@"@Column(""{((IProperty)property).SqlName.ToLower()}"")");
            fw.WriteLine(1, $"private {_config.GetType(property)} {property.NameCamel};");
        }
    }

    private void WriteAutogeneratedAnnotations(JavaWriter fw, Class classe, string javaOrJakarta)
    {
        fw.AddImports(new List<string>
                {
                    $"{javaOrJakarta}.persistence.GeneratedValue",
                    $"{javaOrJakarta}.persistence.GenerationType"
                });

        if (_config.Identity.Mode == IdentityMode.IDENTITY)
        {
            fw.WriteLine(1, @$"@GeneratedValue(strategy = GenerationType.IDENTITY)");
        }
        else if (_config.Identity.Mode == IdentityMode.SEQUENCE)
        {
            fw.AddImport($"{javaOrJakarta}.persistence.SequenceGenerator");
            var seqName = $"SEQ_{classe.SqlName}";
            var initialValue = _config.Identity.Start != null ? $", initialValue = {_config.Identity.Start}" : string.Empty;
            var increment = _config.Identity.Increment != null ? $", allocationSize = {_config.Identity.Increment}" : string.Empty;
            fw.WriteLine(1, @$"@SequenceGenerator(name = ""{seqName}"", sequenceName = ""{seqName}""{initialValue}{increment})");
            fw.WriteLine(1, @$"@GeneratedValue(strategy = GenerationType.SEQUENCE, generator = ""{seqName}"")");
        }
    }

    private void WriteColumnAnnotation(JavaWriter fw, IProperty property, int indentLevel)
    {
        var javaOrJakarta = _config.PersistenceMode.ToString().ToLower();
        string column;
        if (!_config.UseJdbc)
        {
            var nullable = property.Required ? ", nullable = false" : string.Empty;
            column = @$"@Column(name = ""{property.SqlName}""{nullable}";
            if (property.Domain != null)
            {
                if (property.Domain.Length != null)
                {
                    if (_config.GetImplementation(property.Domain)?.Type?.ToUpper() == "STRING")
                    {
                        column += $", length = {property.Domain.Length}";
                    }
                    else
                    {
                        column += $", precision = {property.Domain.Length}";
                    }
                }

                if (property.Domain.Scale != null)
                {
                    column += $", scale = {property.Domain.Scale}";
                }

                column += @$", columnDefinition = ""{property.Domain.Implementations["sql"].Type}""";
            }

            if (property is CompositionProperty && property.Domain is null)
            {
                column += @$", columnDefinition = ""jsonb""";
            }

            column += ")";
            fw.AddImport($"{javaOrJakarta}.persistence.Column");
        }
        else
        {
            fw.AddImport("org.springframework.data.relational.core.mapping.Column");
            column = $@"@Column(""{property.SqlName.ToLower()}"")";
        }

        fw.WriteLine(indentLevel, column);
    }

    private void WriteConvertAnnotation(JavaWriter fw, CompositionProperty property, int indentLevel, string tag)
    {
        var javaOrJakarta = _config.PersistenceMode.ToString().ToLower();
        fw.AddImport($"{javaOrJakarta}.persistence.Convert");
        fw.AddImport(_config.CompositionConverterCanonicalName
            .Replace("{class}", property.Composition.Name)
            .Replace("{package}", _config.GetPackageName(property.Composition, _config.GetBestClassTag(property.Composition, tag))));
        fw.WriteLine(indentLevel, $"@Convert(converter = {_config.CompositionConverterSimpleName.Replace("{class}", property.Composition.Name)}.class)");
    }

    private void WriteDomainAnnotations(JavaWriter fw, IProperty property, string tag, int indentLevel)
    {
        foreach (var annotation in _config.GetDomainAnnotations(property, tag))
        {
            fw.WriteLine(indentLevel, $"{(annotation.StartsWith('@') ? string.Empty : '@')}{annotation}");
        }
    }

    private void WriteIFieldProperty(JavaWriter fw, Class classe, IProperty property, string tag)
    {
        var javaOrJakarta = _config.PersistenceMode.ToString().ToLower();

        fw.WriteDocEnd(1);
        if (ShouldWriteIdAnnotation(classe, property))
        {
            WriteIdAnnotation(fw, classe, property);
        }

        if (ShouldWriteColumnAnnotation(classe, property) && (_config.UseJdbc || !(property.PrimaryKey && classe.PrimaryKey.Count() > 1)))
        {
            WriteColumnAnnotation(fw, property, 1);
        }

        if (property is AssociationProperty ap && _config.EnumsAsEnum && _config.CanClassUseEnums(ap.Association, _classes))
        {
            WriteConvertEnumAnnotation(fw, ap, tag);
        }

        if (property.Required && !property.PrimaryKey && (!classe.IsPersistent || _config.UseJdbc))
        {
            WriteValidationAnnotations(fw, javaOrJakarta);
        }

        if (_config.CanClassUseEnums(classe) && property.PrimaryKey && !_config.UseJdbc && !_config.EnumsAsEnum)
        {
            WriteEnumAnnotation(fw, javaOrJakarta);
        }

        if (!property.PrimaryKey || classe.PrimaryKey.Count() <= 1)
        {
            WriteDomainAnnotations(fw, property, tag, 1);
        }

        var defaultValue = _config.GetValue(property, _classes);
        var suffix = defaultValue != "null" ? $" = {defaultValue}" : string.Empty;
        var isAssociationNotPersistent = property is AssociationProperty asp && !asp.Association.IsPersistent;
        var useClassForAssociation = classe.IsPersistent && !isAssociationNotPersistent && !_config.UseJdbc;
        fw.WriteLine(1, $"private {_config.GetType(property, useClassForAssociation: useClassForAssociation)} {(isAssociationNotPersistent ? property.NameCamel : property.NameByClassCamel)}{suffix};");
    }

    private void WriteConvertEnumAnnotation(JavaWriter fw, AssociationProperty property, string tag)
    {
        var javaOrJakarta = _config.PersistenceMode.ToString().ToLower();
        fw.AddImport($"{javaOrJakarta}.persistence.Convert");
        var packageName = _config.ResolveVariables(
       _config.EnumConverterPath!,
       tag,
       module: property.Association.Namespace.Module).ToPackageName();
        fw.AddImport($"{packageName}.{_config.GetType(property, useClassForAssociation: true)}Converter");
        fw.WriteLine(1, $"@Convert(converter = {_config.GetType(property, useClassForAssociation: true)}Converter.class)");
    }

    private bool ShouldWriteIdAnnotation(Class classe, IProperty property)
    {
        return property.PrimaryKey && classe.IsPersistent && !(_config.EnumsAsEnum && _config.CanClassUseEnums(property.Class, _classes));
    }

    private void WriteIdAnnotation(JavaWriter fw, Class classe, IProperty property)
    {
        var javaOrJakarta = _config.PersistenceMode.ToString().ToLower();
        if (!_config.UseJdbc)
        {
            fw.AddImport($"{javaOrJakarta}.persistence.Id");

            if (property.Domain.AutoGeneratedValue && classe.PrimaryKey.Count() == 1)
            {
                WriteAutogeneratedAnnotations(fw, classe, javaOrJakarta);
            }
        }
        else
        {
            fw.AddImport("org.springframework.data.annotation.Id");
        }

        fw.WriteLine(1, "@Id");
    }

    private void WriteManyToManyAnnotations(JavaWriter fw, Class classe, AssociationProperty property, int indentLevel)
    {
        var role = property.Role is not null ? "_" + property.Role.ToConstantCase() : string.Empty;
        var fk = ((IProperty)property).SqlName;
        var pk = classe.PrimaryKey.Single().SqlName + role;
        var javaOrJakarta = _config.PersistenceMode.ToString().ToLower();
        if (!_config.CanClassUseEnums(property.Association))
        {
            fw.AddImport($"{javaOrJakarta}.persistence.CascadeType");
        }

        var cascade = _config.CanClassUseEnums(property.Association) ? string.Empty : $", cascade = {{ CascadeType.PERSIST, CascadeType.MERGE }}";
        if (property is ReverseAssociationProperty rap)
        {
            fw.WriteLine(indentLevel, @$"@{property.Type}(fetch = FetchType.LAZY, mappedBy = ""{rap.ReverseProperty.NameByClassCamel}""{cascade})");
        }
        else
        {
            fw.AddImport($"{javaOrJakarta}.persistence.JoinTable");
            fw.AddImport($"{javaOrJakarta}.persistence.JoinColumn");
            fw.WriteLine(indentLevel, @$"@{property.Type}(fetch = FetchType.LAZY{cascade})");
            fw.WriteLine(indentLevel, @$"@JoinTable(name = ""{property.Class.SqlName}_{property.Association.SqlName}{(property.Role != null ? "_" + property.Role.ToConstantCase() : string.Empty)}"", joinColumns = @JoinColumn(name = ""{pk}""), inverseJoinColumns = @JoinColumn(name = ""{fk}""))");
        }
    }

    private void WriteManyToOneAnnotations(JavaWriter fw, AssociationProperty property, int indentLevel)
    {
        var fk = ((IProperty)property).SqlName;
        var apk = property.Property.SqlName;
        var javaOrJakarta = _config.PersistenceMode.ToString().ToLower();
        fw.WriteLine(indentLevel, @$"@{property.Type}(fetch = FetchType.LAZY, optional = {(property.Required ? "false" : "true")}, targetEntity = {property.Association.NamePascal}.class)");
        fw.WriteLine(indentLevel, @$"@JoinColumn(name = ""{fk}"", referencedColumnName = ""{apk}"")");
        fw.AddImport($"{javaOrJakarta}.persistence.JoinColumn");
    }

    private void WriteOneToManyAnnotations(JavaWriter fw, Class classe, AssociationProperty property, int indentLevel)
    {
        var javaOrJakarta = _config.PersistenceMode.ToString().ToLower();
        fw.AddImport($"{javaOrJakarta}.persistence.CascadeType");
        if (property is ReverseAssociationProperty rap)
        {
            fw.WriteLine(1, @$"@{property.Type}(cascade = {{CascadeType.PERSIST, CascadeType.MERGE}}, fetch = FetchType.LAZY, mappedBy = ""{rap.ReverseProperty.NameByClassCamel}"")");
        }
        else
        {
            var pk = classe.PrimaryKey.Single().SqlName;
            var hasReverse = property.Class.Namespace.RootModule == property.Association.Namespace.RootModule;
            fw.WriteLine(indentLevel, @$"@{property.Type}(cascade = CascadeType.ALL, fetch = FetchType.LAZY{(hasReverse ? @$", mappedBy = ""{property.Class.NameCamel}{property.Role ?? string.Empty}""" : string.Empty)})");
            if (!hasReverse)
            {
                fw.WriteLine(indentLevel, @$"@JoinColumn(name = ""{pk}"", referencedColumnName = ""{pk}"")");
                fw.AddImport($"{javaOrJakarta}.persistence.JoinColumn");
            }
        }
    }

    private void WriteOneToOneAnnotations(JavaWriter fw, AssociationProperty property, int indentLevel)
    {
        var fk = ((IProperty)property).SqlName;
        var apk = property.Property.SqlName;
        var javaOrJakarta = _config.PersistenceMode.ToString().ToLower();
        fw.AddImport($"{javaOrJakarta}.persistence.CascadeType");
        fw.WriteLine(indentLevel, @$"@{property.Type}(fetch = FetchType.LAZY, cascade = CascadeType.ALL, optional = {(!property.Required).ToString().ToLower()})");
        fw.WriteLine(indentLevel, @$"@JoinColumn(name = ""{fk}"", referencedColumnName = ""{apk}"", unique = true)");
        fw.AddImport($"{javaOrJakarta}.persistence.JoinColumn");
    }
}
