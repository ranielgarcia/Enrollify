using DbUp.Engine;
using Enrollify.Core.Constants;
using Microsoft.Extensions.Primitives;
using System.Data;
using System.Text;

namespace Enrollify.DatabaseMigration.Seeds;

public class Seed0002__PermissionScopes : IScript
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

        foreach(var scoped in permissionScopes)
        {
            scriptBuilder.Append(string.Format("({0}, '{1}')", scoped.Value, scoped.Name));
        }
        scriptBuilder.Append(") AS [Source] ([Id], [Name])");
        scriptBuilder.Append(" ON [Target].[Id] = [Source].[Id]");
        scriptBuilder.Append(" WHEN MATCHED THEN UPDATE SET [Target].[Name] = [Source].[Name]");
        scriptBuilder.Append(" WHEN NOT MATCHED THEN INSERT ([Id], [Name], [CreatedBy]) VALUES ([Source].[Id], [Source].[Name], @InitialUserId);");

        scriptBuilder.Append(" SET IDENTITY_INSERT dbo.PermissionScopes OFF;");

        return scriptBuilder.ToString();
    }
}
