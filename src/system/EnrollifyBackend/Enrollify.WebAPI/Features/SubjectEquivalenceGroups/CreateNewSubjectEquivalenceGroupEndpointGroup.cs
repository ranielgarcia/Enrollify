namespace Enrollify.WebAPI.Features.SubjectEquivalenceGroups;

public class CreateNewSubjectEquivalenceGroupEndpointGroup : Group
{
    public CreateNewSubjectEquivalenceGroupEndpointGroup()
    {
        Configure("subject-equivalence-groups", ep =>
        {
            ep.Description(x => x.Produces(401));
        });
    }
}
