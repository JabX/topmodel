using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Extensions.Logging;

namespace TopModel.Utils;

/// <summary>
/// Classe de base pour l'écriture des fichiers générés.
/// </summary>
[Obsolete("Utiliser GeneretedFileWriter via OpenFileWriter() à la place.")]
public class FileWriter : TextWriter
{
    /// <summary>
    /// Nombre de lignes d'en-tête à ignorer dans le calcul de checksum.
    /// </summary>
    private const int LinesInHeader = 4;

    private readonly string _fileName;
    private readonly ILogger _logger;
    private readonly StringBuilder _sb;

    /// <summary>
    /// Crée une nouvelle instance.
    /// </summary>
    /// <param name="fileName">Nom du fichier à écrire.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="encoderShouldEmitUTF8Identifier">UTF8 avec BOM ?</param>
    public FileWriter(string fileName, ILogger logger, bool encoderShouldEmitUTF8Identifier = true)
        : this(fileName, logger, new UTF8Encoding(encoderShouldEmitUTF8Identifier))
    {
    }

    /// <summary>
    /// Crée une nouvelle instance.
    /// </summary>
    /// <param name="fileName">Nom du fichier à écrire.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="encoding">Encoding</param>
    public FileWriter(string fileName, ILogger logger, Encoding encoding)
        : base(CultureInfo.InvariantCulture)
    {
        _logger = logger;
        _fileName = fileName ?? throw new ArgumentNullException(nameof(fileName));
        _sb = new StringBuilder();
        Encoding = encoding;
    }

    /// <summary>
    /// Retourne l'encodage à utiliser.
    /// </summary>
    public override Encoding Encoding { get; }

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
    public virtual string StartCommentToken { get; init; } = "////";

    /// <summary>
    /// Indentation.
    /// </summary>
    public string IndentValue { get; init; } = "    ";

    /// <summary>
    /// Ecrit un caractère dans le stream.
    /// </summary>
    /// <param name="value">Caractère.</param>
    public override void Write(char value)
    {
        _sb.Append(value);
    }

    /// <summary>
    /// Ecrit un string dans le stream.
    /// </summary>
    /// <param name="value">Chaîne de caractère.</param>
    public override void Write(string? value)
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
    public override void WriteLine(string? value)
    {
        _sb.Append(value + NewLine);
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
        this.WriteLine(indentValue + value);
    }

    /// <summary>
    /// Libère les ressources.
    /// </summary>
    /// <param name="disposing">True.</param>
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (Marshal.GetExceptionPointers() != IntPtr.Zero)
        {
            return;
        }

        string? currentContent = null;
        var fileExists = File.Exists(_fileName.Replace("\\", "/"));
        if (fileExists)
        {
            using var reader = new StreamReader(_fileName, Encoding);

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
        var dir = new FileInfo(_fileName).DirectoryName!;
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        using (var sw = new StreamWriter(_fileName, false, Encoding))
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

        _logger.LogInformation($"{(fileExists ? "Modifié:  " : "Créé:     ")}{_fileName.ToRelative()}");
    }
}