using DbUp.Engine;
using Enrollify.Core.Constants.Authorization;
using System.Data;
using System.Text;

namespace Enrollify.DatabaseMigration.Seeds;

public class Seed0005__SuperAdminPermissions : IScript
{
    public string ProvideScript(Func<IDbCommand> dbCommandFactory)
    {
        var scriptBuilder = new StringBuilder();

        scriptBuilder.Append("""
            DECLARE @RoleId INT = (SELECT Id FROM Roles WHERE Name='SystemAdmin');
            DECLARE @InitialUserId INT = (SELECT Id FROM Users WHERE Email='system@enrollify.local');

            MERGE [RolePermissions] AS [Target] USING (VALUES
            """);

        var values = new List<string>();

        foreach (var scope in PermissionScopeEnum.List)
        {
            values.Add(string.Format("({0}, {1})", scope.Value, PermissionEnum.Full.Value));
        }
        scriptBuilder.Append(string.Join(", ", values.ToArray()));

        scriptBuilder.Append("""
            ) AS [Source]([PermissionScopeId], [BitmaskPermission])
                ON [Target].[RoleId] = @roleId AND [Target].[PermissionScopeId]=[Source].[PermissionScopeId]
            WHEN MATCHED THEN
            	UPDATE SET [Target].[BitmaskPermission] = [Source].[BitmaskPermission]
            WHEN NOT MATCHED THEN
            	INSERT (RoleId, PermissionScopeId, BitmaskPermission, CreatedBy) VALUES (@RoleId, [Source].[PermissionScopeId], [Source].[BitmaskPermission], @InitialUserId);
            """);

        return scriptBuilder.ToString();
    }
}
