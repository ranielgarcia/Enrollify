using Bogus;
using Enrollify.Core.Aggregates.CollegeAggregate;
using Enrollify.Core.Aggregates.RoomTypeAggregate;

namespace Enrollify.IntegrationTests.Helpers;

/// <summary>
/// Helper class for creating test entities using Bogus for realistic fake data.
/// NOTE: Only simple entities are included here. Complex entities (Teacher, Subject, Room, etc.)
/// require multiple dependencies and should be created manually in tests or expanded here as needed.
/// </summary>
public static class TestDataBuilder
{
    private static readonly Faker _faker = new();

    // Shared atomic counters — all test classes across the Application collection share one DB,
    // so all counters here must be shared to avoid duplicate-key violations.

    /// <summary>
    /// Provides a globally unique academic year start year (far-future, post-2100).
    /// Increment once per AcademicYear insertion to avoid UQ_AcademicYears_StartEnd violations.
    /// </summary>
    private static int _ayYearCounter = 2100;
    public static int NextAyStartYear() => System.Threading.Interlocked.Increment(ref _ayYearCounter);

    /// <summary>
    /// Provides a globally unique Philippine mobile number for Teacher inserts.
    /// All generated numbers use the Smart 0912-xxxxxxx range and pass libphonenumber-csharp.
    /// </summary>
    private static int _phoneCounter = 0;
    public static string NextPhoneNumber()
    {
        int suffix = System.Threading.Interlocked.Increment(ref _phoneCounter);
        // Subscriber digits: 1000001, 1000002, … — always 7 digits within the 0912-xxxxxxx range.
        return $"0912{(1000000 + suffix):D7}";
    }

    /// <summary>
    /// Creates a test RoomType entity.
    /// </summary>
    public static RoomType CreateRoomType(string? name = null, string? description = null)
    {
        return new RoomType(
            name ?? _faker.Lorem.Word(),
            description ?? _faker.Lorem.Sentence()
        );
    }

    /// <summary>
    /// Creates a test College entity.
    /// </summary>
    public static College CreateCollege(string? code = null, string? name = null, string? description = null, string? dean = null)
    {
        return new College(
            CollegeCode.From(code ?? _faker.Random.AlphaNumeric(3).ToUpper()),
            name ?? _faker.Commerce.Department(),
            description ?? _faker.Lorem.Sentence(),
            dean ?? _faker.Name.FullName()
        );
    }

    /// <summary>
    /// Generates a random unique string for testing.
    /// </summary>
    public static string GenerateUniqueString(string prefix = "test")
    {
        return $"{prefix}_{Guid.NewGuid():N}";
    }

    /// <summary>
    /// Generates a random email address.
    /// </summary>
    public static string GenerateEmail()
    {
        return _faker.Internet.Email();
    }

    /// <summary>
    /// Generates a random decimal within a range.
    /// </summary>
    public static decimal GenerateDecimal(decimal min = 1, decimal max = 100)
    {
        return _faker.Random.Decimal(min, max);
    }

    /// <summary>
    /// Generates a random integer within a range.
    /// </summary>
    public static int GenerateInt(int min = 1, int max = 100)
    {
        return _faker.Random.Number(min, max);
    }

    // Note: Additional entity builders for Building, Room, Department, Course, Subject, Teacher
    // can be added here as needed for specific tests. Those entities have complex constructors
    // with multiple dependencies and value objects that are better created in context of actual tests.
}
