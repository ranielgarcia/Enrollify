using DbUp.Engine;
using Enrollify.Core.Constants;
using System.Data;
using System.Text;

namespace Enrollify.DatabaseMigration.Seeds;

public class Seed0004__CurriculumStatuses : IScript
{
    public string ProvideScript(Func<IDbCommand> dbCommandFactory)
    {
        var curriculumStatuses = CurriculumStatusEnum.List;

        var scriptBuilder = new StringBuilder();

        scriptBuilder.Append(@"MERGE [CurriculumStatuses] As [Target]
            USING (");
        scriptBuilder.Append(@"VALUES");

        var values = new List<string>(curriculumStatuses.Count);

        foreach (var status in curriculumStatuses)
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
