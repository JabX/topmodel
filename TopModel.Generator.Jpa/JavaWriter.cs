using System.Text;
using TopModel.Utils;

namespace TopModel.Generator.Jpa;

/// <summary>
/// FileWriter avec des méthodes spécialisées pour écrire du CJava
/// </summary>
public class JavaWriter(IFileWriter writer, string packageName) : IDisposable
{
    private readonly List<WriterLine> _toWrite = [];

    private List<string> _imports = [];

    public void AddImport(string value)
    {
        _imports.Add(value);
    }

    public void AddImports(IEnumerable<string> values)
    {
        _imports.AddRange(values);
    }

    /// <inheritdoc cref="IDisposable.Dispose" />
    void IDisposable.Dispose()
    {
        writer.IndentValue = "	";
        writer.WriteLine($"package {packageName};");
        WriteImports();
        _toWrite.ForEach(l => writer.WriteLine(l.Indent, l.Line));
        writer.Dispose();
    }

    /// <summary>
    /// Ecrit la signature de méthode avec le niveau indenté.
    /// </summary>
    /// <param name="indentationLevel">Niveau d'indentation.</param>
    /// <param name="javaMethod">Valeur à écrire dans le flux.</param>
    public void Write(int indentationLevel, JavaMethod javaMethod)
    {
        AddImports(javaMethod.Imports);
        WriteAnnotations(indentationLevel, javaMethod.Annotations);
        if (!string.IsNullOrEmpty(javaMethod.Comment))
        {
            WriteDocStart(indentationLevel, javaMethod.Comment);
            foreach (var param in javaMethod.Parameters)
            {
                WriteParam(param.Name, param.Comment);
            }

            if (!string.IsNullOrEmpty(javaMethod.ReturnComment))
            {
                WriteReturns(indentationLevel, javaMethod.ReturnComment);
            }

            WriteDocEnd(indentationLevel);
        }

        var hasBody = javaMethod.Body.Count > 0;
        _toWrite.Add(new WriterLine() { Line = @$"{javaMethod.Signature}{(hasBody ? " {" : ";")}", Indent = indentationLevel });
        foreach (var bodyLine in javaMethod.Body)
        {
            _toWrite.Add(new WriterLine() { Line = bodyLine.Line, Indent = bodyLine.Indent + indentationLevel + 1 });
        }

        if (hasBody)
        {
            _toWrite.Add(new WriterLine() { Line = "}", Indent = indentationLevel });
        }
    }

    /// <summary>
    /// Ecrit l'annotation avec le niveau indenté.
    /// </summary>
    /// <param name="indentationLevel">Niveau d'indentation.</param>
    /// <param name="javaAnnotations">Valeurs à écrire dans le flux.</param>
    public void WriteAnnotations(int indentationLevel, IEnumerable<JavaAnnotation> javaAnnotations)
    {
        foreach (var annotation in javaAnnotations.DistinctBy(e => e.Name.Split('(').First()))
        {
            WriteLine(indentationLevel, annotation);
        }
    }

    /// <summary>
    /// Ecrit la classe Java avec le niveau indenté.
    /// </summary>
    /// <param name="indentationLevel">Niveau d'indentation.</param>
    /// <param name="javaClass">Classe à écrire dans le flux.</param>
    public void WriteClass(int indentationLevel, JavaClass javaClass)
    {
        AddImports(javaClass.Imports);
        WriteAnnotations(indentationLevel, javaClass.Annotations);

        if (!string.IsNullOrEmpty(javaClass.Comment))
        {
            WriteDocStart(indentationLevel, javaClass.Comment);
            WriteDocEnd(indentationLevel);
        }

        WriteClassDeclaration(javaClass.Name, "public", null, null);

        foreach (var field in javaClass.Fields)
        {
            WriteField(indentationLevel + 1, field);
        }

        foreach (var constructor in javaClass.Constructors)
        {
            WriteConstructor(indentationLevel + 1, constructor);
        }

        foreach (var method in javaClass.Methods)
        {
            Write(indentationLevel + 1, method);
        }

        WriteLine(indentationLevel, "}");
    }

    /// <summary>
    /// Retourne le code associé à la déclaration.
    /// </summary>
    /// <param name="name">Nom de la classe.</param>
    /// <param name="modifier">Modifier.</param>
    /// <param name="inheritedClass">Classe parente.</param>
    /// <param name="implementingInterfaces">Interfaces implémentées.</param>
    public void WriteClassDeclaration(string name, string? modifier, string? inheritedClass = null, IList<string>? implementingInterfaces = null)
    {
        if (string.IsNullOrEmpty(name))
        {
            throw new ArgumentNullException(nameof(name));
        }

        var sb = new StringBuilder();

        if (string.IsNullOrEmpty(modifier))
        {
            sb.Append($"public class ");
        }
        else
        {
            sb.Append($"public {modifier} class ");
        }

        sb.Append(name);
        if (!string.IsNullOrEmpty(inheritedClass))
        {
            sb.Append($" extends {inheritedClass}");
        }

        if (implementingInterfaces is not null && implementingInterfaces.Count > 0)
        {
            sb.Append($" implements {string.Join(", ", implementingInterfaces)}");
        }

        sb.Append(" {");
        WriteLine(0, sb.ToString());
    }

    /// <summary>
    /// Ecrit la déclaration d'un constructeur.
    /// </summary>
    /// <param name="indentationLevel">Niveau d'indentation.</param>
    /// <param name="constructor">Constructeur à écrire.</param>
    public void WriteConstructor(int indentationLevel, JavaConstructor constructor)
    {
        AddImports(constructor.Imports);
        WriteAnnotations(indentationLevel, constructor.Annotations);
        if (!string.IsNullOrEmpty(constructor.Comment))
        {
            WriteDocStart(indentationLevel, constructor.Comment);
            foreach (var param in constructor.Parameters)
            {
                WriteParam(param.Name, param.Comment);
            }

            WriteDocEnd(indentationLevel);
        }

        var hasBody = constructor.Body.Count > 0;
        _toWrite.Add(new WriterLine() { Line = @$"{constructor.Signature}{(hasBody ? " {" : ";")}", Indent = indentationLevel });
        foreach (var bodyLine in constructor.Body)
        {
            _toWrite.Add(new WriterLine() { Line = bodyLine.Line, Indent = bodyLine.Indent + indentationLevel + 1 });
        }

        if (hasBody)
        {
            _toWrite.Add(new WriterLine() { Line = "}", Indent = indentationLevel });
        }
    }

    public void WriteDocEnd(int indentationLevel)
    {
        WriteLine(indentationLevel, " */");
    }

    /// <summary>
    /// Ecrit la valeur du résumé du commentaire.
    /// </summary>
    /// <param name="indentationLevel">Niveau d'indentation.</param>
    /// <param name="value">Valeur à écrire.</param>
    public void WriteDocStart(int indentationLevel, string value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            WriteLine(indentationLevel, LoadDocStart(value));
        }
    }

    /// <summary>
    /// Ecrit la déclaration d'un champ.
    /// </summary>
    /// <param name="indentationLevel">Niveau d'indentation.</param>
    /// <param name="field">Champ à écrire.</param>
    public void WriteField(int indentationLevel, JavaField field)
    {
        AddImports(field.Imports);
        WriteAnnotations(indentationLevel, field.Annotations);
        if (!string.IsNullOrEmpty(field.Comment))
        {
            WriteDocStart(indentationLevel, field.Comment);
            WriteDocEnd(indentationLevel);
        }

        _toWrite.Add(new WriterLine() { Line = field.ToString(), Indent = indentationLevel });
    }

    /// <summary>
    /// Ecrit l'annotation avec le niveau indenté.
    /// </summary>
    /// <param name="indentationLevel">Niveau d'indentation.</param>
    /// <param name="javaAnnotation">Valeur à écrire dans le flux.</param>
    public void WriteLine(int indentationLevel, JavaAnnotation javaAnnotation)
    {
        AddImports(javaAnnotation.Imports);
        _toWrite.Add(new WriterLine() { Line = javaAnnotation.ToString(), Indent = indentationLevel });
    }

    /// <summary>
    /// Ecrit l'annotation avec le niveau indenté.
    /// </summary>
    /// <param name="javaAnnotation">Valeur à écrire dans le flux.</param>
    public void WriteLine(JavaAnnotation javaAnnotation)
    {
        AddImports(javaAnnotation.Imports);
        _toWrite.Add(new WriterLine() { Line = javaAnnotation.ToString(), Indent = 0 });
    }

    /// <summary>
    /// Ecrit la chaine de caractère dans le flux.
    /// </summary>
    /// <param name="value">Valeur à écrire dans le flux.</param>
    public void WriteLine(string? value = null)
    {
        WriteLine(0, value ?? string.Empty);
    }

    /// <summary>
    /// Ecrit la chaine avec le niveau indenté.
    /// </summary>
    /// <param name="indentationLevel">Niveau d'indentation.</param>
    /// <param name="value">Valeur à écrire dans le flux.</param>
    public void WriteLine(int indentationLevel, string value)
    {
        _toWrite.Add(new WriterLine() { Line = value, Indent = indentationLevel });
    }

    /// <summary>
    /// Ecrit le commentaire de parametre.
    /// </summary>
    /// <param name="paramName">Nom du paramètre.</param>
    /// <param name="value">Valeur du paramètre.</param>
    public void WriteParam(string paramName, string value)
    {
        if (!string.IsNullOrEmpty(paramName) && !string.IsNullOrEmpty(value))
        {
            WriteLine(1, LoadParam(paramName, value));
        }
    }

    /// <summary>
    /// Ecrit le commentaire de returns.
    /// </summary>
    /// <param name="indentationLevel">Niveau d'indention.</param>
    /// <param name="value">Description du returns.</param>
    public void WriteReturns(int indentationLevel, string value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            WriteLine(indentationLevel, " *");
            WriteLine(indentationLevel, LoadReturns(value));
        }
    }

    /// <summary>
    /// Ecrit le commentaire de throws.
    /// </summary>
    /// <param name="indentationLevel">Niveau d'indention.</param>
    /// <param name="value">Description du returns.</param>
    public void WriteThrows(int indentationLevel, string value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            WriteLine(indentationLevel, LoadThrows(value));
        }
    }

    /// <summary>
    /// Retourne le commentaire du summary formatté.
    /// </summary>
    /// <param name="summary">Contenu du commentaire.</param>
    /// <returns>Code généré.</returns>
    private static string LoadDocStart(string summary)
    {
        if (string.IsNullOrEmpty(summary))
        {
            throw new ArgumentNullException(nameof(summary));
        }

        summary = summary.Trim();

        var sb = new StringBuilder();
        sb.Append("/**\n");
        sb.Append(" * " + summary.Replace("\n", "\n * "));
        if (!summary.EndsWith(".", StringComparison.OrdinalIgnoreCase))
        {
            sb.Append('.');
        }

        return sb.ToString();
    }

    /// <summary>
    /// Retourne le commentaire du param formatté.
    /// </summary>
    /// <param name="paramName">Nom du paramètre.</param>
    /// <param name="value">Description du paramètre.</param>
    /// <returns>Code généré.</returns>
    private static string LoadParam(string paramName, string value)
    {
        if (string.IsNullOrEmpty(paramName))
        {
            throw new ArgumentNullException(nameof(paramName));
        }

        if (string.IsNullOrEmpty(value))
        {
            throw new ArgumentNullException(nameof(value));
        }

        var sb = new StringBuilder();
        sb.Append(" * @param ");
        sb.Append(paramName);
        sb.Append(' ');
        sb.Append(value);
        if (!value.EndsWith(".", StringComparison.OrdinalIgnoreCase))
        {
            sb.Append('.');
        }

        return sb.ToString();
    }

    /// <summary>
    /// Retourne le commentaire du returns formatté.
    /// </summary>
    /// <param name="value">Description de la valeur retournée.</param>
    /// <returns>Code généré.</returns>
    private static string LoadReturns(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            throw new ArgumentNullException(nameof(value));
        }

        var sb = new StringBuilder();
        sb.Append(" * @return ");
        sb.Append(value);
        if (!value.EndsWith(".", StringComparison.OrdinalIgnoreCase))
        {
            sb.Append('.');
        }

        return sb.ToString();
    }

    /// <summary>
    /// Retourne le commentaire du returns formatté.
    /// </summary>
    /// <param name="value">Description de la valeur retournée.</param>
    /// <returns>Code généré.</returns>
    private static string LoadThrows(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            throw new ArgumentNullException(nameof(value));
        }

        var sb = new StringBuilder();
        sb.Append(" * @throws ");
        sb.Append(value);
        if (!value.EndsWith(".", StringComparison.OrdinalIgnoreCase))
        {
            sb.Append('.');
        }

        return sb.ToString();
    }

    /// <summary>
    /// Ajoute les imports
    /// </summary>
    /// <param name="fw">FileWriter.</param>
    private void WriteImports()
    {
        _imports = _imports.Distinct().Where(i => string.Join('.', i.Split('.').SkipLast(1).ToList()) != packageName).Distinct().ToArray().ToList();
        var currentPackage = string.Empty;
        foreach (var import in this._imports.Where(i => i.StartsWith("java") || i.StartsWith("org")).OrderBy(x => x))
        {
            var package = import.Split('.').First();
            if (package != currentPackage)
            {
                writer.WriteLine();
                currentPackage = package;
            }

            writer.WriteLine($"import {import};");
        }

        foreach (var import in this._imports.Where(i => !(i.StartsWith("java") || i.StartsWith("org"))).OrderBy(x => x))
        {
            var package = import.Split('.').First();
            if (package != currentPackage)
            {
                writer.WriteLine();
                currentPackage = package;
            }

            writer.WriteLine($"import {import};");
        }
    }
}
