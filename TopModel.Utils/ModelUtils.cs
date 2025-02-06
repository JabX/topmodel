using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace TopModel.Utils;

/// <summary>
/// Regroupe quelques utilitaires pour la génération.
/// </summary>
public static class ModelUtils
{
    public static void CombinePath<T>(string directoryName, T classe, Expression<Func<T, string?>> getter)
    {
        var property = (PropertyInfo)((MemberExpression)getter.Body).Member;

        if (property.GetValue(classe) != null)
        {
            property.SetValue(classe, Path.GetFullPath(Path.Combine(directoryName, (string)property.GetValue(classe)!)).TrimEnd('/').Replace("\\", "/"));
        }
    }

    /// <summary>
    /// Convertit un text en camelCase.
    /// </summary>
    /// <param name="text">Le texte en entrée.</param>
    /// <param name="strict">camelCase stricte.</param>
    /// <param name="strictIfUppercase">camelCase stricte si le texte est en majuscule.</param>
    /// <returns>Le texte en sortie.</returns>
    public static string ToCamelCase(this string text, bool strict = false, bool strictIfUppercase = false)
    {
        return text.ToPascalCase(strict, strictIfUppercase).ToFirstLower();
    }

    /// <summary>
    /// Convertit un text en SNAKE_CASE majuscule.
    /// </summary>
    /// <param name="text">Le texte en entrée.</param>
    /// <returns>Le texte en sortie.</returns>
    public static string ToConstantCase(this string text)
    {
        var sb = new StringBuilder();
        var c = text.ToCharArray();
        var lastIsUp = true;
        var lastIsUnderscore = false;

        for (var i = 0; i < c.Length; ++i)
        {
            var upperChar = char.ToUpper(c[i]);
            var isLastChar = i == c.Length - 1;
            var nextIsLow = !isLastChar && char.ToUpper(c[i + 1]) != c[i + 1];

            if (upperChar == c[i] && upperChar != '_')
            {
                if (sb.Length != 0 && !lastIsUnderscore && (!lastIsUp || nextIsLow))
                {
                    sb.Append('_');
                }
            }

            lastIsUp = upperChar == c[i];
            lastIsUnderscore = c[i] == '_';

            sb.Append(upperChar);
        }

        return sb.ToString();
    }

    /// <summary>
    /// Met la première lettre d'un string en minuscule.
    /// </summary>
    /// <param name="text">Le texte en entrée.</param>
    /// <returns>Le texte en sortie.</returns>
    public static string ToFirstLower(this string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return text;
        }

        return char.ToLower(text[0]) + text[1..];
    }

    /// <summary>
    /// Met la première lettre d'un string en majuscule.
    /// </summary>
    /// <param name="text">Le texte en entrée.</param>
    /// <returns>Le texte en sortie.</returns>
    public static string ToFirstUpper(this string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return text;
        }

        return char.ToUpper(text[0]) + text[1..];
    }

    /// <summary>
    /// Convertit un text en dash-case.
    /// </summary>
    /// <param name="text">Le texte en entrée.</param>
    /// <returns>Le texte en sortie.</returns>
    public static string ToKebabCase(this string text)
    {
        return text.ToSnakeCase().Replace("_", "-");
    }

    /// <summary>
    /// Convertit un text en PascalCase.
    /// </summary>
    /// <param name="text">Le texte en entrée.</param>
    /// <param name="strict">PascalCase stricte.</param>
    /// <param name="strictIfUppercase">PascalCase stricte si le text est en majuscule.</param>
    /// <returns>Le texte en sortie.</returns>
    public static string ToPascalCase(this string text, bool strict = false, bool strictIfUppercase = false)
    {
        if (strictIfUppercase && text == text.ToUpperInvariant())
        {
            strict = true;
        }

        var invalidCharsRgx = new Regex("[^_\\w]");
        var whiteSpace = new Regex(@"(?<=\s)");
        var startsWithLowerCaseChar = new Regex("^[a-z]");
        var firstCharFollowedByUpperCasesOnly = new Regex("(?<=[A-Z])[A-Z0-9]+$");
        var lowerCaseNextToNumber = new Regex("(?<=[0-9])[a-z]");
        var upperCaseInside = new Regex("(?<=[A-Z])[A-Z]+?((?=[A-Z][a-z])|(?=[0-9]))");

        var pascalCase =

            // replace white spaces with undescore, then replace all invalid chars with empty string
            invalidCharsRgx.Replace(whiteSpace.Replace(text.Replace("-", "_"), "_"), string.Empty)

            // split by underscores
            .Split(['_'], StringSplitOptions.RemoveEmptyEntries)

            // set first letter to uppercase
            .Select(w => startsWithLowerCaseChar.Replace(w, m => m.Value.ToUpper()));

        if (strict)
        {
            // replace second and all following upper case letters to lower if there is no next lower (ABC -> Abc)
            pascalCase = pascalCase.Select(w => firstCharFollowedByUpperCasesOnly.Replace(w, m => m.Value.ToLower()));
        }

        // set upper case the first lower case following a number (Ab9cd -> Ab9Cd)
        pascalCase = pascalCase.Select(w => lowerCaseNextToNumber.Replace(w, m => m.Value.ToUpper()));

        if (strict)
        {
            // lower second and next upper case letters except the last if it follows by any lower (ABcDEf -> AbcDef)
            pascalCase = pascalCase.Select(w => upperCaseInside.Replace(w, m => m.Value.ToLower()));
        }

        return string.Concat(pascalCase);
    }

    public static string ToRelative(this string path, string? relativeTo = null)
    {
        var relative = Path.GetRelativePath(relativeTo ?? Directory.GetCurrentDirectory(), path);
        if (!relative.StartsWith('.'))
        {
            relative = $".\\{relative}";
        }

        return relative.Replace("\\", "/");
    }

    /// <summary>
    /// Convertit un text en snake_case minuscule.
    /// </summary>
    /// <param name="text">Le texte en entrée.</param>
    /// <returns>Le texte en sortie.</returns>
    public static string ToSnakeCase(this string text)
    {
        return ToConstantCase(text).ToLower();
    }

    public static void TrimSlashes<T>(T classe, Expression<Func<T, string?>> getter)
    {
        var property = (PropertyInfo)((MemberExpression)getter.Body).Member;

        if (property.GetValue(classe) != null)
        {
            property.SetValue(classe, ((string)property.GetValue(classe)!).Trim('/'));
        }
    }

    /// <summary>
    /// Ecrit un caractère dans le stream.
    /// </summary>
    /// <param name="fw">IFileWriter.</param>
    /// <param name="indentationLevel">Indentation.</param>
    /// <param name="value">Caractère.</para
    public static void Write(this IFileWriter fw, int indentationLevel, string value)
    {
        var indentValue = string.Empty;
        for (var i = 0; i < indentationLevel; ++i)
        {
            indentValue += fw.IndentValue;
        }

        value = value.Replace("\n", "\n" + indentValue);
        fw.Write(indentValue + value);
    }

    /// <summary>
    /// Ecrit un ligne dans le stream.
    /// </summary>
    /// <param name="fw">IFileWriter.</param>
    /// <param name="value">Chaîne de caractère.</param>
    public static void WriteLine(this IFileWriter fw, string? value = null)
    {
        fw.Write((value ?? string.Empty) + Environment.NewLine);
    }

    /// <summary>
    /// Ecrit la chaine avec le niveau indenté.
    /// </summary>
    /// <param name="fw">IFileWriter.</param>
    /// <param name="indentationLevel">Niveau d'indentation.</param>
    /// <param name="value">Valeur à écrire dans le flux.</param>
    public static void WriteLine(this IFileWriter fw, int indentationLevel, string value)
    {
        var indentValue = string.Empty;
        for (var i = 0; i < indentationLevel; ++i)
        {
            indentValue += fw.IndentValue;
        }

        value = value.Replace("\n", "\n" + indentValue);
        fw.WriteLine(indentValue + value);
    }
}