using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using TopModel.Core;
using TopModel.Core.FileModel;
using TopModel.Utils;
using Range = OmniSharp.Extensions.LanguageServer.Protocol.Models.Range;

namespace TopModel.LanguageServer;

public class CodeActionHandler : CodeActionHandlerBase
{
    private readonly ModelConfig _config;
    private readonly ILanguageServerFacade _facade;
    private readonly ModelFileCache _fileCache;
    private readonly ModelStore _modelStore;

    public CodeActionHandler(ModelStore modelStore, ILanguageServerFacade facade, ModelFileCache modelFileCache, ModelConfig config)
    {
        _config = config;
        _facade = facade;
        _fileCache = modelFileCache;
        _modelStore = modelStore;
    }

    public override Task<CodeAction> Handle(CodeAction request, CancellationToken cancellationToken)
    {
        return Task.FromResult(request);
    }

    public override Task<CommandOrCodeActionContainer?> Handle(CodeActionParams request, CancellationToken cancellationToken)
    {
        var modelFile = _modelStore.Files.SingleOrDefault(f => _facade.GetFilePath(f) == request.TextDocument.Uri.GetFileSystemPath());
        var codeActions = new List<CommandOrCodeAction>();
        if (modelFile != null)
        {
            if (modelFile.Uses.Except(modelFile.UselessImports).Any())
            {
                codeActions.Add(GetCodeActionOrganizeImports(request, modelFile));
            }

            foreach (var diagnostic in request.Context.Diagnostics.Where(d => !string.IsNullOrEmpty(d.Code)))
            {
                var modelErrorType = Enum.Parse<ModelErrorType>(diagnostic.Code!);
                switch (modelErrorType)
                {
                    case ModelErrorType.TMD1005:
                        codeActions.AddRange(GetCodeActionCreateDomain(request, diagnostic));
                        break;
                    case ModelErrorType.TMD1002:
                        codeActions.AddRange(GetCodeActionMissingClassImport(request, diagnostic, modelFile));
                        codeActions.AddRange(GetCodeActionAddClass(request, diagnostic, modelFile));
                        break;
                    case ModelErrorType.TMD1008:
                        codeActions.AddRange(GetCodeActionMissingDecoratorImport(request, diagnostic, modelFile));
                        break;
                    case ModelErrorType.TMD2000:
                        codeActions.AddRange(GetCodeActionMissingDataFlowImport(request, diagnostic, modelFile));
                        break;
                    default:
                        break;
                }
            }
        }

        return Task.FromResult<CommandOrCodeActionContainer?>(CommandOrCodeActionContainer.From(codeActions));
    }

    protected static CodeAction GetCodeActionOrganizeImports(CodeActionParams request, ModelFile modelFile)
    {
        var start = modelFile.Uses.First().ToRange()!.Start;
        var end = modelFile.Uses.Last().ToRange()!.End;
        var uselessImports = modelFile.UselessImports;
        return new CodeAction()
        {
            Title = "Trier les Uses",
            Kind = CodeActionKind.SourceOrganizeImports,
            IsPreferred = true,
            Edit = new WorkspaceEdit
            {
                Changes =
                    new Dictionary<DocumentUri, IEnumerable<TextEdit>>
                    {
                        [request.TextDocument.Uri] = new List<TextEdit>
                        {
                            new TextEdit
                            {
                                NewText = string.Join(
                                    "\n  - ",
                                    modelFile.Uses
                                        .Except(uselessImports)
                                        .DistinctBy(u => u.ReferenceName)
                                        .OrderBy(u => u.ReferenceName)
                                        .Select(u => u.ReferenceName)),
                                Range = new Range(start, end)
                            }
                        }
                    }
            }
        };
    }

    protected override CodeActionRegistrationOptions CreateRegistrationOptions(CodeActionCapability capability, ClientCapabilities clientCapabilities)
    {
        return new()
        {
            DocumentSelector = _config.GetDocumentSelector(),
            ResolveProvider = true,
            CodeActionKinds = new List<CodeActionKind>
            {
                CodeActionKind.SourceOrganizeImports,
                CodeActionKind.QuickFix
            }
        };
    }

    protected IEnumerable<CommandOrCodeAction> GetCodeActionAddClass(CodeActionParams request, Diagnostic diagnostic, ModelFile modelFile)
    {
        var text = _fileCache.GetFile(request.TextDocument.Uri.GetFileSystemPath());
        var line = text.ElementAt(diagnostic.Range.Start.Line);
        var className = line[diagnostic.Range.Start.Character..Math.Min(diagnostic.Range.End.Character, line.Length)];
        return new List<CommandOrCodeAction>
        {
            new CodeAction
            {
                Title = $"TopModel : Créer la classe {className} dans ce fichier",
                Kind = CodeActionKind.QuickFix,
                IsPreferred = true,
                Diagnostics = new List<Diagnostic>
                {
                    diagnostic
                },
                Edit = new WorkspaceEdit
                {
                    Changes =
                        new Dictionary<DocumentUri, IEnumerable<TextEdit>>
                        {
                            [new Uri(_facade.GetFilePath(modelFile))] = new List<TextEdit>()
                            {
                                new TextEdit()
                                {
                                    NewText = @$"
---
class: 
  name: {className}
  comment: 
  properties:
    - 
",
                                    Range = new Range(text.Length, 0, text.Length, 0)
                                }
                            }
                        }
                }
            }
        };
    }

    protected IEnumerable<CommandOrCodeAction> GetCodeActionCreateDomain(CodeActionParams request, Diagnostic diagnostic)
    {
        var fs = request.TextDocument.Uri.GetFileSystemPath();
        var text = _fileCache.GetFile(request.TextDocument.Uri.GetFileSystemPath());
        var line = text.ElementAt(diagnostic.Range.Start.Line);
        var domainName = line[diagnostic.Range.Start.Character..Math.Min(diagnostic.Range.End.Character, line.Length)];

        return _modelStore.Files.Where(f => f.Domains.Any()).Select(f =>
        {
            var lastLine = File.ReadAllLines(_facade.GetFilePath(f)).Length;
            return (CommandOrCodeAction)new CodeAction
            {
                Title = $"TopModel : Ajouter le domain au fichier {f.Path}",
                Kind = CodeActionKind.QuickFix,
                IsPreferred = true,
                Diagnostics = new List<Diagnostic>
                {
                    diagnostic
                },
                Edit = new WorkspaceEdit
                {
                    Changes =
                        new Dictionary<DocumentUri, IEnumerable<TextEdit>>
                        {
                            [new Uri(_facade.GetFilePath(f))] = new List<TextEdit>()
                            {
                                new TextEdit()
                                {
                                    Range = new Range(new Position(lastLine, 0), new Position(lastLine, 0)),
                                    NewText = $@"
---
domain:
  name: {domainName}
  label: 
"
                                }
                            }
                        }
                }
            };
        }).ToList();
    }

    protected IEnumerable<CommandOrCodeAction> GetCodeActionMissingClassImport(CodeActionParams request, Diagnostic diagnostic, ModelFile modelFile)
    {
        var fs = request.TextDocument.Uri.GetFileSystemPath();
        var text = _fileCache.GetFile(request.TextDocument.Uri.GetFileSystemPath());
        var line = text.ElementAt(diagnostic.Range.Start.Line);
        var className = line[diagnostic.Range.Start.Character..Math.Min(diagnostic.Range.End.Character, line.Length)];
        var useIndex = modelFile!.Uses.Any()
            ? modelFile.Uses.Last().ToRange()!.Start.Line + 1
            : text.First().StartsWith("-")
                ? 1
                : 0;

        return _modelStore.Classes.Where(c => c.Name == className)
            .Select(classToImport => GetFileImportAction(diagnostic, modelFile, classToImport.ModelFile, useIndex));
    }

    protected IEnumerable<CommandOrCodeAction> GetCodeActionMissingDataFlowImport(CodeActionParams request, Diagnostic diagnostic, ModelFile modelFile)
    {
        var fs = request.TextDocument.Uri.GetFileSystemPath();
        var text = _fileCache.GetFile(request.TextDocument.Uri.GetFileSystemPath());
        var line = text.ElementAt(diagnostic.Range.Start.Line);
        var dataFlowName = line[diagnostic.Range.Start.Character..Math.Min(diagnostic.Range.End.Character, line.Length)];
        var useIndex = modelFile!.Uses.Any()
            ? modelFile.Uses.Last().ToRange()!.Start.Line + 1
            : text.First().StartsWith("-")
                ? 1
                : 0;

        return _modelStore.DataFlows.Where(c => c.Name == dataFlowName)
            .Select(decoratorToImport => GetFileImportAction(diagnostic, modelFile, decoratorToImport.ModelFile, useIndex));
    }

    protected IEnumerable<CommandOrCodeAction> GetCodeActionMissingDecoratorImport(CodeActionParams request, Diagnostic diagnostic, ModelFile modelFile)
    {
        var fs = request.TextDocument.Uri.GetFileSystemPath();
        var text = _fileCache.GetFile(request.TextDocument.Uri.GetFileSystemPath());
        var line = text.ElementAt(diagnostic.Range.Start.Line);
        var decoratorName = line[diagnostic.Range.Start.Character..Math.Min(diagnostic.Range.End.Character, line.Length)];
        var useIndex = modelFile!.Uses.Any()
            ? modelFile.Uses.Last().ToRange()!.Start.Line + 1
            : text.First().StartsWith("-")
                ? 1
                : 0;

        return _modelStore.Decorators.Where(c => c.Name == decoratorName)
            .Select(decoratorToImport => GetFileImportAction(diagnostic, modelFile, decoratorToImport.ModelFile, useIndex));
    }

    private CommandOrCodeAction GetFileImportAction(Diagnostic diagnostic, ModelFile targetFile, ModelFile sourceFile, int useIndex)
    {
        return (CommandOrCodeAction)new CodeAction
        {
            Title = $"TopModel : Ajouter l'import {sourceFile.Name}",
            Kind = CodeActionKind.QuickFix,
            IsPreferred = true,
            Diagnostics = new List<Diagnostic> { diagnostic },
            Edit = new WorkspaceEdit
            {
                Changes =
                    new Dictionary<DocumentUri, IEnumerable<TextEdit>>
                    {
                        [new Uri(_facade.GetFilePath(targetFile))] = new List<TextEdit>()
                        {
                            new TextEdit()
                            {
                                NewText = targetFile.Uses.Any() ? $"  - {sourceFile.Name}{Environment.NewLine}" : $"uses:{Environment.NewLine}  - {sourceFile.Name}{Environment.NewLine}",
                                Range = new Range(useIndex, 0, useIndex, 0)
                            }
                        }
                    }
            }
        };
    }
}