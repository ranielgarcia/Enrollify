namespace Enrollify.WebAPI.Features.ClassSectionScheduling;

public class ClassSectionEndpointSubGroup : SubGroup<ClassSectionSchedulingEndpointGroup>
{
  public ClassSectionEndpointSubGroup()
  {
    Configure("class-sections", ep => ep.Tags("Class Sections"));
  }
}
