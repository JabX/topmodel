namespace TopModel.Generator.Jpa;

public class JavaConstructor : JavaMethod
{
    public JavaConstructor(string returnType)
        : base(returnType, string.Empty)
    {
    }

    public override string Signature => $@"{(!string.IsNullOrEmpty(Visibility) ? $"{Visibility} " : string.Empty)}{(GenericTypes.Count() > 0 ? $"<{string.Join(", ", GenericTypes)}> " : string.Empty)}{ReturnType}({string.Join(", ", Parameters.Select(p => p.Declaration))})";

    public override JavaMethod AddParameter(JavaMethodParameter parameter)
    {
        Imports.AddRange(parameter.Imports);
        Parameters.Add(parameter);
        AddBodyLine($@"this.{parameter.Name} = {parameter.Name};");
        return this;
    }
}
