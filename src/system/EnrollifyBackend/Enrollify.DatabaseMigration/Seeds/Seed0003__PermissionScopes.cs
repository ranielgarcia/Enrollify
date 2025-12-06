using DbUp.Engine;
using Enrollify.Core.Constants.Authorization;
using Microsoft.Extensions.Primitives;
using System.Data;
using System.Text;

namespace Enrollify.DatabaseMigration.Seeds;

public class Seed0003__PermissionScopes : IScript
{
    public string ProvideScript(Func<IDbCommand> dbCommandFactory)
    {
        var permissionScopes = PermissionScopeEnum.List;

        var scriptBuilder = new StringBuilder();

        scriptBuilder.Append("DECLARE @InitialUserId INT = (SELECT Id FROM Users WHERE Email='system@enrollify.local');");
        scriptBuilder.Append("SET IDENTITY_INSERT dbo.PermissionScopes ON;");
        scriptBuilder.Append(@"MERGE [PermissionScopes] As [Target]
            USING (");
        scriptBuilder.Append(@"VALUES");

        var values = new List<string>();

        foreach(var scoped in permissionScopes)
        {
            values.Add(string.Format("({0}, '{1}')", scoped.Value, scoped.Name));
        }
        scriptBuilder.Append(string.Join(", ", values.ToArray()));
        scriptBuilder.Append(") AS [Source] ([Id], [Name])");
        scriptBuilder.Append(" ON [Target].[Id] = [Source].[Id]");
        scriptBuilder.Append(" WHEN MATCHED THEN UPDATE SET [Target].[Name] = [Source].[Name]");
        scriptBuilder.Append(" WHEN NOT MATCHED THEN INSERT ([Id], [Name], [CreatedBy]) VALUES ([Source].[Id], [Source].[Name], @InitialUserId);");

        scriptBuilder.Append(" SET IDENTITY_INSERT dbo.PermissionScopes OFF;");

        return scriptBuilder.ToString();
    }
}
