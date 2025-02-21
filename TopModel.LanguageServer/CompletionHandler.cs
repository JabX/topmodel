using NJsonSchema;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using TopModel.Core;
using TopModel.Core.FileModel;

namespace TopModel.LanguageServer;

public class CompletionHandler : CompletionHandlerBase
{
    private static readonly List<char> Separators = new()
        {
            ':',
            ',',
            '{',
            '}',
            '[',
            ']',
            ' ',
            '(',
            ')',
            '-',
            '=',
            '\'',
            '#',
            '\n',
            '.'
        };

    private readonly ModelConfig _config;
    private readonly ILanguageServerFacade _facade;
    private readonly ModelFileCache _fileCache;
    private readonly ModelStore _modelStore;

    public CompletionHandler(ModelStore modelStore, ILanguageServerFacade facade, ModelFileCache fileCache, ModelConfig config)
    {
        _config = config;
        _facade = facade;
        _fileCache = fileCache;
        _modelStore = modelStore;
    }

    public override Task<CompletionItem> Handle(CompletionItem request, CancellationToken cancellationToken)
    {
        return Task.FromResult(request);
    }

    public override Task<CompletionList> Handle(CompletionParams request, CancellationToken cancellationToken)
    {
        var text = _fileCache.GetFile(request.TextDocument.Uri.GetFileSystemPath());
        var currentLine = text.ElementAtOrDefault(request.Position.Line);

        if (currentLine == null)
        {
            return Task.FromResult(new CompletionList());
        }

        var file = _modelStore.Files.SingleOrDefault(f => _facade.GetFilePath(f) == request.TextDocument.Uri.GetFileSystemPath());
        if (file == null || currentLine == string.Empty)
        {
            return Task.FromResult(new CompletionList());
        }

        var reqChar = Math.Min(request.Position.Character, currentLine.Length);
        var rootObject = GetRootObject(request).Object;
        if (rootObject.StartsWith("--"))
        {
            return Task.FromResult(new CompletionList());
        }

        var currentKey = GetCurrentKey(request).Key;
        var parentKey = GetParentKey(request).Key;
        var useIndex = GetUseIndex(file, text);
        if (parentKey == "asDomains" && currentLine[..reqChar].Contains(':')
            || currentKey == "domain"
            || parentKey == "domain" && currentLine[..reqChar].Contains("name: ")
            || rootObject == "converter"
                && (currentKey == "to" || currentKey == "from"))
        {
            return Task.FromResult(CompleteDomain(request));
        }

        List<string> classCompleteKeys = new()
        {
            "association",
            "composition",
            "class",
            "extends"
        };
        if (classCompleteKeys.Contains(currentKey))
        {
            return Task.FromResult(CompleteClass(request, file, useIndex));
        }

        // Tags
        if (currentKey == "tags")
        {
            return Task.FromResult(CompleteTag(request, file));
        }

        // Use
        if (currentKey == "uses")
        {
            return Task.FromResult(CompleteFile(request, file));
        }

        // Décorateur
        else if (currentKey == "decorators")
        {
            return Task.FromResult(CompleteDecorator(request, file, useIndex));
        }

        // DataFlow
        else if (currentKey == "dependsOn:")
        {
            return Task.FromResult(CompleteDataFlow(request, file, useIndex));
        }
        else
        {
            return Task.FromResult(CompleteProperty(request, text, currentLine, file));
        }
    }

    protected override CompletionRegistrationOptions CreateRegistrationOptions(CompletionCapability capability, ClientCapabilities clientCapabilities)
    {
        return new CompletionRegistrationOptions
        {
            DocumentSelector = _config.GetDocumentSelector()
        };
    }

    private CompletionList CompleteClass(CompletionParams request, ModelFile file, int useIndex)
    {
        var searchText = GetSearchText(request);
        var availableClasses = new HashSet<Class>(_modelStore.GetAvailableClasses(file));

        return new CompletionList(
            _modelStore.Classes
                .Where(classe => classe.Name.ToLower().ShouldMatch(searchText))
                .Select(classe => new CompletionItem
                {
                    Kind = CompletionItemKind.Class,
                    Label = availableClasses.Contains(classe) ? classe.Name : $"{classe.Name} - ({classe.ModelFile.Name})",
                    LabelDetails = new()
                    {
                        Description = $"{classe.Comment}"
                    },
                    InsertText = classe.Name,
                    SortText = availableClasses.Contains(classe) ? "0000" + classe.Name : classe.Name,
                    TextEdit = new TextEditOrInsertReplaceEdit(new TextEdit
                    {
                        NewText = classe.Name,
                        Range = GetCompleteRange(searchText, request)
                    }),
                    AdditionalTextEdits = !availableClasses.Contains(classe) ?
                        new TextEditContainer(new TextEdit
                        {
                            NewText = file.Uses.Any() ? $"  - {classe.ModelFile.Name}{Environment.NewLine}" : $"uses:{Environment.NewLine}  - {classe.ModelFile.Name}{Environment.NewLine}",
                            Range = new OmniSharp.Extensions.LanguageServer.Protocol.Models.Range(useIndex, 0, useIndex, 0)
                        })
                        : null
                }));
    }

    private CompletionList CompleteDataFlow(CompletionParams request, ModelFile file, int useIndex)
    {
        var searchText = GetSearchText(request);
        var availableDataFlows = new HashSet<DataFlow>(_modelStore.GetAvailableDataFlows(file));

        return new CompletionList(
            _modelStore.DataFlows
                .Where(dataFlow => dataFlow.Name.ToLower().ShouldMatch(searchText))
                .OrderBy(dataFlow => dataFlow.Name)
                .Select(dataFlow => new CompletionItem
                {
                    Kind = CompletionItemKind.Class,
                    Label = availableDataFlows.Contains(dataFlow) ? dataFlow.Name : $"{dataFlow.Name} - ({dataFlow.ModelFile.Name})",
                    InsertText = dataFlow.Name,
                    SortText = availableDataFlows.Contains(dataFlow) ? "0000" + dataFlow.Name : dataFlow.Name,
                    TextEdit = new TextEditOrInsertReplaceEdit(new TextEdit
                    {
                        NewText = dataFlow.Name,
                        Range = GetCompleteRange(searchText, request)
                    }),
                    AdditionalTextEdits = !availableDataFlows.Contains(dataFlow) ?
                        new TextEditContainer(new TextEdit
                        {
                            NewText = file.Uses.Any() ? $"  - {dataFlow.ModelFile.Name}{Environment.NewLine}" : $"uses:{Environment.NewLine}  - {dataFlow.ModelFile.Name}{Environment.NewLine}",
                            Range = new OmniSharp.Extensions.LanguageServer.Protocol.Models.Range(useIndex, 0, useIndex, 0)
                        })
                        : null
                }));
    }

    private CompletionList CompleteDecorator(CompletionParams request, ModelFile file, int useIndex)
    {
        var searchText = GetSearchText(request);
        var availableDecorators = new HashSet<Decorator>(_modelStore.GetAvailableDecorators(file));

        return new CompletionList(
            _modelStore.Decorators
                .Where(decorator => decorator.Name.ToLower().ShouldMatch(searchText))
                .OrderBy(decorator => decorator.Name)
                .Select(decorator => new CompletionItem
                {
                    Kind = CompletionItemKind.Class,
                    Label = availableDecorators.Contains(decorator) ? decorator.Name : $"{decorator.Name} - ({decorator.ModelFile.Name})",
                    LabelDetails = new()
                    {
                        Description = $"{decorator.Description}"
                    },
                    InsertText = decorator.Name,
                    SortText = availableDecorators.Contains(decorator) ? "0000" + decorator.Name : decorator.Name,
                    TextEdit = new TextEditOrInsertReplaceEdit(new TextEdit
                    {
                        NewText = decorator.Name,
                        Range = GetCompleteRange(searchText, request)
                    }),
                    AdditionalTextEdits = !availableDecorators.Contains(decorator) ?
                        new TextEditContainer(new TextEdit
                        {
                            NewText = file.Uses.Any() ? $"  - {decorator.ModelFile.Name}{Environment.NewLine}" : $"uses:{Environment.NewLine}  - {decorator.ModelFile.Name}{Environment.NewLine}",
                            Range = new OmniSharp.Extensions.LanguageServer.Protocol.Models.Range(useIndex, 0, useIndex, 0)
                        })
                        : null
                }));
    }

    private CompletionList CompleteDomain(CompletionParams request)
    {
        var searchText = GetSearchText(request);
        return new CompletionList(
            _modelStore.Domains
                .Where(domain => domain.Key.ToLower().ShouldMatch(searchText))
                .OrderBy(domain => domain.Key)
                .Select(domain => new CompletionItem
                {
                    Kind = CompletionItemKind.EnumMember,
                    Label = domain.Key,
                    TextEdit = new TextEditOrInsertReplaceEdit(new TextEdit
                    {
                        NewText = domain.Key,
                        Range = GetCompleteRange(searchText, request)
                    }),
                }));
    }

    private CompletionList CompleteFile(CompletionParams request, ModelFile file)
    {
        var searchText = GetSearchText(request);
        return new CompletionList(
            _modelStore.Files.Select(f => f.Name)
                .Except(file.Uses.Select(u => u.ReferenceName))
                .Where(name => name != file.Name && name.ToLower().ShouldMatch(searchText))
                .Select(name => new CompletionItem
                {
                    Kind = CompletionItemKind.File,
                    Label = name,
                    TextEdit = new TextEditOrInsertReplaceEdit(new TextEdit
                    {
                        NewText = name,
                        Range = GetCompleteRange(searchText, request)
                    })
                }));
    }

    private CompletionList CompleteProperty(CompletionParams request, string[] text, string currentLine, ModelFile file)
    {
        // Alias, propriété d'association ou propriété de flux de données
        string? className = null;
        var requestLine = request.Position.Line;
        var isListElement = currentLine.TrimStart().StartsWith('-');
        var isInlineList = currentLine.Contains(':') && currentLine.Split(':')[1].TrimStart().StartsWith('[');
        var objectRange = isListElement ? GetParentRange(text, requestLine) : GetObjectRange(text, requestLine);
        var objectLines = text[objectRange.Start..objectRange.End];
        var searchText = GetSearchText(request);
        var cl = objectLines.ToList().Find(o => o.Contains("class: ") || o.Contains("association: "));
        if (cl != null)
        {
            className = cl.Split(": ")[1].Trim();
        }

        var currentKey = GetCurrentKey(request);
        var propertyListKeyWords = new List<string>()
        {
            "include", "exclude", "property", "joinProperties"
        };
        var propertyKeyWords = new List<string>()
        {
            "property", "activeProperty", "exclude"
        };
        if (className != null && ((isListElement || isInlineList) && propertyListKeyWords.Contains(currentKey.Key)
                                || propertyKeyWords.Contains(currentKey.Key)))
        {
            var referencedClasses = _modelStore.GetReferencedClasses(file);
            if (referencedClasses.TryGetValue(className, out var aliasedClass))
            {
                return CompleteProperty(request, aliasedClass, false);
            }
        }

        var rootObject = GetRootObject(request);
        if (rootObject.Object == "class")
        {
            var classRange = GetObjectRange(text, rootObject.Line);
            var classLines = text[classRange.Start..classRange.End];
            var nameLine = classLines.OrderBy(GetIndentLevel).First(l => l.Trim().StartsWith("name: "));
            className = nameLine.Split(':')[1].Trim();
            var classe = file.Classes.Find(c => c.Name == className);
            if (classe != null)
            {
                var includeExtends = false;

                var selfClassropertyKeyWords = new List<string>()
                {
                    "defaultProperty", "flagProperty", "orderProperty", "target", "unique"
                };

                var isValues = false;
                var isMappings = false;
                if (!selfClassropertyKeyWords.Contains(currentKey.Key))
                {
                    var parentKey = currentKey;
                    while (parentKey.Key != "class" && parentKey.Key != "mappings" && parentKey.Key != "values" && parentKey.Line > 0)
                    {
                        parentKey = GetParentKey(text, parentKey.Line, parentKey.End);
                    }

                    isValues = parentKey.Key == "values";
                    isMappings = parentKey.Key == "mappings";
                    includeExtends = isMappings || isValues;

                    if (isMappings)
                    {
                        var requestLineText = text[request.Position.Line];
                        var textBefore = requestLineText[..request.Position.Character];
                        var isKey = textBefore.LastIndexOf(':') == -1 || textBefore.LastIndexOf(':') < Math.Max(textBefore.LastIndexOf(','), textBefore.LastIndexOf('{'));
                        if (!isKey)
                        {
                            var parentObjectRange = GetObjectRange(text, parentKey.Line);
                            var parentObjectLines = text[parentObjectRange.Start..(parentObjectRange.End + 1)];
                            className = parentObjectLines.First(l => l.Contains("class: ")).TrimStart().Split(':')[1].Trim();
                        }

                        var referencedClasses = _modelStore.GetReferencedClasses(file);
                        if (referencedClasses.TryGetValue(className, out var aliasedClass))
                        {
                            classe = aliasedClass;
                        }
                    }
                    else if (isValues)
                    {
                        var requestLineText = text[request.Position.Line];
                        var textBefore = requestLineText[..request.Position.Character];
                        var isKey = textBefore.LastIndexOf(':') == -1 || textBefore.LastIndexOf(':') < Math.Max(textBefore.LastIndexOf(','), textBefore.LastIndexOf('{'));
                        if (!isKey)
                        {
                            return new CompletionList();
                        }
                    }
                }

                if (searchText != null && (isValues || isMappings || selfClassropertyKeyWords.Contains(currentKey.Key)))
                {
                    return CompleteProperty(request, classe, includeExtends);
                }
            }
        }

        return new CompletionList();
    }

    private CompletionList CompleteProperty(CompletionParams request, Class classe, bool includeExtends)
    {
        var properties = includeExtends ? classe.ExtendedProperties : classe.Properties;
        var searchText = GetSearchText(request);
        return new CompletionList(properties
            .Where(f => f.Name.ShouldMatch(searchText))
            .Select(f => new CompletionItem
            {
                Kind = CompletionItemKind.Property,
                Label = f.Name,
                LabelDetails = new()
                {
                    Description = $"{f.Comment}"
                },
                TextEdit = new TextEditOrInsertReplaceEdit(new TextEdit
                {
                    NewText = f.Name,
                    Range = GetCompleteRange(searchText, request)
                })
            }));
    }

    private CompletionList CompleteTag(CompletionParams request, ModelFile file)
    {
        var searchText = GetSearchText(request);
        return new CompletionList(
            _modelStore.Files.SelectMany(f => f.Tags)
            .Distinct()
            .Where(t => !file.Tags.Contains(t))
            .Where(tag => tag.ShouldMatch(searchText))
            .Select(tag => new CompletionItem
            {
                Kind = CompletionItemKind.Keyword,
                Label = tag,
                TextEdit = new TextEditOrInsertReplaceEdit(new TextEdit
                {
                    NewText = tag,
                    Range = GetCompleteRange(searchText, request)
                })
            }));
    }

    private OmniSharp.Extensions.LanguageServer.Protocol.Models.Range GetCompleteRange(string searchText, CompletionParams request)
    {
        var text = _fileCache.GetFile(request.TextDocument.Uri.GetFileSystemPath());
        var currentLine = text.ElementAtOrDefault(request.Position.Line)!;
        int start, end = currentLine.Length;
        if (currentLine.Length > 0 && Separators.Exists(currentLine.Contains))
        {
            var left = currentLine[..request.Position.Character];
            start = left.LastIndexOfAny(Separators.ToArray()) + 1;
            var right = currentLine[request.Position.Character..];
            if (right.IndexOfAny(Separators.ToArray()) >= 0)
            {
                end = request.Position.Character + right.IndexOfAny(Separators.ToArray());
            }
        }
        else
        {
            start = currentLine.IndexOf(searchText);
        }

        return new OmniSharp.Extensions.LanguageServer.Protocol.Models.Range(
                            request.Position.Line,
                            start,
                            request.Position.Line,
                            end);
    }

    private (string Key, int Line, int End, bool IsKey) GetCurrentKey(CompletionParams request)
    {
        var text = _fileCache.GetFile(request.TextDocument.Uri.GetFileSystemPath());
        return GetCurrentKey(text, request.Position.Line, request.Position.Character);
    }

    private (string Key, int Line, int End, bool IsKey) GetCurrentKey(string[] text, int line, int position)
    {
        var currentLine = text.ElementAtOrDefault(line);
        var rootLine = currentLine ?? string.Empty;
        if (rootLine.Trim().StartsWith('-'))
        {
            rootLine = rootLine.Replace('-', ' ');
        }

        var isInLineObject = rootLine[..position].Contains(": {");
        if (isInLineObject)
        {
            var isKey = rootLine[..position].LastIndexOf(':') < Math.Max(rootLine[..position].LastIndexOf(','), rootLine[..position].LastIndexOf('{'));
            if (isKey)
            {
                return (Key: rootLine.TrimStart().Split(':')[0], Line: line, End: rootLine.Split(':')[0].Length - 1, IsKey: true);
            }
            else
            {
                return (Key: rootLine[rootLine[..position].LastIndexOf(':')..position].Split(':')[0], Line: line, End: rootLine[..position].LastIndexOf(':') - 1, IsKey: false);
            }
        }

        if (rootLine[..position].Contains(": "))
        {
            return (Key: rootLine.TrimStart().Split(':')[0], Line: line, End: rootLine.Split(':')[0].Length - 1, IsKey: false);
        }

        var requestLine = line;
        var currentIndent = GetIndentLevel(text, line);
        var rootIndent = currentIndent;

        while (rootIndent >= currentIndent && requestLine > 0 && !rootLine.StartsWith("---"))
        {
            requestLine--;

            rootLine = text.ElementAtOrDefault(requestLine) ?? string.Empty;
            while ((string.IsNullOrWhiteSpace(rootLine) || rootLine.Contains('#') && string.IsNullOrWhiteSpace(rootLine[..rootLine.IndexOf('#')].Trim())) && requestLine > 0)
            {
                requestLine--;
                rootLine = text.ElementAtOrDefault(requestLine) ?? string.Empty;
            }

            rootIndent = GetIndentLevel(text, requestLine);
            if (rootLine.Trim().StartsWith('-'))
            {
                rootLine = rootLine.Replace('-', ' ');
            }
        }

        return (Key: rootLine.Split(":")[0].Trim(), Line: requestLine, End: rootLine.Split(":")[0].Length - 1, IsKey: false);
    }

    private int GetIndentLevel(string[] text, int lineNumber)
    {
        var line = text[lineNumber];
        return GetIndentLevel(line);
    }

    private int GetIndentLevel(string line)
    {
        if (line.StartsWith("---"))
        {
            return 0;
        }

        var isList = line.TrimStart().StartsWith('-');
        if (isList)
        {
            line = line.Replace('-', ' ');
        }

        return line.Length - line.TrimStart().Length;
    }

    private (int Start, int End) GetObjectRange(string[] text, int lineNumber)
    {
        var start = lineNumber;
        var end = lineNumber;
        var indentLevelLine = GetIndentLevel(text, lineNumber);
        while (start > 0 && !text[start].StartsWith("---") && GetIndentLevel(text, start) >= indentLevelLine)
        {
            start--;
            if (GetIndentLevel(text, start) == indentLevelLine && text[start].TrimStart().StartsWith('-'))
            {
                start--;
                break;
            }
        }

        while (end < text.Length && !text[end].StartsWith("---") && GetIndentLevel(text, end) >= indentLevelLine && !(GetIndentLevel(text, end) == indentLevelLine && text[end].TrimStart().StartsWith('-')))
        {
            end++;
        }

        return (Math.Min(start + 1, lineNumber), Math.Max(end - 1, lineNumber));
    }

    private (string Key, int Line, int End, bool IsKey) GetParentKey(CompletionParams request)
    {
        var text = _fileCache.GetFile(request.TextDocument.Uri.GetFileSystemPath());
        return GetParentKey(text, request.Position.Line, request.Position.Character);
    }

    private (string Key, int Line, int End, bool IsKey) GetParentKey(string[] text, int line, int position)
    {
        var currentLine = text.ElementAtOrDefault(line);
        var rootLine = currentLine ?? string.Empty;
        var isInLineObject = position < rootLine.Length && rootLine[..Math.Min(position, rootLine.Length - 1)].Contains(": {");
        if (isInLineObject)
        {
            return (Key: rootLine.TrimStart().Split(':')[0], Line: line, End: rootLine[..position].LastIndexOf(':') - 1, IsKey: false);
        }

        var requestLine = line;
        var currentIndent = GetIndentLevel(text, line);
        var rootIndent = currentIndent;

        while (rootIndent >= currentIndent && requestLine > 0 && !rootLine.StartsWith("---"))
        {
            requestLine--;
            rootLine = text.ElementAtOrDefault(requestLine) ?? string.Empty;
            while ((string.IsNullOrWhiteSpace(rootLine) || rootLine.Contains('#') && string.IsNullOrWhiteSpace(rootLine[..rootLine.IndexOf('#')].Trim())) && requestLine > 0)
            {
                requestLine--;
                rootLine = text.ElementAtOrDefault(requestLine) ?? string.Empty;
            }

            rootLine = text.ElementAtOrDefault(requestLine) ?? string.Empty;
            rootIndent = GetIndentLevel(text, requestLine);
        }

        return (Key: rootLine.Split(":")[0].Trim(), Line: requestLine, End: rootLine.Split(":")[0].Length - 1, IsKey: false);
    }

    private (int Start, int End) GetParentRange(string[] text, int lineNumber)
    {
        var start = lineNumber;
        var indentLevelLine = GetIndentLevel(text, lineNumber);
        while (GetIndentLevel(text, start) >= indentLevelLine)
        {
            start--;
        }

        return GetObjectRange(text, start);
    }

    private (string Object, int Line) GetRootObject(CompletionParams request)
    {
        var text = _fileCache.GetFile(request.TextDocument.Uri.GetFileSystemPath());
        var currentLine = text.ElementAtOrDefault(request.Position.Line);
        var requestLine = request.Position.Line;
        var rootLine = currentLine ?? string.Empty;
        while (
            !(rootLine.StartsWith("class")
            || rootLine.StartsWith("domain")
            || rootLine.StartsWith("decorator")
            || rootLine.StartsWith("converter")
            || rootLine.StartsWith("endpoint")
            || rootLine.StartsWith("dataFlow")))
        {
            requestLine--;
            if (requestLine < 0 || rootLine.StartsWith("---"))
            {
                break;
            }

            rootLine = text.ElementAtOrDefault(requestLine) ?? string.Empty;
        }

        return (Object: rootLine.Split(":")[0], Line: requestLine);
    }

    private string GetSearchText(CompletionParams request)
    {
        var text = _fileCache.GetFile(request.TextDocument.Uri.GetFileSystemPath());
        var currentLine = text.ElementAtOrDefault(request.Position.Line)!;
        int start = 0, end = request.Position.Character;
        var left = currentLine[..end];
        if (currentLine.Length > 0 && Separators.Exists(currentLine.Contains))
        {
            start = Math.Min(left.LastIndexOfAny(Separators.ToArray()) + 1, end);
        }

        return currentLine[start..end].Trim();
    }

    private int GetUseIndex(ModelFile file, string[] text)
    {
        if (file.Uses.Any())
        {
            return file.Uses.Last().ToRange()!.Start.Line + 1;
        }
        else if (text.First().StartsWith('-'))
        {
            return 1;
        }

        return 0;
    }
}