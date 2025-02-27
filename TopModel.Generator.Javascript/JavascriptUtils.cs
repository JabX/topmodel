﻿using TopModel.Core;
using TopModel.Utils;

namespace TopModel.Generator.Javascript;

public static class JavascriptUtils
{
    public static List<(string Import, string Path)> GroupAndSort(this IEnumerable<(string Import, string Path)> imports)
    {
        return imports
             .GroupBy(i => i.Path)
             .Select(i => (Import: string.Join(", ", i.Select(l => l.Import).Distinct().OrderBy(x => x)), Path: i.Key))
             .OrderBy(i => i.Path.StartsWith('.') ? i.Path : $"...{i.Path}")
             .ToList();
    }

    public static bool IsJSReference(this Class classe)
    {
        return classe.EnumKey != null || classe.Reference && !classe.ReferenceKey!.Domain.AutoGeneratedValue;
    }

    public static void WriteReferenceDefinition(IFileWriter fw, Class classe)
    {
        fw.Write("export const ");
        fw.Write(classe.NameCamel);
        fw.Write(" = {type: {} as ");
        fw.Write(classe.NamePascal);
        fw.Write(", valueKey: \"");
        fw.Write(classe.ReferenceKey!.NameCamel);
        fw.Write("\", labelKey: \"");
        fw.Write(classe.DefaultProperty?.NameCamel);
        fw.Write("\"} as const;\r\n");
    }
}
