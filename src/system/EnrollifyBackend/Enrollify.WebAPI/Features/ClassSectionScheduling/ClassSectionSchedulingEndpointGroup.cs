namespace Enrollify.WebAPI.Features.ClassSectionScheduling;

public class ClassSectionSchedulingEndpointGroup : Group
{
  public ClassSectionSchedulingEndpointGroup()
  {
    Configure("scheduling", ep => { ep.Description(x => x.Produces(401)); });
  }
}
