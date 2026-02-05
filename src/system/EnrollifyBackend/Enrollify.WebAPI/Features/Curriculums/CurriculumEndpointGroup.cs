namespace Enrollify.WebAPI.Features.Curriculums;

public class CurriculumEndpointGroup : Group
{
    public CurriculumEndpointGroup()
    {
        Configure("curriculums", ep =>
        {
            ep.Description(x => x.Produces(401));
        });
    }
}
