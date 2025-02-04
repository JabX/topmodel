using System.Text;
using TopModel.Generator.Core;
using TopModel.Utils;

namespace TopModel.Generator.Php;

public static class PhpUtils
{
    public static PhpWriter OpenPhpWriter(this GeneratorBase<PhpConfig> generator, string fileName, string packageName, int? codePage = 1252)
    {
        return new PhpWriter(generator.OpenFileWriter(fileName, codePage != null ? CodePagesEncodingProvider.Instance.GetEncoding(codePage.Value)! : new UTF8Encoding(false)), packageName);
    }

    public static string ToFilePath(this string path)
    {
        return path.Replace(':', '.').Replace('.', Path.DirectorySeparatorChar);
    }

    public static string ToPackageName(this string path)
    {
        return @"App\" + path.Split(':').Last().Replace('/', '\\').Replace('.', '\\');
    }

    public static string WithPrefix(this string name, string prefix)
    {
        return $"{prefix}{name.ToFirstUpper()}";
    }
}
