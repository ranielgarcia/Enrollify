using System.Data;
using System.Text;
using DbUp.Engine;
using Enrollify.Core.Constants;

namespace Enrollify.DatabaseMigration.Seeds;

public class Seed0009__ClassSectionStatuses : IScript
{
    public string ProvideScript(Func<IDbCommand> dbCommandFactory)
    {
        var classSectionStatuses = ClassSectionStatusEnum.List;

        var scriptBuilder = new StringBuilder();

        scriptBuilder.Append(@"MERGE [ClassSectionStatuses] As [Target]
            USING (");
        scriptBuilder.Append(@"VALUES");

        var values = new List<string>(classSectionStatuses.Count);

        foreach (var status in classSectionStatuses)
        {
            values.Add(string.Format("({0}, '{1}')", status.Value, status.Name));
        }
        scriptBuilder.Append(string.Join(", ", values.ToArray()));
        scriptBuilder.Append(") AS [Source] ([Id], [Name])");
        scriptBuilder.Append(" ON [Target].[Id] = [Source].[Id]");
        scriptBuilder.Append(" WHEN MATCHED THEN UPDATE SET [Target].[Name] = [Source].[Name]");
        scriptBuilder.Append(" WHEN NOT MATCHED THEN INSERT ([Id], [Name]) VALUES ([Source].[Id], [Source].[Name]);");

        return scriptBuilder.ToString();
    }
}
