﻿using Microsoft.Extensions.Logging;
using TopModel.Core;
using TopModel.Utils;

namespace TopModel.Generator.Sql.Procedural.Postgres;

public class PostgresCrebasGenerator(ILogger<PostgresCrebasGenerator> logger, TranslationStore translationStore, IFileWriterProvider writerProvider)
    : AbstractCrebasGenerator(logger, translationStore, writerProvider)
{
    public override string Name => "PostgresCrebasGen";

    protected override string JsonType => "jsonb";

    protected override bool SupportsClusteredKey => false;

    /// <summary>
    /// Gère l'auto-incrémentation des clés primaires en ajoutant identity à la colonne.
    /// </summary>
    /// <param name="writer">Flux d'écriture création bases.</param>
    protected override void WriteIdentityColumn(IFileWriter writer)
    {
        writer.Write(" generated by default as identity");
        if (Config.Procedural!.Identity.Increment != null || Config.Procedural!.Identity.Start != null)
        {
            writer.Write(" (");
            if (Config.Procedural!.Identity.Start != null)
            {
                writer.Write($"{$"start with {Config.Procedural!.Identity.Start}"}");
            }

            if (Config.Procedural!.Identity.Increment != null)
            {
                writer.Write($"{$" increment {Config.Procedural!.Identity.Increment}"}");
            }

            writer.Write(")");
        }
    }

    protected override void WriteSequenceDeclaration(Class classe, IFileWriter writerCrebas, string tableName)
    {
        writerCrebas.Write($"create sequence {Config.GetSequenceName(classe)} as {Config.GetType(classe.PrimaryKey.Single()).ToUpper()}");

        if (Config.Procedural!.Identity.Start != null)
        {
            writerCrebas.Write($"{$" start {Config.Procedural!.Identity.Start}"}");
        }

        if (Config.Procedural!.Identity.Increment != null)
        {
            writerCrebas.Write($"{$" increment {Config.Procedural!.Identity.Increment}"}");
        }

        writerCrebas.Write($" owned by {tableName}.{classe.PrimaryKey.Single().SqlName}");
    }
}
