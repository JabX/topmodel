namespace TopModel.ModelGenerator;

public class TmdAliasProperty : TmdProperty
{
    public required TmdProperty Alias { get; set; }

    public string? As { get; set; }
}