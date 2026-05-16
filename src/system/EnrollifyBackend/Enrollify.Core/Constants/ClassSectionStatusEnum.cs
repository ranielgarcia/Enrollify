using Ardalis.SmartEnum;

namespace Enrollify.Core.Constants;

public sealed class ClassSectionStatusEnum : SmartEnum<ClassSectionStatusEnum>
{
    public static readonly ClassSectionStatusEnum Draft =
        new ClassSectionStatusEnum("Draft", 1, "Newly created; subject offerings, schedules, or room assignments are still incomplete");

    public static readonly ClassSectionStatusEnum Open =
        new ClassSectionStatusEnum("Open", 2, "Fully configured and accepting student enrollments");

    public static readonly ClassSectionStatusEnum Locked =
        new ClassSectionStatusEnum("Locked", 3, "Enrollment period has ended; no new enrollments accepted but term hasn't started yet");

    public static readonly ClassSectionStatusEnum Active =
        new ClassSectionStatusEnum("Active", 4, "Academic term is in progress; students are attending classes");

    public static readonly ClassSectionStatusEnum Completed =
        new ClassSectionStatusEnum("Completed", 5, "Academic term has ended; grades are being finalized or already finalized");

    public static readonly ClassSectionStatusEnum Cancelled =
        new ClassSectionStatusEnum("Cancelled", 6, "Section was cancelled (e.g., insufficient enrollment, room unavailable)");

    public string Description { get; set; }

    private ClassSectionStatusEnum(string name, int value, string description) : base(name, value)
    {
        Description = description;
    }
}
