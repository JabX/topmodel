namespace TopModel.Generator.Jpa;

public class JavaClass(string name)
{
    public string Name { get; set; } = name;

    public List<JavaAnnotation> Annotations { get; } = [];

    public List<string> Imports { get; } = [];

    public List<JavaField> Fields { get; } = [];

    public List<JavaConstructor> Constructors { get; } = [];

    public List<JavaMethod> Methods { get; } = [];

    public string Comment { get; set; } = string.Empty;

    public JavaClass AddAnnotation(JavaAnnotation annotation)
    {
        Imports.AddRange(annotation.Imports);
        Annotations.Add(annotation);
        return this;
    }

    public JavaClass AddConstructor(JavaConstructor constructor)
    {
        Imports.AddRange(constructor.Imports);
        Constructors.Add(constructor);
        return this;
    }

    public JavaClass AddField(JavaField field)
    {
        Imports.AddRange(field.Imports);
        Fields.Add(field);
        return this;
    }

    public JavaClass AddImport(string import)
    {
        Imports.Add(import);
        return this;
    }

    public JavaClass AddMethod(JavaMethod method)
    {
        Imports.AddRange(method.Imports);
        Methods.Add(method);
        return this;
    }

    public override string ToString()
    {
        var sb = new System.Text.StringBuilder();

        if (!string.IsNullOrEmpty(Comment))
        {
            sb.AppendLine($"/**");
            sb.AppendLine($" * {Comment}");
            sb.AppendLine($" */");
        }

        foreach (var annotation in Annotations)
        {
            sb.AppendLine(annotation.ToString());
        }

        sb.AppendLine($"public class {Name}");
        sb.AppendLine("{");

        foreach (var field in Fields)
        {
            sb.AppendLine(field.ToString());
        }

        foreach (var constructor in Constructors)
        {
            sb.AppendLine(constructor.ToString());
        }

        foreach (var method in Methods)
        {
            sb.AppendLine(method.ToString());
        }

        sb.AppendLine("}");

        return sb.ToString();
    }
}