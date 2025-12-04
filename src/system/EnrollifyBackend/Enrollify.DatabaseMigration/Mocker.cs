using DbUp;
using DbUp.Engine;
using DbUp.Helpers;
using DbUp.Support;
using Spectre.Console;
using System.Reflection;

namespace Enrollify.DatabaseMigration;

internal static class Mocker
{
    private const string mockFolderNamespace = "Enrollify.DatabaseMigration.Mocks";

    public static DatabaseUpgradeResult CreateMockData(string connectionString)
    {
        var runtimeEnvironment = Environment.GetEnvironmentVariable("ENVIRONMENT");

        var environments = new List<string> { "Common" }; // Always run Common mock scripts
        if (!string.IsNullOrWhiteSpace(runtimeEnvironment) && !environments.Any(e => e.ToLowerInvariant().Equals(runtimeEnvironment.ToLowerInvariant())))
        {
            // Apply mock overrides per environment
            environments.Add(runtimeEnvironment);
        }

        AnsiConsole.MarkupLine($"[blue]Executing mock scrpts for '{string.Join(',', environments)}' environment[/]");

        var upgrader = DeployChanges.To
            .SqlDatabase(connectionString)
            .WithTransactionPerScript()
            .WithScriptsAndCodeEmbeddedInAssembly(
                Assembly.GetExecutingAssembly(), 
                (s => ShouldRunScript(s, environments)), 
                new SqlScriptOptions { ScriptType = ScriptType.RunAlways }
             )
            .JournalTo(new NullJournal())
            .LogToConsole()
            .Build();
        return upgrader.PerformUpgrade();
    }

    private static bool ShouldRunScript(string script, IEnumerable<string> environments)
    {
        var mockFolders = environments.Select(e => $"{mockFolderNamespace}.{e}");
        return mockFolders.Any(f => script.StartsWith(f, StringComparison.OrdinalIgnoreCase));
    }
}