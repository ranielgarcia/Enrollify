using Vogen;

[assembly: VogenDefaults(
        staticAbstractsGeneration: StaticAbstractsGeneration.MostCommon | StaticAbstractsGeneration.InstanceMethodsAndProperties)]

namespace Enrollify.Core.RoomTypeAggregate;

[ValueObject<int>]
public partial struct RoomTypeId
{
}
