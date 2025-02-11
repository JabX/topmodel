using System.CommandLine.Invocation;
using Spectre.Console;
using TopModel.Utils;

public class InitCommandHandler : ICommandHandler
{
    public InitCommandHandler()
    {
    }

    /// <inheritdoc cref="ICommandHandler.Invoke" />
    public int Invoke(InvocationContext context)
    {
        var config = "# yaml-language-server: $schema=./topmodel.config.schema.json\n";
        AnsiConsole.WriteLine("Bonjour yolo clic-clic, TopModel c'est génial");
        var appName = AnsiConsole.Prompt(new TextPrompt<string>("Quel sera le nom de votre application ?").DefaultValue("my-app"));
        config += $"app: {appName}\n";
        var shouldUseCustomGenerators = AnsiConsole.Prompt(new TextPrompt<bool>("Souhaitez-vous utiliser des générateurs customs ?")
            .AddChoice(true)
            .AddChoice(false)
            .WithConverter(choice => choice ? "y" : "n")
            .DefaultValue(false));
        if (shouldUseCustomGenerators)
        {
            config += "generators:\n";
            while (shouldUseCustomGenerators)
            {
                var path = AnsiConsole.Prompt(new TextPrompt<string>("Quel est le chemin de votre générateur ?").DefaultValue("../../mon/generateur"));
                config += $"  - {path}\n";
                shouldUseCustomGenerators = AnsiConsole.Prompt(new TextPrompt<bool>("Souhaitez-vous utiliser d'autres générateurs customs ?")
                .AddChoice(true)
                .AddChoice(false)
                .WithConverter(choice => choice ? "y" : "n")
                .DefaultValue(false));
            }
        }

        var modules = AnsiConsole.Prompt(
            new MultiSelectionPrompt<string>()
                .Title("Quelles configurations souhaitez vous ajouter ?")
                .PageSize(10)
                .MoreChoicesText("[grey](Utiliser le flèches pour afficher plus de configurations)[/]")
                .InstructionsText(
                    "[grey](Appuyer sur [blue]<space>[/] pour selectionner ou déselectionner une configuration, " +
                    "[green]<enter>[/] pour accepter)[/]")
                .AddChoices([
                    "csharp", "jpa", "sql", "javascript"
                ]));
        foreach (var module in modules)
        {
            config += PromptModule(module);
        }

        File.WriteAllText("topmodel.config", config);
        return 0;
    }

    /// <inheritdoc cref="ICommandHandler.InvokeAsync" />
    public Task<int> InvokeAsync(InvocationContext context)
    {
        return Task.FromResult(Invoke(context));
    }

    public string PromptModule(string module)
    {
        var result = $"{module}:\n";
        result += $"  - tags:\n";
        var nextTag = AnsiConsole.Prompt(new TextPrompt<string>($"[blue]{module}[/] Quel tag cette configuration doit-elle cibler ?"));
        List<string> tags = [];
        while (nextTag != string.Empty)
        {
            tags.Add(nextTag);
            tags = [.. tags.Distinct()];
            AnsiConsole.WriteLine($"tags : {string.Join(", ", tags)}");
            nextTag = AnsiConsole.Prompt(new TextPrompt<string>($"[blue]{module}[/] Quel autre tag cette configuration doit-elle cibler ? [blue]<enter>[/] pour terminer").AllowEmpty());
        }

        result += string.Join(string.Empty, tags.Select(t => $"      - {t}\n"));
        var outputDirectory = AnsiConsole.Prompt(new TextPrompt<string>($"[blue]{module}[/]Quel sera le dossier cible de la génération ?").DefaultValue($"./{module}"));
        result += $"    outputDirectory: {outputDirectory}\n";
        // Charger le wizard de chaque générateur...
        return result;
    }
}
