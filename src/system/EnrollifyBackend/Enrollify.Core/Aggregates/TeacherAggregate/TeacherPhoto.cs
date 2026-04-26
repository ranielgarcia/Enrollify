using Enrollify.Core.ValueObjects.Storage;

namespace Enrollify.Core.Aggregates.TeacherAggregate;

public record TeacherPhoto(FileName Filename, string ContentType);
