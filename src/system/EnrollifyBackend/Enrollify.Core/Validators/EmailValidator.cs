using System.Net.Mail;

namespace Enrollify.Core.Validators;

public static class EmailValidator
{
    public static bool IsValid(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        return MailAddress.TryCreate(email, out _);
    }
}
