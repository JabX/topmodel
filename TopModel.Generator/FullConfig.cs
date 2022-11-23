﻿using TopModel.Core;
using TopModel.Generator.CSharp;
using TopModel.Generator.Javascript;
using TopModel.Generator.Jpa;
using TopModel.Generator.ProceduralSql;
using TopModel.Generator.Ssdt;
using TopModel.Generator.Translation;

namespace TopModel.Generator;

public class FullConfig : ModelConfig
{
    public IList<ProceduralSqlConfig>? ProceduralSql { get; set; }

    public IList<SsdtConfig>? Ssdt { get; set; }

    public IList<JavascriptConfig>? Javascript { get; set; }

    public IList<CSharpConfig>? Csharp { get; set; }

    public IList<JpaConfig>? Jpa { get; set; }

    public IList<TranslationConfig>? Translation { get; set; }
}