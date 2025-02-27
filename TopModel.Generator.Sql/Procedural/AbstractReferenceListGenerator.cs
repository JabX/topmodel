﻿using System.Text;
using Microsoft.Extensions.Logging;
using TopModel.Core;
using TopModel.Generator.Core;
using TopModel.Utils;

namespace TopModel.Generator.Sql.Procedural;

public abstract class AbstractReferenceListGenerator(ILogger<ClassGroupGeneratorBase<SqlConfig>> logger, IFileWriterProvider writerProvider)
    : ClassGroupGeneratorBase<SqlConfig>(logger, writerProvider)
{
    protected override bool PersistentOnly => true;

    /// <summary>
    /// Indique si pour une insertion dans une table avec une identité en mode séquence la colonne de PK doit être explicitement initialisée via la séquence.
    /// </summary>
    protected virtual bool ExplicitSequenceNextVal { get; } = false;

    /// <summary>
    /// Crée un dictionnaire { nom de la propriété => valeur } pour un item à insérer.
    /// </summary>
    /// <param name="modelClass">Modele de la classe.</param>
    /// <param name="initItem">Item a insérer.</param>
    /// <returns>Dictionnaire contenant { nom de la propriété => valeur }.</returns>
    protected Dictionary<string, string?> CreatePropertyValueDictionary(Class modelClass, ClassValue initItem)
    {
        var nameValueDict = new Dictionary<string, string?>();
        var definition = initItem.Value;
        foreach (var property in modelClass.Properties)
        {
            /* Cas d'un PK en mode séquence : on appelle nextval si la PK n'est pas fournie. */
            if (property.PrimaryKey && property.Domain.AutoGeneratedValue && Config.Procedural!.Identity.Mode == IdentityMode.SEQUENCE && ExplicitSequenceNextVal)
            {
                string sequenceName = Config.GetSequenceName(modelClass);
                nameValueDict[property.SqlName] =
                   definition.TryGetValue(property, out var value) ? value : GetNextValCall(sequenceName);
            }

            if (!property.PrimaryKey || !property.Domain.AutoGeneratedValue)
            {
                definition.TryGetValue(property, out var value);
                nameValueDict[property.SqlName] = Config.GetValue(property, Classes, value);

                if (Config.TranslateReferences == true && modelClass.DefaultProperty == property && !Config.CanClassUseEnums(modelClass, prop: property))
                {
                    nameValueDict[property.SqlName] = $@"'{initItem.ResourceKey}'";
                }
            }
        }

        return nameValueDict;
    }

    protected override IEnumerable<(string FileType, string FileName)> GetFileNames(Class classe, string tag)
    {
        if (classe.IsPersistent && !classe.Abstract && classe.Values.Count > 0)
        {
            yield return ("references", Config.Procedural!.InitListFile!);
        }
    }

    /// <summary>
    /// Renvoie le SQL pour appeler la valeur suivante d'une séquence donnée.
    /// </summary>
    /// <param name="sequenceName">Nom de la séquence.</param>
    /// <returns>SQL.</returns>
    protected virtual string GetNextValCall(string sequenceName)
        => throw new NotImplementedException($"Sequence declaration is not implemented with {Config.TargetDBMS}");

    protected override void HandleFile(string fileType, string fileName, string tag, IEnumerable<Class> classes)
    {
        using var writerInsert = this.OpenSqlWriter(fileName);

        writerInsert.WriteLine("-- =========================================================================================== ");
        writerInsert.WriteLine($"--   Application Name	:	{classes.First().Namespace.App} ");
        writerInsert.WriteLine("--   Script Name		:	" + fileName.Split('/').Last());
        writerInsert.WriteLine("--   Description		:	Script d'insertion des données de références");
        writerInsert.WriteLine("-- ===========================================================================================");

        WriteInsertStart(writerInsert);

        // Construit la liste des Reference Class ordonnée.
        var orderList = CoreUtils.Sort(classes.OrderBy(c => c.SqlName), c => c.Properties
            .OfType<AssociationProperty>()
            .Select(a => a.Association)
            .Where(a => a != c && a.Values.Count > 0 && a.IsPersistent));

        foreach (var classe in orderList)
        {
            WriteInsert(writerInsert, classe);
        }

        WriteInsertEnd(writerInsert);
    }

    protected virtual void WriteInsertEnd(IFileWriter writerInsert)
    {
    }

    protected virtual void WriteInsertStart(IFileWriter writerInsert)
    {
    }

    /// <summary>
    /// Retourne la ligne d'insert.
    /// </summary>
    /// <param name="modelClass">Modele de la classe.</param>
    /// <param name="initItem">Item a insérer.</param>
    /// <returns>Requête.</returns>
    private string GetInsertLine(Class modelClass, ClassValue initItem)
    {
        var propertyValueDict = CreatePropertyValueDictionary(modelClass, initItem);
        return GetInsertLine(modelClass.SqlName, propertyValueDict);
    }

    /// <summary>
    /// Retourne la ligne d'insert.
    /// </summary>
    /// <param name="tableName">Nom de la table dans laquelle ajouter la ligne.</param>
    /// <param name="propertyValuePairs">Dictionnaire au format {nom de la propriété => valeur}.</param>
    /// <returns>La requête "INSERT INTO ..." générée.</returns>
    private string GetInsertLine(string tableName, Dictionary<string, string?> propertyValuePairs)
    {
        var sb = new StringBuilder();
        sb.Append("INSERT INTO " + tableName + "(");
        var isFirst = true;
        foreach (var columnName in propertyValuePairs.Keys)
        {
            if (!isFirst)
            {
                sb.Append(", ");
            }

            isFirst = false;
            sb.Append(columnName);
        }

        sb.Append(") VALUES(");

        isFirst = true;
        foreach (var value in propertyValuePairs.Values)
        {
            if (!isFirst)
            {
                sb.Append(", ");
            }

            isFirst = false;
            sb.Append(string.IsNullOrEmpty(value) ? "null" : value);
        }

        sb.Append(");");
        return sb.ToString();
    }

    /// <summary>
    /// Ecrit dans le writer le script d'insertion dans la table staticTable ayant pour model modelClass.
    /// </summary>
    /// <param name="writer">Writer.</param>
    /// <param name="modelClass">Modele de la classe.</param>
    private void WriteInsert(IFileWriter writer, Class modelClass)
    {
        writer.WriteLine("/**\t\tInitialisation de la table " + modelClass.SqlName + "\t\t**/");
        foreach (var initItem in modelClass.Values)
        {
            writer.WriteLine(GetInsertLine(modelClass, initItem));
        }

        writer.WriteLine();
    }
}
