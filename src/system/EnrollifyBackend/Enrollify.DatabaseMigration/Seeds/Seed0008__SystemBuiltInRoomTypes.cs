using DbUp.Engine;
using System.Data;
using System.Text;

namespace Enrollify.DatabaseMigration.Seeds;

internal class Seed0008__SystemBuiltInRoomTypes : IScript
{
    public string ProvideScript(Func<IDbCommand> dbCommandFactory)
    {
        var builtInRoomTypes = Core.Constants.AcademicBuiltInData.BuiltInRoomTypeEnum.List;

        var scriptBuilder = new StringBuilder();

        scriptBuilder.Append("DECLARE @InitialUserId INT = (SELECT Id FROM Users WHERE Email='system@enrollify.local');");

        scriptBuilder.Append(@"MERGE [RoomTypes] AS [Target]
            USING (");
        scriptBuilder.Append(@"VALUES");

        var values = new List<string>();

        foreach (var roomType in builtInRoomTypes)
        {
            var descriptionValue = !string.IsNullOrEmpty(roomType.Description)
                ? $"'{roomType.Description.Replace("'", "''")}'"
                : "NULL";

            values.Add(string.Format("('{0}', {1})",
                roomType.Name.Replace("'", "''"),
                descriptionValue));
        }

        scriptBuilder.Append(string.Join(", ", values.ToArray()));
        scriptBuilder.Append(") AS [Source] ([Name], [Description])");
        scriptBuilder.Append(" ON [Target].[Name] = [Source].[Name]");
        scriptBuilder.Append(" WHEN MATCHED THEN UPDATE SET [Target].[Description] = [Source].[Description], [Target].[UpdatedBy] = @InitialUserId, [Target].[UpdatedAt] = GETUTCDATE()");
        scriptBuilder.Append(" WHEN NOT MATCHED THEN INSERT ([Name], [Description], [CreatedBy], [CreatedAt]) VALUES ([Source].[Name], [Source].[Description], @InitialUserId, GETUTCDATE());");

        return scriptBuilder.ToString();
    }
}
