using DbUp;
using DbUp.Engine;
using DbUp.Helpers;
using System.Reflection;

namespace Enrollify.DatabaseMigration;

internal static class Seeder
{
    public static DatabaseUpgradeResult Seed(string connectionString)
    {
        var upgrader = DeployChanges.To
            .SqlDatabase(connectionString)
            .WithExecutionTimeout(TimeSpan.FromMinutes(5))
            .WithScriptsAndCodeEmbeddedInAssembly(Assembly.GetExecutingAssembly(),
                s => s.StartsWith("Enrollify.DatabaseMigration.Seeds"))
            .JournalTo(new NullJournal())
            .LogToConsole()
            .LogScriptOutput()
            .Build();

        return upgrader.PerformUpgrade();
    }
}