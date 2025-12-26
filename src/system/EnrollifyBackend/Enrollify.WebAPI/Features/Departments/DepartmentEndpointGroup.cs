namespace Enrollify.WebAPI.Features.Departments;

public class DepartmentEndpointGroup : Group
{
    public DepartmentEndpointGroup()
    {
        Configure("departments", ep =>
        {
            ep.Description(x => x.Produces(401));
        });
    }
}
