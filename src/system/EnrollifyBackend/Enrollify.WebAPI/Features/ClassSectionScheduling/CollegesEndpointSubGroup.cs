namespace Enrollify.WebAPI.Features.ClassSectionScheduling;

public class CollegesEndpointSubGroup : SubGroup<ClassSectionSchedulingEndpointGroup>
{
  public CollegesEndpointSubGroup()
  {
    Configure("colleges", ep => ep.Tags("Colleges"));
  }
}
