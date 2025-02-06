using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using TopModel.Core;

namespace TopModel.LanguageServer;

public class DocumentLinkHandler(ModelStore modelStore, ILanguageServerFacade facade, ModelConfig config, ModelFileCache modelFileCache) : DocumentLinkHandlerBase
{
    private readonly ModelConfig _config = config;
    private readonly ILanguageServerFacade _facade = facade;
    private readonly ModelFileCache _fileCache = modelFileCache;
    private readonly ModelStore _modelStore = modelStore;

    public override Task<DocumentLink> Handle(DocumentLink request, CancellationToken cancellationToken)
    {
        return Task.FromResult(request);
    }

    public override async Task<DocumentLinkContainer?> Handle(DocumentLinkParams request, CancellationToken cancellationToken)
    {
        var lockFile = new FileInfo(Path.GetFullPath(Path.Combine(_config.ConfigRoot, _config.LockFileName)));
        if (lockFile.Exists)
        {
            using var file = lockFile.OpenText();
            var text = (await file.ReadToEndAsync()).Split('\n').ToList();
            var indexOfGeneratedFiles = text.IndexOf(text.First(l => l.StartsWith("generatedFiles:")));
            var end = text.Count();
            List<DocumentLink> documentLinks = [];
            var lockFileDir = Path.GetFullPath(Path.GetDirectoryName(request.TextDocument.Uri.Path)!.Trim(Path.DirectorySeparatorChar));

            for (var i = indexOfGeneratedFiles + 1; i < end - 1; i++)
            {
                var target = new Uri(Path.GetFullPath(Path.Combine(lockFileDir!, text[i]["  - ".Length..].Replace('/', Path.DirectorySeparatorChar))));
                documentLinks.Add(new DocumentLink()
                {
                    Range = new OmniSharp.Extensions.LanguageServer.Protocol.Models.Range(new Position(i, 4), new Position(i, text[i].Length)),
                    Target = target
                });
            }

            return new DocumentLinkContainer(documentLinks);
        }

        return new DocumentLinkContainer();
    }

    protected override DocumentLinkRegistrationOptions CreateRegistrationOptions(DocumentLinkCapability capability, ClientCapabilities clientCapabilities)
    {
        return new DocumentLinkRegistrationOptions
        {
            DocumentSelector = TextDocumentSelector.ForPattern($"**/{_config.LockFileName ?? "topmodel.lock"}")
        };
    }
}