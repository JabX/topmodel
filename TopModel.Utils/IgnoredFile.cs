namespace TopModel.Utils;

public class IgnoredFile
{
    /// <summary>
    /// Chemin du fichier à ignorer, relatif au fichier de config.
    /// </summary>
    public required string Path { get; set; }

    /// <summary>
    /// Motif.
    /// </summary>
    public required string Comment { get; set; }
}
