namespace TopModel.ModelGenerator;

public class TmdProperty
{
#nullable disable
    public string Name { get; set; }

    public string CompositionReference { get; set; }

    public TmdClass Class { get; set; }

    public string Comment { get; set; } = "Non documenté";

    public string Domain { get; set; }

    public bool Required { get; set; }
}
