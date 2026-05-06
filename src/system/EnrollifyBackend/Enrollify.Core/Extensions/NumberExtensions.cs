namespace Enrollify.Core.Extensions;

public static class NumberExtensions
{
    public static string ToOrdinal(this int number)
    {
        if (number <= 0)
            return number.ToString();

        int lastTwoDigits = number % 100;

        // Special cases: 11th, 12th, 13th
        if (lastTwoDigits >= 11 && lastTwoDigits <= 13)
        {
            return $"{number}th";
        }

        return (number % 10) switch
        {
            1 => $"{number}st",
            2 => $"{number}nd",
            3 => $"{number}rd",
            _ => $"{number}th"
        };
    }
}
