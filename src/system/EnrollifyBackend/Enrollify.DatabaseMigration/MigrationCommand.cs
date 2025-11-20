using Microsoft.Extensions.Configuration;
using Spectre.Console;
using Spectre.Console.Cli;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Enrollify.DatabaseMigration;

internal sealed class MigrationCommand : Command<MigrationCommand.Settings>
{
    private readonly IConfiguration _configuration;

    public MigrationCommand(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public sealed class Settings : CommandSettings
    {
        [Description("Override connection string from config.")]
        [CommandOption("-c|--connection-string")]
        public string? ConnectionString { get; init; }

        [Description("Create the database if it does not exists.")]
        [CommandOption("-f|--force-ensure-database")]
        [DefaultValue(true)]
        public bool ForceEnsureDatabase { get; init; }

        [Description("Create mock data.")]
        [CommandOption("-m|--mock-data")]

#if DEBUG
        [DefaultValue(true)]
#else
        [DefaultValue(false)]
#endif
        public bool MockData { get; init; }

        [Description("Create seed data.")]
        [CommandOption("-s|--seed-data")]
        [DefaultValue(true)]
        public bool SeedData { get; init; }
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        var connectionString = settings.ConnectionString ?? _configuration.GetConnectionString("Default");
        if (connectionString == null) throw new ArgumentNullException(nameof(connectionString));

        try
        {
            DatabaseUpgrader.RunDbUpgradeActivities(connectionString, settings.ForceEnsureDatabase, settings.MockData, settings.SeedData);
        }
        catch (Exception e)
        {
            AnsiConsole.MarkupLine($"[red]{e.Message}[/]");
            throw;
        }

        return 0;
    }



}