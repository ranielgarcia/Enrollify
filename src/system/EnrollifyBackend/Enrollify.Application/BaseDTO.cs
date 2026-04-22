namespace Enrollify.Application;

public class BaseDto
{
    public DateTimeOffset CreatedAt { get; set; }
    public BaseUserDto? CreatedBy { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public BaseUserDto? UpdatedBy { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public BaseUserDto? DeletedBy { get; set; }
    public bool IsActive { get; set; }

}
