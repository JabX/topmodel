namespace TopModel.Generator.Jpa;

public class JavaMethodParameter
{
    public JavaMethodParameter(string type, string name)
    {
        Name = name;
        Type = type;
    }

    public JavaMethodParameter(string import, string type, string name)
    {
        Name = name;
        Type = type;
        Imports.Add(import);
    }

    public string Declaration => $@"{(Final ? "final " : string.Empty)}{string.Join(' ', Annotations)}{(Annotations.Count() > 0 ? ' ' : string.Empty)}{Type} {Name}";

    public List<string> Imports { get; } = [];

    public List<JavaAnnotation> Annotations { get; } = [];

    public bool Final { get; set; } = false;

    public string Name { get; }

    public string Comment { get; set; } = string.Empty;

    private string Type { get; set; }

    public JavaMethodParameter AddAnnotation(JavaAnnotation annotation)
    {
        Imports.AddRange(annotation.Imports);
        Annotations.Add(annotation);
        return this;
    }

    public JavaMethodParameter AddAnnotations(IEnumerable<JavaAnnotation> annotations)
    {
        foreach (var a in annotations)
        {
            AddAnnotation(a);
        }

        return this;
    }
}