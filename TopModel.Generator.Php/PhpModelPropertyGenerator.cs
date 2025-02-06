using TopModel.Core;
using TopModel.Generator.Core;
using TopModel.Utils;

namespace TopModel.Generator.Php;

/// <summary>
/// Générateur de fichiers de modèles JPA.
/// </summary>
public class PhpModelPropertyGenerator(PhpConfig config, IEnumerable<Class> classes)
{
    public void WriteProperties(PhpWriter fw, Class classe, IEnumerable<Class> availableClasses, string tag)
    {
        var isFirst = true;
        foreach (var property in classe.GetProperties(availableClasses))
        {
            if (!isFirst)
            {
                fw.WriteLine();
            }

            isFirst = false;
            WriteProperty(fw, classe, property, tag);
        }
    }

    public void WriteProperty(PhpWriter fw, CompositionProperty property)
    {
        fw.WriteLine(1, $"private {config.GetType(property)} ${property.NameCamel};");
    }

    public void WriteProperty(PhpWriter fw, Class classe, IProperty property, string tag)
    {
        switch (property)
        {
            case CompositionProperty cp:
                WriteProperty(fw, cp);
                break;
            case AssociationProperty { Association.IsPersistent: true } ap:
                WriteProperty(fw, classe, ap, tag);
                break;
            default:
                WriteRegularProperty(fw, classe, property, tag);
                break;
        }
    }

    private static void WriteManyToMany(PhpWriter fw, Class classe, AssociationProperty property)
    {
        fw.AddImport(@$"Doctrine\ORM\Mapping\ManyToMany");
        fw.AddImport(@$"Doctrine\ORM\Mapping\InverseJoinColumn");
        fw.AddImport(@$"Doctrine\ORM\Mapping\JoinColumn");

        var role = property.Role is not null ? "_" + property.Role.ToConstantCase() : string.Empty;
        var fk = property.Property.SqlName;
        var pk = classe.PrimaryKey.Single().SqlName + role;

        fw.WriteLine(1, @$"#[JoinColumn(name: '{pk}', referencedColumnName: '{pk}')]");
        fw.WriteLine(1, @$"#[InverseJoinColumn(name: '{fk}', referencedColumnName: '{fk}')]");
        fw.WriteLine(1, @$"#[ManyToMany(targetEntity: {property.Association.NamePascal}::class)]");
    }

    private static void WriteManyToOne(PhpWriter fw, AssociationProperty property)
    {
        var fk = ((IProperty)property).SqlName;
        var apk = property.Property.SqlName;
        fw.AddImport(@$"Doctrine\ORM\Mapping\ManyToOne");
        fw.AddImport(@$"Doctrine\ORM\Mapping\JoinColumn");
        fw.WriteLine(1, @$"#[ManyToOne(targetEntity: {property.Association}::class)]");
        fw.WriteLine(1, @$"#[JoinColumn(name: '{fk}', referencedColumnName: '{apk}')]");
    }

    private static void WriteOneToMany(PhpWriter fw, AssociationProperty property)
    {
        fw.AddImport(@$"Doctrine\ORM\Mapping\OneToMany");
        fw.WriteLine(1, @$"#[OneToMany(mappedBy: '{(property is ReverseAssociationProperty rap ? rap.ReverseProperty.NameByClassCamel : @$"{property.Class.NameCamel}{property.Role ?? string.Empty}")}', targetEntity: {property.Association.Name}::class)]");
    }

    private static void WriteOneToOne(PhpWriter fw, AssociationProperty property)
    {
        var fk = ((IProperty)property).SqlName;
        var apk = property.Property.SqlName;
        fw.AddImport(@$"Doctrine\ORM\Mapping\OneToOne");
        fw.AddImport(@$"Doctrine\ORM\Mapping\JoinColumn");
        fw.WriteLine(1, @$"#[OneToOne(targetEntity: {property.Association.NamePascal}::class)]");
        fw.WriteLine(1, @$"#[JoinColumn(name: '{fk}', referencedColumnName: '{apk}')]");
    }

    private void WriteProperty(PhpWriter fw, Class classe, AssociationProperty property, string tag)
    {
        if (property.Type.IsToMany())
        {
            fw.AddImport(@"Doctrine\Common\Collections\Collection");
            fw.WriteDocStart(1, @$"@var Collection<{property.Association}>");
            fw.WriteDocEnd(1);
        }

        fw.AddImport(property.Association.GetImport(config, tag));
        switch (property.Type)
        {
            case AssociationType.ManyToOne:
                WriteManyToOne(fw, property);
                break;
            case AssociationType.OneToMany:
                WriteOneToMany(fw, property);
                break;
            case AssociationType.ManyToMany:
                WriteManyToMany(fw, classe, property);
                break;
            case AssociationType.OneToOne:
                WriteOneToOne(fw, property);
                break;
        }

        fw.WriteLine(1, $"private {config.GetType(property, classes, classe.IsPersistent)} ${property.NameByClassCamel};");
    }

    private void WriteRegularProperty(PhpWriter fw, Class classe, IProperty property, string tag)
    {
        if (classe.IsPersistent)
        {
            if (property.PrimaryKey)
            {
                fw.WriteLine(1, @"#[Id]");
                fw.AddImport(@$"Doctrine\ORM\Mapping\Id");
                if (property.Domain.AutoGeneratedValue)
                {
                    fw.AddImport(@$"Doctrine\ORM\Mapping\GeneratedValue");
                    var strategy = config.Identity.Mode == IdentityMode.NONE ? string.Empty : $@"(strategy: ""{config.Identity.Mode}"")";
                    fw.WriteLine(1, @$"#[GeneratedValue{strategy}]");
                    if (config.Identity.Mode == IdentityMode.SEQUENCE)
                    {
                        var seqName = $"SEQ_{classe.SqlName}";
                        var initialValue = config.Identity.Start != null ? $", initialValue: {config.Identity.Start}" : string.Empty;
                        var increment = config.Identity.Increment != null ? $", allocationSize: {config.Identity.Increment}" : string.Empty;
                        fw.AddImport(@$"Doctrine\ORM\Mapping\SequenceGenerator");
                        fw.WriteLine(1, @$"#[SequenceGenerator(sequenceName: ""{seqName}""{initialValue}{increment})]");
                    }
                }
            }

            fw.AddImport(@$"Doctrine\ORM\Mapping\Column");
            var column = $@"name: '{property.SqlName}'";
            if (property.Domain.Length != null)
            {
                column += $", length: {property.Domain.Length}";
            }

            if (property.Domain.Scale != null)
            {
                column += $", scale: {property.Domain.Scale}";
            }

            if (!property.Required)
            {
                column += $", nullable: true";
            }

            fw.WriteLine(1, $@"#[Column({column})]");
        }
        else
        {
            if (property.Required && !property.PrimaryKey)
            {
                fw.WriteLine(1, $@"#[Symfony\Component\Validator\Constraints\NotNull]");
                fw.AddImport(@$"NotNull");
            }

            if (property.Domain.Length != null)
            {
                fw.WriteLine(1, $@"#[Length(max: {property.Domain.Length})]");
                fw.AddImport(@$"Symfony\Component\Validator\Constraints\Length");
            }
        }

        foreach (var annotation in config.GetDomainAnnotations(property, tag))
        {
            fw.WriteLine(1, annotation);
        }

        var defaultValue = config.GetValue(property, classes);
        var suffix = defaultValue != "null" ? $" = {defaultValue}" : string.Empty;
        if (property is AliasProperty ap && ap.Property is AssociationProperty asp && asp.Type.IsToMany())
        {
            fw.AddImport(@"Doctrine\Common\Collections\Collection");
        }

        fw.WriteLine(1, $"private {config.GetType(property, classes, classe.IsPersistent)}{(property.Required ? string.Empty : "|null")} ${property.NameByClassCamel}{suffix};");
    }
}