using Ardalis.SmartEnum;

namespace Enrollify.Core.Constants.Authorization;

public sealed class PermissionEnum : SmartFlagEnum<PermissionEnum>
{
    public static readonly PermissionEnum None =
        new(nameof(None), 0);

    public static readonly PermissionEnum View =
        new(nameof(View), 1 << 0); // 1   (binary 0001)

    public static readonly PermissionEnum Create =
        new(nameof(Create), 1 << 1); // 2   (binary 0010)

    public static readonly PermissionEnum Update =
        new(nameof(Update), 1 << 2); // 4   (binary 0100)

    public static readonly PermissionEnum Delete =
        new(nameof(Delete), 1 << 3); // 8   (binary 1000)

    public static readonly PermissionEnum Full =
        new(nameof(Full), View | Create | Update | Delete);
    private PermissionEnum(string name, int value) : base(name, value) { }
}
