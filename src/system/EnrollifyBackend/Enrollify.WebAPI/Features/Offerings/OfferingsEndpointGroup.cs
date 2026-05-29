namespace Enrollify.WebAPI.Features.Offerings;

public class OfferingsEndpointGroup : Group
{
    public OfferingsEndpointGroup()
    {
        Configure("offerings", ep =>
        {
            ep.Description(x => x.Produces(401));
        });
    }
}
