using TopModel.Core;

namespace TopModel.Generator.Jpa;

public static class ImportsJpaExtensions
{
    public static string GetImport(this Class classe, JpaConfig config, string tag)
    {
        return $"{config.GetPackageName(classe, config.GetBestClassTag(classe, tag))}.{classe.NamePascal}";
    }

    public static List<string> GetKindImports(this CompositionProperty cp, JpaConfig config, string tag)
    {
        return config.GetDomainImports(cp, config.GetBestClassTag(cp.Composition, tag)).ToList();
    }

    public static List<string> GetTypeImports(this IProperty p, JpaConfig config, string tag)
    {
        return p switch
        {
            CompositionProperty cp => cp.GetTypeImports(config, tag),
            AssociationProperty ap => ap.GetTypeImports(config, tag),
            AliasProperty ap => ap.GetTypeImports(config, tag),
            _ => p.GetRegularTypeImports(config, tag)
        };
    }

    private static List<string> GetRegularTypeImports(this IProperty rp, JpaConfig config, string tag)
    {
        var imports = new List<string>();

        imports.AddRange(config.GetDomainImports(rp, tag));

        if (rp is AliasProperty apo)
        {
            imports.AddRange(apo.GetTypeImports(config, tag));
        }
        else if (rp is RegularProperty rpr)
        {
            imports.AddRange(rpr.GetTypeImports(config, tag));
        }

        if (rp.Class != null && config.CanClassUseEnums(rp.Class, prop: rp))
        {
            imports.Add($"{config.GetEnumPackageName(rp.Class, config.GetBestClassTag(rp.Class, tag))}.{config.GetEnumName(rp, rp.Class)}");
        }

        return imports;
    }

    private static List<string> GetTypeImports(this AssociationProperty ap, JpaConfig config, string tag)
    {
        var imports = new List<string>();

        imports.AddRange(config.GetDomainImports(ap, config.GetBestClassTag(ap.Association, tag)));
        if (!config.UseJdbc && ap.Class != null && ap.Association.IsPersistent)
        {
            imports.Add(ap.Association.GetImport(config, config.GetBestClassTag(ap.Association, tag)));
        }
        else if (!(ap.Association.IsPersistent || ap.Class != null && !ap.Class.IsPersistent || ap.Class == null) && config.CanClassUseEnums(ap.Association, prop: ap.Property))
        {
            imports.Add($"{config.GetEnumPackageName(ap.Association, config.GetBestClassTag(ap.Association, tag))}.{config.GetEnumName(ap.Property, ap.Association)}");
        }

        return imports;
    }

    private static List<string> GetTypeImports(this CompositionProperty cp, JpaConfig config, string tag)
    {
        var imports = new List<string>() { cp.Composition.GetImport(config, config.GetBestClassTag(cp.Composition, tag)) };
        imports.AddRange(config.GetDomainImports(cp, config.GetBestClassTag(cp.Composition, tag)));

        return imports;
    }

    private static List<string> GetTypeImports(this AliasProperty ap, JpaConfig config, string tag)
    {
        var imports = new List<string>();

        if (config.CanClassUseEnums(ap.Property.Class, prop: ap.Property))
        {
            imports.Add($"{config.GetEnumPackageName(ap.Property.Class, config.GetBestClassTag(ap.Property.Class, tag))}.{config.GetEnumName(ap.Property, ap.Property.Class)}");
        }
        else if (ap.Property is AssociationProperty apr && apr.Association.PrimaryKey.Count() == 1 && config.CanClassUseEnums(apr.Association, prop: apr.Property))
        {
            imports.Add($"{config.GetEnumPackageName(apr.Association, config.GetBestClassTag(apr.Association, tag))}.{config.GetEnumName(apr.Property, apr.Association)}");
        }
        else if (ap.Property is CompositionProperty cp)
        {
            imports.AddRange(GetTypeImports(cp, config, tag));
        }

        imports.AddRange(config.GetDomainImports(ap, tag));

        return imports;
    }

    private static List<string> GetTypeImports(this RegularProperty rp, JpaConfig config, string tag)
    {
        var imports = new List<string>();
        if (rp.Class != null && config.CanClassUseEnums(rp.Class))
        {
            imports.Add($"{rp.Class.GetImport(config, tag)}");
        }

        imports.AddRange(config.GetDomainImports(rp, tag));

        return imports;
    }
}
