using DbUp.Engine;
using System.Data;
using System.Text;

namespace Enrollify.DatabaseMigration.Seeds;

public class Seed0010__StudentTypes : IScript
{
    public string ProvideScript(Func<IDbCommand> dbCommandFactory)
    {
        var studentTypes = Core.Constants.StudentTypeEnum.List;

        var scriptBuilder = new StringBuilder();

        scriptBuilder.Append("DECLARE @InitialUserId INT = (SELECT Id FROM Users WHERE Email='system@enrollify.local');");

        scriptBuilder.Append(@"MERGE [StudentTypes] As [Target]
            USING (");
        scriptBuilder.Append(@"VALUES");

        var values = new List<string>();

        foreach (var type in studentTypes)
        {
            values.Add(string.Format("({0}, '{1}', '{2}', '{3}', {4})", type.Value, type.Code, type.Name, type.Description, type.DisplayOrder));
        }
        scriptBuilder.Append(string.Join(", ", values.ToArray()));
        scriptBuilder.Append(") AS [Source] ([Id], [Code], [Name], [Description], [DisplayOrder])");
        scriptBuilder.Append(" ON [Target].[Id] = [Source].[Id]");
        scriptBuilder.Append(" WHEN MATCHED THEN UPDATE SET [Target].[Code] = [Source].[Code], [Target].[Name] = [Source].[Name], [Target].[Description] = [Source].[Description], [Target].[DisplayOrder] = [Source].[DisplayOrder], [Target].[UpdatedBy] = @InitialUserId");
        scriptBuilder.Append(" WHEN NOT MATCHED THEN INSERT ([Id], [Code], [Name], [Description], [DisplayOrder], [CreatedBy]) VALUES ([Source].[Id], [Source].[Code], [Source].[Name], [Source].[Description], [Source].[DisplayOrder], @InitialUserId);");

        return scriptBuilder.ToString();
    }
}
