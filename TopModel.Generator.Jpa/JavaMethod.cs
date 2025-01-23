using TopModel.Utils;

namespace TopModel.Generator.Jpa;

public class JavaMethod
{
    public JavaMethod(string returnType, string name)
    {
        Name = name;
        ReturnType = returnType;
    }

    public JavaMethod(string import, string returnType, string name)
    {
        Name = name;
        ReturnType = returnType;
        Imports.Add(import);
    }

    public List<JavaAnnotation> Annotations { get; } = [];

    public List<WriterLine> Body { get; } = [];

    public List<string> Imports { get; } = [];

    public virtual string Signature => $@"{(!string.IsNullOrEmpty(Visibility) ? $"{Visibility} " : string.Empty)}{(Static ? "static " : string.Empty)}{(GenericTypes.Count() > 0 ? $"<{string.Join(", ", GenericTypes)}> " : string.Empty)}{ReturnType} {Name}({string.Join(", ", Parameters.Select(p => p.Declaration))})";

    public string Visibility { get; set; } = string.Empty;

    public bool Static { get; set; }

    public string Comment { get; set; } = string.Empty;

    public string ReturnComment { get; set; } = string.Empty;

    public List<JavaMethodParameter> Parameters { get; } = [];

    protected string Name { get; }

    protected string ReturnType { get; }

    protected List<string> GenericTypes { get; } = new();

    public JavaMethod AddAnnotation(JavaAnnotation annotation)
    {
        Imports.AddRange(annotation.Imports);
        Annotations.Add(annotation);
        return this;
    }

    public JavaMethod AddBodyLine(string line)
    {
        Body.Add(new WriterLine() { Line = line, Indent = 0 });
        return this;
    }

    public JavaMethod AddBodyLine(int indentationLevel, string line)
    {
        Body.Add(new WriterLine() { Line = line, Indent = indentationLevel });
        return this;
    }

    public JavaMethod AddGenericType(string type)
    {
        GenericTypes.Add(type);
        return this;
    }

    public virtual JavaMethod AddParameter(JavaMethodParameter parameter)
    {
        Imports.AddRange(parameter.Imports);
        Parameters.Add(parameter);
        return this;
    }

    public JavaMethod AddParameters(IEnumerable<JavaMethodParameter> parameters)
    {
        foreach (var parameter in parameters)
        {
            AddParameter(parameter);
        }

        return this;
    }

    public string CallWith(params string[] parameters)
    {
        return $"{Name}{string.Join(", ", parameters)}";
    }
}
