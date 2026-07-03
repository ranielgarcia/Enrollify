namespace Enrollify.Application.Features.RoomTypes.Specifications;

public class ListRoomTypesWithAllNavigationSpec : Specification<RoomType>
{
    public ListRoomTypesWithAllNavigationSpec() =>
        Query.Include(r => r.CreatedByUser)
            .Include(r => r.UpdatedByUser);
}
