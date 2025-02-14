using TopModel.Core;
using TopModel.Generator.Core;
using TopModel.Utils;

namespace TopModel.Generator.Jpa.ClassGeneration;

/// <summary>
/// Générateur de fichiers de modèles JPA.
/// </summary>
public class JpaModelPropertyGenerator(JpaConfig config, IEnumerable<Class> classes, Dictionary<string, string> newableTypes)
{
    protected IEnumerable<Class> Classes { get; } = classes;

    protected JpaConfig Config { get; } = config;

    protected Dictionary<string, string> NewableTypes { get; } = newableTypes;

    protected virtual string JavaxOrJakarta => Config.JavaxOrJakarta;

    protected virtual JavaAnnotation NotNullAnnotation => new("NotNull", imports: $"{JavaxOrJakarta}.validation.constraints.NotNull");

    protected virtual JavaAnnotation ValidAnnotation => new("Valid", imports: $"{JavaxOrJakarta}.validation.Valid");

    protected JavaAnnotation EnumAnnotation =>
        new JavaAnnotation("Enumerated", imports: $"{JavaxOrJakarta}.persistence.Enumerated")
            .AddAttribute("value", "EnumType.STRING", $"{JavaxOrJakarta}.persistence.EnumType");

    public virtual JavaAnnotation GetColumnAnnotation(IProperty property)
    {
        JavaAnnotation column = new JavaAnnotation("Column", imports: $"{JavaxOrJakarta}.persistence.Column")
            .AddAttribute("name", $@"""{property.SqlName}""");
        if (property.Required)
        {
            column.AddAttribute("nullable", "false");
        }

        if (property.Domain != null)
        {
            if (property.Domain.Length != null)
            {
                if (Config.GetImplementation(property.Domain)?.Type?.ToUpper() == "STRING")
                {
                    column.AddAttribute("length", $"{property.Domain.Length}");
                }
                else
                {
                    column.AddAttribute("precision", $"{property.Domain.Length}");
                }
            }

            if (property.Domain.Scale != null)
            {
                column.AddAttribute("scale", $"{property.Domain.Scale}");
            }

            column.AddAttribute("columnDefinition", @$"""{property.Domain.Implementations["sql"].Type}""");
        }

        if (property is CompositionProperty && property.Domain is null)
        {
            column.AddAttribute("columnDefinition", @$"""jsonb""");
        }

        return column;
    }

    public virtual IEnumerable<JavaAnnotation> GetDomainAnnotations(IProperty property, string tag)
    {
        foreach (var (annotation, imports) in Config.GetDomainAnnotationsAndImports(property, tag))
        {
            yield return new JavaAnnotation(name: annotation, imports: imports.ToArray());
        }

        if (!property.Class.IsPersistent && !(property is AssociationProperty ap && ap.Type.IsToMany()))
        {
            var propertyType = GetPropertyType(property);
            List<string> sizePropertyValidateTypes = [
                "String",
                "CharSequence",
                "Set",
                "Map",
                "List",
                "Collection"
            ];
            var shouldAddSizeAnnotation = property.Domain.Length != null && (sizePropertyValidateTypes.Contains(propertyType.Split("<").First()) || propertyType.EndsWith("[]"));

            if (shouldAddSizeAnnotation)
            {
                yield return new JavaAnnotation(name: "Size", imports: [$"{JavaxOrJakarta}.validation.constraints.Size"])
                    .AddAttribute("max", value: property.Domain.Length.ToString());
            }

            // Techniquement Digit peut aussi être mis sur des chaînes de caractères, mais ce n'est pas forcément l'intention de l'utilisateurs
            List<string> digitPropertyValidateTypes = [
                "BigDecimal",
                "BigInteger",
                "byte",
                "short",
                "int",
                "long",
                "Byte",
                "Short",
                "Integer",
                "Long",
                "double",
                "Double"
            ];
            var shouldAddDigitsAnnotation = (property.Domain.Length != null || property.Domain.Scale != null) && digitPropertyValidateTypes.Contains(propertyType);
            if (shouldAddDigitsAnnotation)
            {
                var digitsAnnotation = new JavaAnnotation(name: "Digits", imports: [$"{JavaxOrJakarta}.validation.constraints.Digits"]);
                if (property.Domain.Length != null)
                {
                    digitsAnnotation.AddAttribute("integer", value: property.Domain.Length.ToString());
                }

                if (property.Domain.Scale != null)
                {
                    digitsAnnotation.AddAttribute("fraction", value: property.Domain.Scale.ToString());
                }

                yield return digitsAnnotation;
            }
        }
    }

    public virtual string GetGetterName(IProperty property)
    {
        var propertyName = GetPropertyName(property);
        var propertyType = GetPropertyType(property);
        var getterPrefix = propertyType == "boolean" ? "is" : "get";
        if (property.Class.PreservePropertyCasing)
        {
            return propertyName.ToFirstUpper().WithPrefix(getterPrefix);
        }

        return propertyName.ToPascalCase().WithPrefix(getterPrefix);
    }

    public IEnumerable<JavaAnnotation> GetJpaAssociationAnnotations(AssociationProperty property, string tag)
    {
        return property.Type switch
        {
            AssociationType.ManyToOne => GetManyToOneAnnotations(property, tag),
            AssociationType.OneToMany => GetOneToManyAnnotations(property),
            AssociationType.ManyToMany => GetManyToManyAnnotations(property),
            AssociationType.OneToOne => GetOneToOneAnnotations(property),
            _ => [],
        };
    }

    public virtual string GetPropertyName(IProperty property)
    {
        var isAssociationNotPersistent = property is AssociationProperty apr && !apr.Association.IsPersistent;
        return isAssociationNotPersistent ? property.NameCamel : property.NameByClassCamel;
    }

    public virtual string GetPropertyType(IProperty property)
    {
        var isAssociationNotPersistent = property is AssociationProperty apr && !apr.Association.IsPersistent;
        var useClassForAssociation = property.Class.IsPersistent && !isAssociationNotPersistent;
        return Config.GetType(property, Classes, useClassForAssociation);
    }

    public virtual string GetSetterName(IProperty property)
    {
        var propertyName = GetPropertyName(property);
        if (property.Class.PreservePropertyCasing)
        {
            return propertyName.WithPrefix("set");
        }

        return propertyName.ToPascalCase().WithPrefix("set");
    }

    public virtual void WriteGetter(JavaWriter fw, string tag, IProperty property, int indentLevel = 1)
    {
        var propertyName = GetPropertyName(property);
        var propertyType = GetPropertyType(property);
        fw.WriteLine();
        string getterName = GetGetterName(property);
        var method = new JavaMethod(propertyType, getterName)
        {
            Visibility = "public",
            Comment = $"Getter for {propertyName}",
            ReturnComment = $"value of {{@link {property.Class.GetImport(Config, tag)}#{propertyName} {propertyName}}}"
        };
        var genericType = propertyType.Split('<').First();
        if (NewableTypes.TryGetValue(genericType, out var newableType) && property.Class.IsPersistent)
        {
            fw.AddImport($"java.util.{newableType}");
            method
                .AddBodyLine($"if(this.{propertyName} == null) {{")
                .AddBodyLine(1, $"this.{propertyName} = new {newableType}<>();")
                .AddBodyLine($"}}");
        }

        method.AddBodyLine(@$"return this.{propertyName};");
        fw.Write(indentLevel, method);
    }

    public virtual void WriteProperties(JavaWriter fw, Class classe, string tag)
    {
        foreach (var property in classe.GetProperties(Classes))
        {
            WriteProperty(fw, property, tag);
        }
    }

    public virtual void WriteProperty(JavaWriter fw, IProperty property, string tag)
    {
        fw.WriteLine();
        fw.WriteDocStart(1, property.Comment);
        IEnumerable<JavaAnnotation> annotations = GetAnnotations(property, tag);
        if (property is AliasProperty ap && Classes.Contains(ap.Property.Class))
        {
            fw.WriteLine(1, $" * Alias of {{@link {ap.Property.Class.GetImport(Config, tag)}#get{GetPropertyName(ap.Property).ToFirstUpper()}() {ap.Property.Class.NamePascal}#get{GetPropertyName(ap.Property).ToFirstUpper()}()}} ");
        }

        fw.WriteDocEnd(1);

        if (!property.PrimaryKey || property.Class.PrimaryKey.Count() <= 1)
        {
            annotations = GetDomainAnnotations(property, tag).Concat(annotations).ToList();
        }

        fw.WriteAnnotations(1, annotations);
        string defaultValue = GetDefaultValue(property);
        fw.AddImports(GetDefaultValueImports(property, tag));
        fw.AddImports(property.GetTypeImports(Config, tag));
        fw.WriteLine(1, $"private {GetPropertyType(property)} {GetPropertyName(property)}{defaultValue};");
    }

    public virtual void WriteSetter(JavaWriter fw, string tag, IProperty property, int indentLevel = 1)
    {
        var propertyName = GetPropertyName(property);
        fw.WriteLine();
        var method = new JavaMethod("void", GetSetterName(property))
        {
            Visibility = "public",
            Comment = $"Set the value of {{@link {property.Class.GetImport(Config, tag)}#{propertyName} {propertyName}}}"
        }
            .AddParameter(new JavaMethodParameter(GetPropertyType(property), propertyName)
            {
                Comment = $"value to set"
            })
            .AddBodyLine(@$"this.{propertyName} = {propertyName};");
        fw.Write(indentLevel, method);
    }

    protected virtual IEnumerable<JavaAnnotation> GetAnnotations(CompositionProperty property, string tag)
    {
        if (property.Class.IsPersistent)
        {
            yield return GetConvertAnnotation(property, tag);
            yield return GetColumnAnnotation(property);
        }
        else
        {
            yield return ValidAnnotation;
            if (property.Required && !property.PrimaryKey)
            {
                yield return NotNullAnnotation;
            }
        }

        foreach (var a in GetDomainAnnotations(property, tag))
        {
            yield return a;
        }
    }

    protected virtual IEnumerable<JavaAnnotation> GetAnnotations(AliasProperty property, string tag)
    {
        if (property.Class.IsPersistent)
        {
            var shouldWriteAssociation = property.Property is AssociationProperty ap && ap.Association.IsPersistent;
            if (property.PrimaryKey && property.Class.IsPersistent)
            {
                foreach (var a in GetIdAnnotations(property))
                {
                    yield return a;
                }
            }

            if (shouldWriteAssociation)
            {
                foreach (var a in GetJpaAssociationAnnotations((AssociationProperty)property.Property, tag))
                {
                    yield return a;
                }
            }
            else if (!(property.PrimaryKey && property.Class.PrimaryKey.Count() > 1))
            {
                yield return GetColumnAnnotation(property);
            }

            if (property.Property is CompositionProperty cp)
            {
                foreach (var a in GetAnnotations(cp, tag))
                {
                    yield return a;
                }
            }

            if (Config.CanClassUseEnums(property.Property.Class, Classes, property.Property))
            {
                yield return EnumAnnotation;
            }
        }
        else if (property.Required && !property.PrimaryKey)
        {
            yield return NotNullAnnotation;
        }
    }

    protected virtual IEnumerable<JavaAnnotation> GetAnnotations(AssociationProperty property, string tag)
    {
        if (property.Class.IsPersistent)
        {
            if (property.Association.IsPersistent && !(Config.EnumsAsEnums && Config.CanClassUseEnums(property.Property.Class, Classes, property.Property)))
            {
                if (!property.PrimaryKey || property.Class.PrimaryKey.Count() <= 1)
                {
                    foreach (var a in GetJpaAssociationAnnotations(property, tag))
                    {
                        yield return a;
                    }
                }

                if (property.Type == AssociationType.ManyToMany || property.Type == AssociationType.OneToMany)
                {
                    if (property.Association.OrderProperty != null && GetPropertyType(property).Contains("List"))
                    {
                        yield return new JavaAnnotation("OrderBy", $@"""{property.Association.OrderProperty.NameByClassCamel} ASC""", $"{JavaxOrJakarta}.persistence.OrderBy");
                    }
                }

                if (property.PrimaryKey)
                {
                    foreach (var a in GetIdAnnotations(property))
                    {
                        yield return a;
                    }
                }
            }
            else
            {
                yield return GetColumnAnnotation(property);
                if (Config.CanClassUseEnums(property.Property.Class, Classes, property.Property) && property.Class.IsPersistent)
                {
                    yield return EnumAnnotation;
                }
            }
        }
    }

    protected virtual IEnumerable<JavaAnnotation> GetAnnotations(IProperty property)
    {
        if (property.Class.IsPersistent)
        {
            if (property.PrimaryKey)
            {
                foreach (var a in GetIdAnnotations(property))
                {
                    yield return a;
                }
            }

            if (!(property.PrimaryKey && property.Class.PrimaryKey.Count() > 1))
            {
                yield return GetColumnAnnotation(property);
            }

            if (Config.CanClassUseEnums(property.Class, Classes, property))
            {
                yield return EnumAnnotation;
            }
        }
        else if (property.Required && !property.PrimaryKey)
        {
            yield return NotNullAnnotation;
        }
    }

    protected virtual IEnumerable<JavaAnnotation> GetAnnotations(IProperty property, string tag)
    {
        return property switch
        {
            AliasProperty alp => GetAnnotations(alp, tag),
            AssociationProperty ap => GetAnnotations(ap, tag),
            CompositionProperty cp => GetAnnotations(cp, tag),
            IProperty ip => GetAnnotations(ip),
            _ => [],
        };
    }

    protected virtual IEnumerable<JavaAnnotation> GetAutogeneratedAnnotations(Class classe)
    {
        var autoGenerated = new JavaAnnotation("GeneratedValue", imports: $"{JavaxOrJakarta}.persistence.GeneratedValue");
        if (Config.Identity.Mode == IdentityMode.IDENTITY)
        {
            autoGenerated.AddAttribute("strategy", "GenerationType.IDENTITY", $"{JavaxOrJakarta}.persistence.GenerationType");
        }
        else if (Config.Identity.Mode == IdentityMode.SEQUENCE)
        {
            var seqName = $"SEQ_{classe.SqlName}";
            autoGenerated
                .AddAttribute("strategy", "GenerationType.SEQUENCE", $"{JavaxOrJakarta}.persistence.GenerationType")
                .AddAttribute("generator", $@"""{seqName}""");
            var sequenceGenerator = new JavaAnnotation("SequenceGenerator", imports: $"{JavaxOrJakarta}.persistence.SequenceGenerator")
                .AddAttribute("sequenceName", $@"""{seqName}""")
                .AddAttribute("name", $@"""{seqName}""");
            if (Config.Identity.Start != null)
            {
                sequenceGenerator.AddAttribute("initialValue", $"{Config.Identity.Start}");
            }

            if (Config.Identity.Increment != null)
            {
                sequenceGenerator.AddAttribute("allocationSize", $"{Config.Identity.Increment}");
            }

            yield return sequenceGenerator;
        }

        yield return autoGenerated;
    }

    protected virtual JavaAnnotation GetConvertAnnotation(CompositionProperty property, string tag)
    {
        var convert = new JavaAnnotation("Convert", imports: $"{JavaxOrJakarta}.persistence.Convert");
        var import = Config.CompositionConverterCanonicalName
            .Replace("{class}", property.Composition.Name)
            .Replace("{package}", Config.GetPackageName(property.Composition, Config.GetBestClassTag(property.Composition, tag)));
        convert.AddAttribute("converter", $"{Config.CompositionConverterSimpleName.Replace("{class}", property.Composition.Name)}.class", import);
        return convert;
    }

    protected virtual string GetDefaultValue(IProperty property)
    {
        var defaultValue = Config.GetValue(property, Classes);
        if (property is AssociationProperty ap)
        {
            if (ap.Association.PrimaryKey.Count() == 1 && Config.CanClassUseEnums(ap.Association, Classes, prop: ap.Association.PrimaryKey.Single()))
            {
                if (defaultValue != "null")
                {
                    if (Config.EnumsAsEnums)
                    {
                        return $" = {defaultValue}";
                    }
                    else
                    {
                        return $" = new {ap.Association.NamePascal}({defaultValue})";
                    }
                }
            }

            return string.Empty;
        }
        else
        {
            var suffix = defaultValue != "null" ? $" = {defaultValue}" : string.Empty;
            return suffix;
        }
    }

    protected virtual IEnumerable<string> GetDefaultValueImports(IProperty property, string tag)
    {
        var defaultValue = Config.GetValue(property, Classes);
        if (property is AssociationProperty ap)
        {
            if (ap.Association.PrimaryKey.Count() == 1 && Config.CanClassUseEnums(ap.Association, Classes, prop: ap.Association.PrimaryKey.Single()))
            {
                if (defaultValue != "null")
                {
                    return [$"{Config.GetEnumPackageName(property.Class, Config.GetBestClassTag(property.Class, tag))}.{GetPropertyType(ap.Association.PrimaryKey.Single())}"];
                }
            }

            return [];
        }
        else
        {
            return Config.GetValueImports(property, tag);
        }
    }

    protected virtual IEnumerable<JavaAnnotation> GetIdAnnotations(IProperty property)
    {
        if (property.Domain.AutoGeneratedValue && property.Class.PrimaryKey.Count() == 1)
        {
            foreach (var a in GetAutogeneratedAnnotations(property.Class))
            {
                yield return a;
            }
        }

        yield return new JavaAnnotation("Id", imports: $"{JavaxOrJakarta}.persistence.Id");
    }

    protected virtual IEnumerable<JavaAnnotation> GetManyToManyAnnotations(AssociationProperty property)
    {
        var role = property.Role is not null ? "_" + property.Role.ToConstantCase() : string.Empty;
        var fk = ((IProperty)property).SqlName;
        var pk = property.Class.PrimaryKey.Single().SqlName + role;
        var association = new JavaAnnotation($"{property.Type}", imports: $"{JavaxOrJakarta}.persistence.{property.Type}")
            .AddAttribute("fetch", "FetchType.LAZY", $"{JavaxOrJakarta}.persistence.FetchType");
        if (!Config.CanClassUseEnums(property.Association))
        {
            association.AddAttribute("cascade", "{ CascadeType.PERSIST, CascadeType.MERGE }", $"{JavaxOrJakarta}.persistence.CascadeType");
        }

        if (property is ReverseAssociationProperty rap)
        {
            association.AddAttribute("mappedBy", $@"""{rap.ReverseProperty.NameByClassCamel}""");
        }

        yield return association;

        if (property is not ReverseAssociationProperty)
        {
            var joinColumns = new JavaAnnotation("JoinColumn", imports: $"{JavaxOrJakarta}.persistence.JoinColumn").AddAttribute("name", $@"""{pk}""");
            var inverseJoinColumns = new JavaAnnotation("JoinColumn", imports: $"{JavaxOrJakarta}.persistence.JoinColumn").AddAttribute("name", $@"""{fk}""");
            var joinTable = new JavaAnnotation("JoinTable", imports: $"{JavaxOrJakarta}.persistence.JoinTable")
                .AddAttribute("name", $@"""{property.Class.SqlName}_{property.Association.SqlName}{(property.Role != null ? "_" + property.Role.ToConstantCase() : string.Empty)}""")
                .AddAttribute("joinColumns", joinColumns)
                .AddAttribute("inverseJoinColumns", inverseJoinColumns);
            yield return joinTable;
        }
    }

    protected virtual IEnumerable<JavaAnnotation> GetManyToOneAnnotations(AssociationProperty property, string tag)
    {
        var association = new JavaAnnotation(@$"{property.Type}", imports: $"{JavaxOrJakarta}.persistence.{property.Type}")
            .AddAttribute("fetch", "FetchType.LAZY", $"{JavaxOrJakarta}.persistence.FetchType")
            .AddAttribute("optional", property.Required ? "false" : "true")
            .AddAttribute("targetEntity", $"{property.Association.NamePascal}.class", property.Association.GetImport(Config, Config.GetBestClassTag(property.Association, tag)));
        yield return association;

        var fk = ((IProperty)property).SqlName;
        var apk = property.Property.SqlName;
        var joinColumn = new JavaAnnotation("JoinColumn", imports: $"{JavaxOrJakarta}.persistence.JoinColumn")
            .AddAttribute("name", $@"""{fk}""")
            .AddAttribute("referencedColumnName", $@"""{apk}""");
        yield return joinColumn;
    }

    protected virtual IEnumerable<JavaAnnotation> GetOneToManyAnnotations(AssociationProperty property)
    {
        var association = new JavaAnnotation(@$"{property.Type}", imports: $"{JavaxOrJakarta}.persistence.{property.Type}");
        if (property is ReverseAssociationProperty rap)
        {
            association
                .AddAttribute("cascade", "{CascadeType.PERSIST, CascadeType.MERGE}", $"{JavaxOrJakarta}.persistence.CascadeType")
                .AddAttribute("fetch", "FetchType.LAZY", $"{JavaxOrJakarta}.persistence.FetchType")
                .AddAttribute("mappedBy", $@"""{rap.ReverseProperty.NameByClassCamel}""");
        }
        else
        {
            var pk = property.Class.PrimaryKey.Single().SqlName;
            var hasReverse = property.Class.Namespace.RootModule == property.Association.Namespace.RootModule;

            association
                .AddAttribute("cascade", "CascadeType.ALL", $"{JavaxOrJakarta}.persistence.CascadeType")
                .AddAttribute("fetch", "FetchType.LAZY", $"{JavaxOrJakarta}.persistence.FetchType");
            if (hasReverse)
            {
                association.AddAttribute("mappedBy", @$"""{property.Class.NameCamel}{property.Role ?? string.Empty}""");
            }
            else
            {
                var joinColumn = new JavaAnnotation("JoinColumn", imports: $"{JavaxOrJakarta}.persistence.JoinColumn")
                    .AddAttribute("name", $@"""{pk}""")
                    .AddAttribute("referencedColumnName", $@"""{pk}""");
                yield return joinColumn;
            }
        }

        yield return association;
    }

    protected virtual IEnumerable<JavaAnnotation> GetOneToOneAnnotations(AssociationProperty property)
    {
        var fk = ((IProperty)property).SqlName;
        var apk = property.Property.SqlName;
        var association = new JavaAnnotation(@$"{property.Type}", imports: $"{JavaxOrJakarta}.persistence.{property.Type}")
                .AddAttribute("fetch", "FetchType.LAZY", $"{JavaxOrJakarta}.persistence.FetchType")
                .AddAttribute("cascade", @"CascadeType.ALL", $"{JavaxOrJakarta}.persistence.CascadeType")
                .AddAttribute("optional", (!property.Required).ToString().ToLower());
        yield return association;

        var joinColumn = new JavaAnnotation("JoinColumn", imports: $"{JavaxOrJakarta}.persistence.JoinColumn")
            .AddAttribute("name", $@"""{fk}""")
            .AddAttribute("referencedColumnName", $@"""{apk}""")
            .AddAttribute("unique", "true");
        yield return joinColumn;
    }
}
