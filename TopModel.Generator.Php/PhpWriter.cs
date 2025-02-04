using System.Text;
using TopModel.Utils;

namespace TopModel.Generator.Php;

/// <summary>
/// FileWriter avec des méthodes spécialisées pour écrire du CPhp
/// </summary>
public class PhpWriter(GeneratedFileWriter writer, string packageName) : IDisposable
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
        writer.IndentValue = "  ";
        writer.EnableHeader = false;

        writer.WriteLine($"<?php");
        writer.WriteLine(writer.StartCommentToken);
        writer.WriteLine($"{writer.StartCommentToken} {writer.HeaderMessage}");
        writer.WriteLine(writer.StartCommentToken);
        writer.WriteLine();
        writer.WriteLine($"namespace {packageName};");
        writer.WriteLine();

        WriteImports();
        _toWrite.ForEach(l => writer.WriteLine(l.Indent, l.Line));
        writer.Dispose();
    }

    /// <summary>
    /// Ecrit un attribut de décoration.
    /// </summary>
    /// <param name="indentLevel">Indentation.</param>
    /// <param name="attributeName">Nom de l'attribut.</param>
    /// <param name="attributeParams">Paramètres.</param>
    public void WriteAttribute(int indentLevel, string attributeName, params string[] attributeParams)
    {
        var aParams = string.Empty;

        if (attributeParams.Length > 0)
        {
            aParams = $@"({string.Join(", ", attributeParams)})";
        }

        WriteLine(indentLevel, $@"@{attributeName}{aParams}");
    }

    /// <summary>
    /// Retourne le code associé à la déclaration.
    /// </summary>
    /// <param name="name">Nom de la classe.</param>
    /// <param name="modifier">Modifier.</param>
    /// <param name="inheritedClass">Classe parente.</param>
    /// <param name="implementingInterfaces">Interfaces implémentées.</param>
    public void WriteClassDeclaration(string name, string? modifier, string? inheritedClass = null, IEnumerable<string>? implementingInterfaces = null)
    {
        if (string.IsNullOrEmpty(name))
        {
            throw new ArgumentNullException(nameof(name));
        }

        var sb = new StringBuilder();

        if (string.IsNullOrEmpty(modifier))
        {
            sb.Append($"class ");
        }
        else
        {
            sb.Append($"{modifier} class ");
        }

        sb.Append(name);
        if (!string.IsNullOrEmpty(inheritedClass))
        {
            sb.Append($" extends {inheritedClass}");
        }

        if (implementingInterfaces is not null && implementingInterfaces.Any())
        {
            sb.Append($" implements {string.Join(", ", implementingInterfaces)}");
        }

        WriteLine(sb.ToString());
        WriteLine("{");
    }

    public void WriteDocEnd(int indentationLevel)
    {
        WriteLine(indentationLevel, " */");
    }

    /// <summary>
    /// Ecrit la valeur du résumé du commentaire..
    /// </summary>
    /// <param name="indentationLevel">Niveau d'indentation.</param>
    public void WriteDocStart(int indentationLevel)
    {
        WriteLine(indentationLevel, LoadDocStart());
    }

    /// <summary>
    /// Ecrit la valeur du résumé du commentaire..
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
    private static string LoadDocStart()
    {
        var sb = new StringBuilder();
        sb.Append("/**");
        return sb.ToString();
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
        _imports = _imports.Distinct().Where(i => string.Join('\\', i.Split('\\').SkipLast(1).ToList()) != packageName).ToList();

        foreach (var import in this._imports.OrderBy(x => x))
        {
            var package = import.Split('.').First();
            writer.WriteLine($"use {import};");
        }

        writer.WriteLine();
    }
}