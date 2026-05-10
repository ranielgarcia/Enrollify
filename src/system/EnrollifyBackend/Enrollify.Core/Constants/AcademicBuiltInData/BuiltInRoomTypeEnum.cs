using Ardalis.SmartEnum;

namespace Enrollify.Core.Constants.AcademicBuiltInData;

public sealed class BuiltInRoomTypeEnum : SmartEnum<BuiltInRoomTypeEnum>
{
    public string Description { get; set; }

    public static readonly BuiltInRoomTypeEnum RoomTypePlaceholder =
        new BuiltInRoomTypeEnum("Placeholder Room Type - Do not delete", "Placeholder room type used for reserved slots.", 1);

    public BuiltInRoomTypeEnum(string name, string description, int id) : base(name, id)
    {
        Description = description;
    }

}
