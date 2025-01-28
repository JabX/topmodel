using TopModel.Core;

namespace TopModel.Generator.Jpa;

/// <summary>
/// Générateur de fichiers de modèles JPA.
/// </summary>
public class JdbcModelPropertyGenerator(JpaConfig config, IEnumerable<Class> classes, Dictionary<string, string> newableTypes) : JpaModelPropertyGenerator(config, classes, newableTypes)
{
    private JavaAnnotation IdAnnotation => new JavaAnnotation("Id", imports: "org.springframework.data.annotation.Id");

    public override JavaAnnotation GetColumnAnnotation(IProperty property)
    {
        return new JavaAnnotation("Column", imports: "org.springframework.data.relational.core.mapping.Column").AddAttribute("value", $@"""{property.SqlName.ToLower()}""");
    }

    public override string GetPropertyName(IProperty property)
    {
        return property.NameCamel;
    }

    public override string GetPropertyType(IProperty property)
    {
        return _config.GetType(property, _classes, false);
    }

    public override void WriteProperties(JavaWriter fw, Class classe, string tag)
    {
        var properties = classe.Properties.Where(p => !(p is AssociationProperty ap && (ap.Type == AssociationType.OneToMany || ap.Type == AssociationType.ManyToMany)));
        foreach (var property in properties)
        {
            WriteProperty(fw, property, tag);
        }
    }

    protected override IEnumerable<JavaAnnotation> GetAnnotations(AliasProperty property, string tag)
    {
        if (property.PrimaryKey && property.Class.IsPersistent)
        {
            yield return IdAnnotation;
        }

        yield return GetColumnAnnotation(property);

        if (property.Required && !property.PrimaryKey && (!property.Class.IsPersistent || _config.UseJdbc))
        {
            yield return NotNullAnnotation;
        }
    }

    protected override IEnumerable<JavaAnnotation> GetAnnotations(AssociationProperty property, string tag)
    {
        if (property.Class.IsPersistent)
        {
            if (property.PrimaryKey && property.Class.PrimaryKey.Count() <= 1)
            {
                yield return IdAnnotation;
            }

            yield return GetColumnAnnotation(property);
        }
    }

    protected override IEnumerable<JavaAnnotation> GetAnnotations(IProperty property)
    {
        if (property.PrimaryKey && property.Class.IsPersistent)
        {
            yield return IdAnnotation;
        }

        yield return GetColumnAnnotation(property);

        if (property.Required && !property.PrimaryKey)
        {
            yield return NotNullAnnotation;
        }
    }
}
