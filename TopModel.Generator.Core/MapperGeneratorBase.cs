using Microsoft.Extensions.Logging;
using TopModel.Core;
using TopModel.Core.FileModel;
using TopModel.Utils;

namespace TopModel.Generator.Core;

public abstract class MapperGeneratorBase<T> : GeneratorBase<T>
    where T : GeneratorConfigBase
{
    [Obsolete("Utiliser la surcharge avec le IFileWriterProvider")]
    public MapperGeneratorBase(ILogger<MapperGeneratorBase<T>> logger)
        : base(logger)
    {
    }

    public MapperGeneratorBase(ILogger<MapperGeneratorBase<T>> logger, IFileWriterProvider writerProvider)
        : base(logger, writerProvider)
    {
    }

    public override IEnumerable<string> GeneratedFiles =>
        FromMappers.SelectMany(m => Config.Tags.Intersect(GetMapperTags(m)).Select(tag => GetFileName(m, tag)))
        .Concat(ToMappers.SelectMany(m => Config.Tags.Intersect(GetMapperTags(m)).Select(tag => GetFileName(m, tag))))
        .Distinct();

    protected IEnumerable<(Class Classe, FromMapper Mapper)> FromMappers => Classes
        .SelectMany(classe => classe.FromMappers.Select(mapper => (classe, mapper)))
        .Where(mapper => mapper.mapper.ClassParams.All(p => Classes.Contains(p.Class)))
        .Select(c => (c.classe, c.mapper));

    protected IEnumerable<(Class Classe, ClassMappings Mapper)> ToMappers => Classes
        .SelectMany(classe => classe.ToMappers.Select(mapper => (classe, mapper))
        .Where(mapper => Classes.Contains(mapper.mapper.Class)))
        .Select(c => (c.classe, c.mapper));

    protected abstract string GetFileName((Class Classe, FromMapper Mapper) mapper, string tag);

    protected abstract string GetFileName((Class Classe, ClassMappings Mapper) mapper, string tag);

    protected abstract void HandleFile(string fileName, string tag, IList<(Class Classe, FromMapper Mapper)> fromMappers, IList<(Class Classe, ClassMappings Mapper)> toMappers);

    protected override void HandleFiles(IEnumerable<ModelFile> files)
    {
        var fromMappers = FromMappers.SelectMany(mapper => Config.Tags.Intersect(GetMapperTags(mapper))
            .Select(tag => (FileName: GetFileName(mapper, tag), Mapper: mapper, Tag: tag)))
            .GroupBy(f => f.FileName)
            .ToDictionary(f => f.Key, f =>
            {
                var tags = f.Select(m => m.Tag);
                return (
                    Mappers: f.Select(m => m.Mapper)
                        .Distinct()
                        .OrderBy(m => m.Classe.NamePascal, StringComparer.Ordinal)
                        .ThenBy(m => m.Mapper.Params.Count)
                        .ThenBy(m => string.Join(',', m.Mapper.Params.Select(p => p.GetName())), StringComparer.Ordinal)
                        .ToArray(),
                    Tags: tags);
            });

        var toMappers = ToMappers.SelectMany(mapper => Config.Tags.Intersect(GetMapperTags(mapper))
            .Select(tag => (FileName: GetFileName(mapper, tag), Mapper: mapper, Tag: tag)))
            .GroupBy(f => f.FileName)
            .ToDictionary(f => f.Key, f => (
                Mappers: f.Select(m => m.Mapper)
                    .Distinct()
                    .OrderBy(m => $"{m.Mapper.Name} {m.Classe.NamePascal}", StringComparer.Ordinal)
                    .ToArray(),
                Tags: f.Select(m => m.Tag)));

        Parallel.ForEach(fromMappers.Keys.Concat(toMappers.Keys).Distinct(), fileName =>
        {
            var (fileFromMappers, fromTags) = fromMappers.ContainsKey(fileName) ? fromMappers[fileName] : (Array.Empty<(Class, FromMapper)>(), Array.Empty<string>());
            var (fileToMappers, toTags) = toMappers.ContainsKey(fileName) ? toMappers[fileName] : (Mappers: Array.Empty<(Class, ClassMappings)>(), Tags: Array.Empty<string>());
            HandleFile(fileName, fromTags.Concat(toTags).First(), fileFromMappers, fileToMappers);
        });
    }

    protected virtual bool IsPersistent(Class classe)
    {
        return classe.IsPersistent;
    }

    private IEnumerable<string> GetMapperTags((Class Classe, FromMapper Mapper) mapper)
    {
        if (IsPersistent(mapper.Classe))
        {
            return mapper.Classe.Tags;
        }

        var persistentParam = mapper.Mapper.ClassParams.FirstOrDefault(p => IsPersistent(p.Class));
        if (persistentParam != null)
        {
            return persistentParam.Class.Tags;
        }

        return mapper.Classe.Tags;
    }

    private IEnumerable<string> GetMapperTags((Class Classe, ClassMappings Mapper) mapper)
    {
        if (IsPersistent(mapper.Classe))
        {
            return mapper.Classe.Tags;
        }

        if (IsPersistent(mapper.Mapper.Class))
        {
            return mapper.Mapper.Class.Tags;
        }

        return mapper.Classe.Tags;
    }
}