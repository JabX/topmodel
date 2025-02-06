using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;
using TopModel.Core;
using TopModel.Core.FileModel;
using TopModel.Generator.Core;
using TopModel.Utils;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace TopModel.Generator.Csharp;

public class CSharpApiServerGenerator(ILogger<CSharpApiServerGenerator> logger, IFileWriterProvider writerProvider)
    : EndpointsGeneratorBase<CsharpConfig>(logger, writerProvider)
{
    public override string Name => "CSharpApiServerGen";

    protected override bool FilterTag(string tag)
    {
        return Config.ResolveVariables(Config.ApiGeneration!, tag) == ApiGeneration.Server;
    }

    protected override string GetFilePath(ModelFile file, string tag)
    {
        return Path.Combine(Config.GetApiPath(file, tag, withControllers: true), $"{file.Options.Endpoints.FileName.ToPascalCase()}Controller.cs");
    }

    protected virtual string GetParam(IProperty param)
    {
        var sb = new StringBuilder();

        string defaultValue = Config.GetValue(param, Classes);

        var type = Config.GetType(param, nonNullable: param is CompositionProperty || (param.Domain?.BodyParam ?? false) || param.IsRouteParam() || defaultValue != "null");

        if (param.Endpoint.IsMultipart && !param.IsQueryParam() && !param.IsRouteParam() && !type.StartsWith("IFormFile"))
        {
            sb.Append("[FromForm] ");
        }
        else if (param.IsJsonBodyParam())
        {
            sb.Append("[FromBody] ");
        }
        else if (type.EndsWith("[]"))
        {
            sb.Append("[FromQuery] ");
        }

        sb.Append($@"{type} {param.GetParamName().Verbatim()}");

        if (param is not CompositionProperty && !param.IsRouteParam() && !(param.Domain?.BodyParam ?? false))
        {
            sb.Append($" = {defaultValue}");
        }

        return sb.ToString();
    }

    protected virtual string GetRoute(Endpoint endpoint)
    {
        var split = endpoint.FullRoute.Split("/");

        for (var i = 0; i < split.Length; i++)
        {
            if (split[i].StartsWith('{'))
            {
                var routeParamName = split[i][1..^1];
                var param = endpoint.Params.Single(param => param.GetParamName() == routeParamName);

                var paramType = Config.GetType(param) switch
                {
                    "int" => "int",
                    "int?" => "int",
                    "Guid" => "guid",
                    "Guid?" => "guid",
                    _ => null
                };
                if (paramType != null)
                {
                    split[i] = $"{{{routeParamName}:{paramType}}}";
                }
            }
        }

        return string.Join("/", split);
    }

    protected override void HandleFile(string filePath, string fileName, string tag, IList<Endpoint> endpoints)
    {
        var className = $"{fileName.ToPascalCase()}Controller";
        var ns = Config.GetNamespace(endpoints.First(), tag);

        var text = File.Exists(filePath)
            ? File.ReadAllText(filePath)
            : $@"using Microsoft.AspNetCore.Mvc;

namespace {ns};

public class {className} : Controller
{{

}}";

        var syntaxTree = CSharpSyntaxTree.ParseText(text);
        var existingController = syntaxTree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().First();

        var controller = existingController;

        var indent = "    ";

        foreach (var endpoint in endpoints)
        {
            var wd = new StringBuilder();

            wd.AppendLine();

            wd.AppendLine($"{indent}/// <summary>");
            wd.AppendLine($"{indent}/// {endpoint.Description}");
            wd.AppendLine($"{indent}/// </summary>");

            foreach (var param in endpoint.Params)
            {
                wd.AppendLine($@"{indent}/// <param name=""{param.GetParamName()}"">{param.Comment}</param>");
            }

            if (!Config.NoAsyncControllers || endpoint.Returns != null)
            {
                wd.AppendLine($"{indent}/// <returns>{(endpoint.Returns != null ? endpoint.Returns.Comment : "Task.")}</returns>");
            }

            if (endpoint.Returns is { Domain.MediaType: string mediaType })
            {
                wd.AppendLine($@"{indent}[Produces(""{mediaType}"")]");
            }

            foreach (var annotation in Config.GetDecoratorAnnotations(endpoint, tag))
            {
                wd.AppendLine($"{indent}[{annotation}]");
            }

            wd.AppendLine($@"{indent}[Http{endpoint.Method.ToPascalCase(true)}(""{GetRoute(endpoint)}"")]");
            wd.AppendLine($"{indent}public {Config.GetReturnTypeName(endpoint.Returns)} {endpoint.NamePascal}({string.Join(", ", endpoint.Params.Select(GetParam))})");
            wd.AppendLine($"{indent}{{");
            wd.AppendLine();
            wd.AppendLine($"{indent}}}");

            var method = (MethodDeclarationSyntax)ParseMemberDeclaration(wd.ToString())!;

            var existingMethod = controller.DescendantNodes().OfType<MethodDeclarationSyntax>().SingleOrDefault(method => method.Identifier.Text == endpoint.NamePascal);
            if (existingMethod != null)
            {
                method = method.WithBody(existingMethod.Body);
                controller = controller.ReplaceNode(existingMethod, method);
            }
            else
            {
                var index = endpoints.IndexOf(endpoint);
                var firstMethod = controller.Members.OfType<MethodDeclarationSyntax>().FirstOrDefault();
                var start = firstMethod != null ? controller.Members.IndexOf(firstMethod) : 0;
                controller = controller.WithMembers(List(controller.Members.Take(start + index).Concat([method]).Concat(controller.Members.Skip(start + index))));
            }
        }

        foreach (var method in controller.DescendantNodes().OfType<MethodDeclarationSyntax>())
        {
            if (method.Modifiers.Any(modifier => modifier.IsKind(SyntaxKind.PublicKeyword)) && !endpoints.Any(endpoint => endpoint.NamePascal == method.Identifier.Text))
            {
                controller = controller.WithMembers(List(controller.Members.Where(member => ((member as MethodDeclarationSyntax)?.Identifier.Text ?? string.Empty) != method.Identifier.Text)));
            }
        }

        if (controller.OpenBraceToken.HasTrailingTrivia && !controller.Members.OfType<FieldDeclarationSyntax>().Any())
        {
            controller = controller.WithOpenBraceToken(controller.OpenBraceToken.WithoutTrivia());
        }

        using var fw = OpenFileWriter(filePath);
        fw.HeaderMessage = "ATTENTION, CE FICHIER EST PARTIELLEMENT GENERE AUTOMATIQUEMENT !";
        fw.Write(syntaxTree.GetRoot().ReplaceNode(existingController, controller).ToString());
    }
}