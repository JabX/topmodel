using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Extensions.Logging;

namespace TopModel.Utils;

/// <summary>
/// Classe de base pour l'écriture des fichiers générés.
/// </summary>
public class GeneratedFileWriter : IDisposable
{
    /// <summary>
    /// Nombre de lignes d'en-tête à ignorer dans le calcul de checksum.
    /// </summary>
    private const int LinesInHeader = 4;

    private readonly ConfigBase _config;
    private readonly Encoding _encoding;
    private readonly ILogger _logger;
    private readonly StringBuilder _sb;

    internal GeneratedFileWriter(ConfigBase config, string fileName, ILogger logger, bool encoderShouldEmitUTF8Identifier)
        : this(config, fileName, logger, new UTF8Encoding(encoderShouldEmitUTF8Identifier))
    {
    }

    internal GeneratedFileWriter(ConfigBase config, string fileName, ILogger logger, Encoding encoding)
    {
        _config = config;
        _encoding = encoding;
        _logger = logger;
        FileName = fileName ?? throw new ArgumentNullException(nameof(fileName));
        _sb = new StringBuilder();
    }

    /// <summary>
    /// Nom du fichier à écrire.
    /// </summary>
    public string FileName { get; }

    /// <summary>
    /// Active la lecture et l'écriture d'un entête avec un hash du fichier.
    /// </summary>
    public bool EnableHeader { get; set; } = true;

    /// <summary>
    /// Message à mettre dans le header.
    /// </summary>
    public string HeaderMessage { get; set; } = "ATTENTION CE FICHIER EST GENERE AUTOMATIQUEMENT !";

    /// <summary>
    /// Renvoie le token de début de ligne de commentaire dans le langage du fichier.
    /// </summary>
    /// <returns>Token de début de ligne de commentaire.</returns>
    public string StartCommentToken { get; set; } = "////";

    /// <summary>
    /// Indentation.
    /// </summary>
    public string IndentValue { get; set; } = "    ";

    /// <summary>
    /// Libère les ressources.
    /// </summary>
    public void Dispose()
    {
        if (Marshal.GetExceptionPointers() != IntPtr.Zero)
        {
            return;
        }

        string? currentContent = null;
        var fileExists = File.Exists(FileName.Replace("\\", "/"));
        if (fileExists)
        {
            using var reader = new StreamReader(FileName, _encoding);

            if (EnableHeader)
            {
                for (var i = 0; i < LinesInHeader; i++)
                {
                    var line = reader.ReadLine();
                }
            }

            currentContent = reader.ReadToEnd();
        }

        var newContent = _sb.ToString();
        if (newContent.ReplaceLineEndings() == currentContent?.ReplaceLineEndings())
        {
            return;
        }

        /* Création du répertoire si inexistant. */
        var dir = new FileInfo(FileName).DirectoryName!;
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        using (var sw = new StreamWriter(FileName, false, _encoding))
        {
            if (EnableHeader && !newContent.StartsWith($"{StartCommentToken}{Environment.NewLine}"))
            {
                sw.WriteLine(StartCommentToken);
                sw.WriteLine($"{StartCommentToken} {HeaderMessage}");
                sw.WriteLine(StartCommentToken);
                sw.WriteLine();
            }

            sw.Write(newContent);
        }

        _logger.LogInformation($"{(fileExists ? "Modifié:  " : "Créé:     ")}{FileName.ToRelative()}");
    }

    /// <summary>
    /// Ecrit un caractère dans le stream.
    /// </summary>
    /// <param name="value">Caractère.</param>
    public void Write(char value)
    {
        _sb.Append(value);
    }

    /// <summary>
    /// Ecrit un string dans le stream.
    /// </summary>
    /// <param name="value">Chaîne de caractère.</param>
    public void Write(string? value)
    {
        _sb.Append(value);
    }

    /// <summary>
    /// Ecrit un caractère dans le stream.
    /// </summary>
    /// <param name="indentationLevel">Indentation.</param>
    /// <param name="value">Caractère.</param>
    public void Write(int indentationLevel, string value)
    {
        var indentValue = string.Empty;
        for (var i = 0; i < indentationLevel; ++i)
        {
            indentValue += IndentValue;
        }

        value = value.Replace("\n", "\n" + indentValue);
        Write(indentValue + value);
    }

    /// <summary>
    /// Ecrit un ligne dans le stream.
    /// </summary>
    /// <param name="value">Chaîne de caractère.</param>
    public void WriteLine(string? value = null)
    {
        _sb.Append((value ?? string.Empty) + Environment.NewLine);
    }

    /// <summary>
    /// Ecrit la chaine avec le niveau indenté.
    /// </summary>
    /// <param name="indentationLevel">Niveau d'indentation.</param>
    /// <param name="value">Valeur à écrire dans le flux.</param>
    public void WriteLine(int indentationLevel, string value)
    {
        var indentValue = string.Empty;
        for (var i = 0; i < indentationLevel; ++i)
        {
            indentValue += IndentValue;
        }

        value = value.Replace("\n", "\n" + indentValue);
        WriteLine(indentValue + value);
    }
}