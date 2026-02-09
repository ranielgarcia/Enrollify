namespace Enrollify.WebAPI.Features.SubjectEquivalences;

public class SubjectEquivalenceGroupEndpointGroup : Group
{
    public SubjectEquivalenceGroupEndpointGroup()
    {
        Configure("subject-equivalences-groups", ep =>
        {
            ep.Description(x => x.Produces(401));
        });
    }
}
