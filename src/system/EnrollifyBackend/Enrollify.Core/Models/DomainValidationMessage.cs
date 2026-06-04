using Enrollify.Core.Constants;

namespace Enrollify.Core.Models;

public sealed record DomainValidationMessage(DomainValidationErrorSeverityEnum Severity, string Code, string Message);
