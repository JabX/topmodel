namespace TopModel.Generator.Jpa;

public class JavaAnnotation
{
    public JavaAnnotation(string name, params string[] imports)
    {
        Name = name.Trim('@');
        Imports.AddRange(imports);
    }

    public JavaAnnotation(string name, string value, params string[] imports)
    {
        Name = name.Trim('@');
        Imports.AddRange(imports);
        Attributes["value"] = value;
    }

    public string Name { get; set; }

    public List<string> Imports { get; set; } = new();

    private Dictionary<string, object> Attributes { get; } = new();

    public JavaAnnotation AddAttribute(string name, string value, params string[] import)
    {
        Attributes[name] = value;
        Imports.AddRange(import);
        return this;
    }

    public JavaAnnotation AddAttribute(string name, string value)
    {
        Attributes[name] = value;
        return this;
    }

    public JavaAnnotation AddAttribute(string name, string[] value)
    {
        Attributes[name] = $@"{{{string.Join(", ", value)}}}";
        return this;
    }

    public JavaAnnotation AddAttribute(string value)
    {
        Attributes["value"] = value;
        return this;
    }

    public JavaAnnotation AddAttribute(string name, JavaAnnotation value)
    {
        Attributes[name] = value;
        Imports.AddRange(value.Imports);
        return this;
    }

    public JavaAnnotation AddAttribute(string name, IEnumerable<JavaAnnotation> value)
    {
        Attributes[name] = $@"{{{string.Join(", ", value.Select(a => a.ToString()))}}}";
        Imports.AddRange(value.SelectMany(a => a.Imports));
        return this;
    }

    public override string ToString()
    {
        var name = Name.StartsWith('@') ? Name : $"@{Name}";
        if (!Attributes.Any())
        {
            return name;
        }
        else if (Attributes.Count() == 1 && Attributes.Any(a => a.Key == "value"))
        {
            return $"{name}({Attributes.First().Value})";
        }
        else
        {
            var attributes = string.Join(", ", Attributes.Select(a => $"{a.Key} = {a.Value}"));
            return $"{name}({attributes})";
        }
    }
}