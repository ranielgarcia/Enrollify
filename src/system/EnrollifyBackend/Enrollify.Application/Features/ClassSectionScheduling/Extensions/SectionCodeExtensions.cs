namespace Enrollify.Application.Features.ClassSectionScheduling.Extensions;

public static class SectionCodeExtensions
{
    public static SectionCode GetNextSectionCode(this SectionCode? lastClassSectionCode)
    {
        int letterA = 65;
        int nextSectionCodeASCII = lastClassSectionCode != null ? ((int)lastClassSectionCode.Value) + 1 : letterA;
        return SectionCode.From((char)nextSectionCodeASCII);
    }
}
