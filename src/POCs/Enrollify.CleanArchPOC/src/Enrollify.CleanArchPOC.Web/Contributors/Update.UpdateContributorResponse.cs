namespace Enrollify.CleanArchPOC.Web.Contributors;

public class UpdateContributorResponse(ContributorRecord contributor)
{
  public ContributorRecord Contributor { get; set; } = contributor;
}
