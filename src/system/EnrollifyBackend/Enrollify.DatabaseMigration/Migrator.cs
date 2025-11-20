using DbUp;
using DbUp.Engine;
using System.Reflection;

namespace Enrollify.DatabaseMigration;

internal class Migrator
{
    public static DatabaseUpgradeResult Migrate(string connectionString)
    {
        var upgrader = DeployChanges.To
            .SqlDatabase(connectionString)
            .WithExecutionTimeout(TimeSpan.FromMinutes(3))
            .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly(), s => s.StartsWith("Enrollify.DatabaseMigration.Scripts"))
            .LogToConsole()
            //.WithTransactionPerScript()
            .Build();
        return upgrader.PerformUpgrade();
    }
}

