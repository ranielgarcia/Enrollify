using Vogen;

[assembly: VogenDefaults(
        staticAbstractsGeneration: StaticAbstractsGeneration.MostCommon | StaticAbstractsGeneration.InstanceMethodsAndProperties)]

namespace Enrollify.Core.Aggregates.RoomTypeAggregate;

[ValueObject<int>]
public partial struct RoomTypeId
{
}
