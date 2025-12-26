namespace Enrollify.Application;

public class BaseDTO
{
    public DateTimeOffset CreatedAt { get; set; }
    public BaseUserDTO? CreatedBy { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public BaseUserDTO? UpdatedBy { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public BaseUserDTO? DeletedBy { get; set; }
    public bool IsActive { get; set; }

}
