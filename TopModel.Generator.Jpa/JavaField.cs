using System.Collections.Generic;
using System.Linq;

namespace TopModel.Generator.Jpa;

public class JavaField
{
    public JavaField(string type, string name)
    {
        Type = type;
        Name = name;
    }

    public string Type { get; set; }

    public string Name { get; set; }

    public List<JavaAnnotation> Annotations { get; } = new();

    public List<string> Imports { get; } = new();

    public string Comment { get; set; } = string.Empty;

    public JavaField AddAnnotation(JavaAnnotation annotation)
    {
        Imports.AddRange(annotation.Imports);
        Annotations.Add(annotation);
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

        sb.AppendLine($"private {Type} {Name};");

        return sb.ToString();
    }
}