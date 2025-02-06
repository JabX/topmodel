using TopModel.Utils;

namespace TopModel.ModelGenerator;

/// <summary>
/// FileWriter avec des méthodes spécialisées pour écrire du TopModel.
/// </summary>
public class TmdWriter : IDisposable
{
    private readonly TmdFile _file;
    private readonly string _modelRoot;
    private readonly IFileWriter _writer;

    public TmdWriter(IFileWriter writer, TmdFile file, string modelRoot)
    {
        _file = file;
        _writer = writer;
        _writer.StartCommentToken = "####";
        _modelRoot = Path.GetRelativePath(modelRoot, string.Join("/", writer.FileName.Split('/').SkipLast(1)));
        if (_modelRoot == ".")
        {
            _modelRoot = string.Empty;
        }
        else
        {
            _modelRoot += '/';
        }
    }

    /// <inheritdoc cref="IDisposable.Dispose" />
    public void Dispose()
    {
        Write();
        _writer.Dispose();
    }

    public void Write()
    {
        _writer.WriteLine($"---");
        var module = _file.Module?.Contains('_') ?? false ? _file.Module!.Split('_')[1] : _file.Module;
        _writer.WriteLine($"module: {module}");
        _writer.WriteLine($"tags:");
        foreach (var tag in _file.Tags)
        {
            _writer.WriteLine($"  - {tag}");
        }

        if (_file.Uses.Where(u => u.Name != _file.Name).Any())
        {
            _writer.WriteLine($"uses:");
            foreach (var u in _file.Uses.OrderBy(u => u.Name).Where(u => u.Name != _file.Name && u.Path != null))
            {
                _writer.WriteLine($"  - {u.Path!.Replace('\\', '/')}");
            }
        }

        foreach (var classe in _file.Classes.Distinct().OrderBy(c => c.Name))
        {
            WriteClass(classe);
        }

        foreach (var endpoint in _file.Endpoints.Distinct().OrderBy(c => c.Name))
        {
            WriteEndpoint(endpoint);
        }
    }

    private void WriteClass(TmdClass classe)
    {
        _writer.WriteLine($"---");
        _writer.WriteLine($"class:");
        _writer.WriteLine($"  name: {classe.Name}");
        if (!string.IsNullOrEmpty(classe.Extends))
        {
            _writer.WriteLine($"  extends: {classe.Extends}");
        }

        if (!string.IsNullOrEmpty(classe.SqlName))
        {
            _writer.WriteLine($"  sqlName: {classe.SqlName}");
        }

        _writer.WriteLine($"  comment: {classe.Comment}");
        if (!string.IsNullOrEmpty(classe.Trigram))
        {
            _writer.WriteLine($"  trigram: {classe.Trigram.ToUpper()}");
        }

        if (classe.PreservePropertyCasing)
        {
            _writer.WriteLine($"  preservePropertyCasing: {classe.PreservePropertyCasing.ToString().ToLower()}");
        }

        if (!classe.Properties.Where(p => !(p is TmdCompositionProperty cp && cp.Composition == null)).Any())
        {
            _writer.WriteLine($"  properties: []");
        }
        else
        {
            _writer.WriteLine($"  properties:");
            var isFirst = true;
            foreach (var property in classe.Properties)
            {
                if (!isFirst)
                {
                    _writer.WriteLine();
                }

                isFirst = false;
                WriteProperty(property);
            }
        }

        if (classe.Unique.Count > 0)
        {
            _writer.WriteLine();
            _writer.WriteLine($"  unique:");
            foreach (var uniq in classe.Unique)
            {
                _writer.WriteLine($"    - [{string.Join(", ", uniq)}]");
            }
        }

        if (classe.Values.Count > 0)
        {
            _writer.WriteLine();
            _writer.WriteLine($"  values:");
            var i = 0;
            foreach (var row in classe.Values)
            {
                _writer.WriteLine(@$"    Value{i++}: {{{string.Join(", ", row.Where(v => v.Value != null).Select(v => $@"{v.Key}: ""{v.Value}"""))}}}");
            }
        }
    }

    private void WriteEndpoint(TmdEndpoint endpoint)
    {
        _writer.WriteLine($"---");
        _writer.WriteLine($"endpoint:");
        _writer.WriteLine($"  name: {endpoint.Name}");
        _writer.WriteLine($"  method: {endpoint.Method}");
        _writer.WriteLine($"  route: {endpoint.Route}");
        _writer.WriteLine(@$"  description: {endpoint.Comment}");
        if (endpoint.PreservePropertyCasing)
        {
            _writer.WriteLine($"  preservePropertyCasing: {endpoint.PreservePropertyCasing.ToString().ToLower()}");
        }

        if (endpoint.Params.Count > 0)
        {
            _writer.WriteLine($"  params:");
            var isFirst = true;
            foreach (var property in endpoint.Params)
            {
                if (!isFirst)
                {
                    _writer.WriteLine();
                }

                isFirst = false;
                WriteProperty(property);
            }
        }

        if (endpoint.Returns != null)
        {
            _writer.WriteLine($"  returns:");
            WriteProperty(endpoint.Returns, true);
        }
    }

    private void WriteProperty(TmdProperty property, bool noList = false)
    {
        var listPrefix = noList ? string.Empty : "  ";
        if (property is TmdAssociationProperty ap)
        {
            _writer.WriteLine($"    {(noList ? string.Empty : "- ")}association: {ap.Association.Name}");
            _writer.WriteLine($@"    {listPrefix}comment: {property.Comment}");
            if (!string.IsNullOrEmpty(ap.Role))
            {
                _writer.WriteLine($"    {listPrefix}role: {ap.Role}");
            }

            if (ap.Required)
            {
                _writer.WriteLine($"    {listPrefix}required: true");
            }

            if (ap.PrimaryKey)
            {
                _writer.WriteLine($"    {listPrefix}primaryKey: true");
            }
        }
        else if (property is TmdRegularProperty rp)
        {
            _writer.WriteLine($"    {(noList ? string.Empty : "- ")}name: {property.Name}");
            _writer.WriteLine($"    {listPrefix}domain: {rp.Domain}");
            if (rp.Required)
            {
                _writer.WriteLine($"    {listPrefix}required: true");
            }

            if (rp.PrimaryKey)
            {
                _writer.WriteLine($"    {listPrefix}primaryKey: true");
            }

            _writer.WriteLine($@"    {listPrefix}comment: {property.Comment}");
        }
        else if (property is TmdCompositionProperty cp)
        {
            if (cp.Composition == null)
            {
                return;
            }

            _writer.WriteLine($"    {(noList ? string.Empty : "- ")}composition: {cp.Composition.Name}");
            _writer.WriteLine($"    {listPrefix}name: {property.Name}");
            if (!string.IsNullOrEmpty(property.Domain))
            {
                _writer.WriteLine($"    {listPrefix}domain: {property.Domain}");
            }

            if (!cp.Required)
            {
                _writer.WriteLine($"    {listPrefix}required: false");
            }

            _writer.WriteLine($@"    {listPrefix}comment: {property.Comment}");
        }
        else if (property is TmdAliasProperty sp)
        {
            _writer.WriteLine($"    {(noList ? string.Empty : "- ")}alias:");
            _writer.WriteLine($"    {listPrefix}  class: {sp.Alias.Class.Name}");
            _writer.WriteLine($"    {listPrefix}  property: {sp.Alias.Name}");
            _writer.WriteLine($"    {listPrefix}name: {property.Name}");

            if (sp.Required)
            {
                _writer.WriteLine($"    {listPrefix}required: true");
            }

            if (!string.IsNullOrEmpty(sp.As))
            {
                _writer.WriteLine($"    {listPrefix}as: {sp.As}");
            }

            if (!string.IsNullOrEmpty(property.Comment))
            {
                _writer.WriteLine(@$"    {listPrefix}comment: {property.Comment}");
            }
        }
    }
}
