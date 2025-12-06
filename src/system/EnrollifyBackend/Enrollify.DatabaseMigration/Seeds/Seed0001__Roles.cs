using DbUp.Engine;
using Enrollify.Core.Constants.Authorization;
using System.Data;
using System.Text;

namespace Enrollify.DatabaseMigration.Seeds;

public class Seed0001__Roles : IScript
{
    public string ProvideScript(Func<IDbCommand> dbCommandFactory)
    {
        var scriptBuilder = new StringBuilder();

        scriptBuilder.Append("""
            DECLARE @InitialUserId INT = (SELECT Id FROM Users WHERE Email='system@enrollify.local');
            SET IDENTITY_INSERT dbo.Roles ON;
            """);

        scriptBuilder.Append("""
            MERGE [Roles] As [Target]
            USING (VALUES
            """);

        var values = new List<string>();

        foreach (var role in RolesEnum.List)
        {
            values.Add(string.Format("({0}, '{1}', '{2}')", role.Value, role.Name, role.Description));
        }

        scriptBuilder.Append(string.Join(", ", values.ToArray()));

        scriptBuilder.Append("""
            ) AS [Source] ([Id], [Name], [Description])
            ON [Target].[Id] = [Source].[Id]
            WHEN MATCHED THEN
            	UPDATE SET [Target].[Name] = [Source].[Name], [Target].[Description] = [Source].[Description]
            WHEN NOT MATCHED THEN
            	INSERT ([Id], [Name], [Description], [CreatedBy]) VALUES ([Source].[Id], [Source].[Name], [Source].[Description], @InitialUserId);
            """);


        scriptBuilder.Append("SET IDENTITY_INSERT dbo.Roles OFF;");

        return scriptBuilder.ToString();
    }
}
