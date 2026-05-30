namespace Enrollify.WebAPI.Features.SubjectOfferings;

public class SubjectOfferingsEndpointGroup : Group
{
    public SubjectOfferingsEndpointGroup()
    {
        Configure("subject-offerings", ep =>
        {
            ep.Description(x => x.Produces(401));
        });
    }
}
