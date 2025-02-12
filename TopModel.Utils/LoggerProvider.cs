using Microsoft.Extensions.Logging;
using Spectre.Console;

namespace TopModel.Utils;

public class LoggerProvider : ILoggerProvider
{
    public int Changes { get; private set; }

    /// <inheritdoc cref="ILoggerProvider.CreateLogger" />
    public ILogger CreateLogger(string categoryName)
    {
        return new ConsoleLogger(categoryName.Split(".").Last(), () => Changes++);
    }

    public void Dispose()
    {
    }

    public class ConsoleLogger : ILogger
    {
        private static readonly object _lock = new();

        private readonly string _categoryName;
        private readonly Action _registerChange;
        private string? _generatorName;
        private string? _storeColor;
        private int? _storeNumber;

        public ConsoleLogger(string categoryName, Action registerChange)
        {
            _categoryName = categoryName;
            _registerChange = registerChange;
        }

        /// <inheritdoc cref="ILogger.BeginScope{TState}" />
        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull
        {
            if (state is LoggingScope scope)
            {
                _storeNumber = scope.Number;
                _storeColor = scope.Color;
            }
            else if (state is string generatorName)
            {
                _generatorName = generatorName;
            }

            return null;
        }

        /// <inheritdoc cref="ILogger.IsEnabled" />
        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        /// <inheritdoc cref="ILogger.Log{TState}" />
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            lock (_lock)
            {
                var message = formatter(state, exception);

                if (message == string.Empty)
                {
                    AnsiConsole.WriteLine();
                    return;
                }

                if (_storeNumber != null && _storeColor != null)
                {
                    AnsiConsole.Markup($"[{_storeColor}]#{_storeNumber.Value} [/]");
                }

                var name = ((_generatorName ?? _categoryName) + " ").PadRight(22, '-');
                var split = name.Split(" ");
                var fColor = _generatorName != null ? "fuchsia" : "grey";
                AnsiConsole.Markup($"[{fColor}]{split[0]}[/]");
                AnsiConsole.Markup($" {split[1]} ");

                message = WriteAction(message, "Supprimé", "maroon");
                message = WriteAction(message, "Créé", "green");
                message = WriteAction(message, "Modifié", "teal");

                if (logLevel != LogLevel.Error && logLevel != LogLevel.Warning)
                {
                    var split2 = message.Split('/');
                    var color = "silver";
                    if (split2.Length > 1)
                    {
                        AnsiConsole.Markup($"{string.Join('/', split2[0..^1])}/");
                        color = "blue";
                    }

                    var split3 = split2[^1].Split('\'');
                    if (split3.Length == 2)
                    {
                        AnsiConsole.Markup($"{split3[0]}");
                        AnsiConsole.MarkupLine($"'{split3[1]}");
                    }
                    else
                    {
                        AnsiConsole.MarkupLine($"[{color}]{split2[^1]}[/]");
                    }
                }
                else
                {
                    AnsiConsole.MarkupLine(message);
                }

                if (exception is not null and not LegitException)
                {
                    AnsiConsole.MarkupLine(exception.Message);
                }
            }
        }

        public string WriteAction(string message, string action, string color)
        {
            if (message.LastIndexOf(action) >= 0)
            {
                _registerChange();
                AnsiConsole.Markup($"[{color}]{action}[/]");
                return message.Split(action)[1];
            }

            return message;
        }
    }
}