using Vogen;

namespace Enrollify.Core.RoomAggregate;

[ValueObject<int>]
public partial struct RoomId
{
    private static Validation Validate(int value)
        => value > 0 ? Validation.Ok : Validation.Invalid("RoomId must be positive.");
}
