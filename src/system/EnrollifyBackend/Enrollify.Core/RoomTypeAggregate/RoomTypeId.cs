using Vogen;

[assembly: VogenDefaults(
        staticAbstractsGeneration: StaticAbstractsGeneration.MostCommon | StaticAbstractsGeneration.InstanceMethodsAndProperties)]

namespace Enrollify.Core.RoomTypeAggregate;

[ValueObject<int>]
public partial struct RoomTypeId
{
    private static Validation Validate(int value)
        => value > 0 ? Validation.Ok : Validation.Invalid("RoomTypeId must be positive.");
}
