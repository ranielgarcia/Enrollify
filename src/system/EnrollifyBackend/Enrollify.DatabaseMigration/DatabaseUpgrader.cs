using DbUp;
using DbUp.Engine;
using Spectre.Console;
using System.Diagnostics;

namespace Enrollify.DatabaseMigration;

public static class DatabaseUpgrader
{
    public static void RunDbUpgradeActivities(string connectionString, bool forceEnsureDatabase, bool createMockData, bool createSeedData)
    {
        // Only try and create database if we're not running in the release pipeline (In Azure DB it is already created for us by Terraform)
        if (forceEnsureDatabase || Environment.GetEnvironmentVariable("TF_BUILD") == null)
        {
            EnsureDatabase.For.SqlDatabase(connectionString);
        }
        var sw = Stopwatch.StartNew();

        var dbScriptVersioner = new DatabaseScriptVersioner(connectionString);
        if (dbScriptVersioner.IsDatabaseAtCurrentScriptVersion())
        {
            AnsiConsole.MarkupLine("[green]Database up to date - skipping migration[/]");
            return;
        }

        var migrationResult = RunDbActivity("Migration", () => Migrator.Migrate(connectionString));
        AnsiConsole.MarkupLine($"[green]Migration took {sw.Elapsed.TotalSeconds} seconds[/]");
        sw.Restart();

        var seedResult = createSeedData ? RunDbActivity("Seed Data Creation", () => Seeder.Seed(connectionString)) : null;
        AnsiConsole.MarkupLine($"[green]Seed Data Creation took {sw.Elapsed.TotalSeconds} seconds[/]");
        sw.Restart();

        var schemaResult = RunDbActivity("Views, TVFs and Stored Procedures", () => SchemaMigrator.Migrate(connectionString));
        AnsiConsole.MarkupLine($"[green]Views, TVFs and Stored Procedures took {sw.Elapsed.TotalSeconds} seconds[/]");
        sw.Restart();

        var mockResult = createMockData ? RunDbActivity("Mock Data Creation", () => Mocker.CreateMockData(connectionString)) : null;
        AnsiConsole.MarkupLine($"[green]Mock Data Creation took {sw.Elapsed.TotalSeconds} seconds[/]");
        sw.Restart();

        if (migrationResult.Successful
            && (createMockData || seedResult is { Successful: true })
            && schemaResult.Successful
            && (createMockData || mockResult is { Successful: true }))
        {
            AnsiConsole.MarkupLine("[green]Success![/]");
            dbScriptVersioner.UpdateScriptVersion();
        }
    }
    private static DatabaseUpgradeResult RunDbActivity(string activityName, Func<DatabaseUpgradeResult> action)
    {
        var result = action();

        if (result.Successful)
        {
            AnsiConsole.MarkupLine("[green]Complete[/]");
        }
        else
        {
            throw result.Error;
        }
        return result;
    }
}


