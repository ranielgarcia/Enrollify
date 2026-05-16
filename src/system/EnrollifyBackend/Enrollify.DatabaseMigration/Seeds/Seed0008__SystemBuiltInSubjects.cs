using DbUp.Engine;
using System.Data;
using System.Text;

namespace Enrollify.DatabaseMigration.Seeds;

internal class Seed0008__SystemBuiltInSubjects : IScript
{
    public string ProvideScript(Func<IDbCommand> dbCommandFactory)
    {
        var builtInSubjects = Core.Constants.AcademicBuiltInData.BuiltInSubjectsEnum.List;

        var scriptBuilder = new StringBuilder();

        // Get system user ID
        scriptBuilder.Append("DECLARE @InitialUserId INT = (SELECT Id FROM Users WHERE Email='system@enrollify.local');");

        var getRoomTypeCommand = dbCommandFactory();
        getRoomTypeCommand.CommandText = "SELECT Id FROM RoomTypes WHERE Name='Placeholder Room Type - Do not delete'";
        var roomTypeId = (int)getRoomTypeCommand.ExecuteScalar();

        scriptBuilder.Append(@"MERGE [Subjects] AS [Target]
            USING (");
        scriptBuilder.Append(@"VALUES");

        var values = new List<string>();

        foreach (var subject in builtInSubjects)
        {
            var descriptionValue = !string.IsNullOrEmpty(subject.Description) 
                ? $"'{subject.Description.Replace("'", "''")}'" 
                : "NULL";
            var unitsValue = subject.Units.HasValue ? subject.Units.Value.ToString("0.0") : "NULL";

            values.Add(string.Format("('{0}', '{1}', {2}, {3}, {4})",
                subject.Code,
                subject.Name.Replace("'", "''"),
                unitsValue,
                descriptionValue,
                roomTypeId));
        }

        scriptBuilder.Append(string.Join(", ", values.ToArray()));
        scriptBuilder.Append(") AS [Source] ([Code], [Title], [Units], [Description], [PreferRoomTypeId])");
        scriptBuilder.Append(" ON [Target].[Code] = [Source].[Code]");
        scriptBuilder.Append(" WHEN MATCHED THEN UPDATE SET [Target].[Title] = [Source].[Title], [Target].[Units] = [Source].[Units], [Target].[Description] = [Source].[Description], [Target].[PreferRoomTypeId] = [Source].[PreferRoomTypeId], [Target].[UpdatedBy] = @InitialUserId, [Target].[UpdatedAt] = GETUTCDATE()");
        scriptBuilder.Append(" WHEN NOT MATCHED THEN INSERT ([Code], [Title], [Units], [Description], [PreferRoomTypeId], [CreatedBy], [CreatedAt]) VALUES ([Source].[Code], [Source].[Title], [Source].[Units], [Source].[Description], [Source].[PreferRoomTypeId], @InitialUserId, GETUTCDATE());");

        return scriptBuilder.ToString();
    }
}
