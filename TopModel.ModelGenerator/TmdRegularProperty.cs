namespace TopModel.ModelGenerator;

public class TmdRegularProperty : TmdProperty
{
    public string SqlName { get; set; } = string.Empty;

    public bool PrimaryKey { get; set; }
}