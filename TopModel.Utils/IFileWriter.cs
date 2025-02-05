namespace TopModel.Utils;

/// <summary>
/// Writer pour les fichiers dans les générateurs.
/// </summary>
public interface IFileWriter : IDisposable
{
    /// <summary>
    /// Active la lecture et l'écriture d'un entête avec un hash du fichier.
    /// </summary>
    bool EnableHeader { get; set; }

    /// <summary>
    /// Nom du fichier à écrire.
    /// </summary>
    string FileName { get; }

    /// <summary>
    /// Message à mettre dans le header.
    /// </summary>
    string HeaderMessage { get; set; }

    /// <summary>
    /// Indentation.
    /// </summary>
    string IndentValue { get; set; }

    /// <summary>
    /// Renvoie le token de début de ligne de commentaire dans le langage du fichier.
    /// </summary>
    string StartCommentToken { get; set; }

    /// <summary>
    /// Ecrit un string dans le stream.
    /// </summary>
    /// <param name="value">Chaîne de caractère.</para
    void Write(string? value);
}