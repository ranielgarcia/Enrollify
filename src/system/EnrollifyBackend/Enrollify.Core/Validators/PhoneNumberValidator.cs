using Enrollify.Core.Constants;
using PhoneNumbers;

namespace Enrollify.Core.Validators;

public static class PhoneNumberValidator
{
    public static bool IsValid (string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return false;

        // https://github.com/twcclegg/libphonenumber-csharp?tab=readme-ov-file#check-if-a-phone-number-is-valid
        // https://www.twilio.com/en-us/blog/validating-phone-numbers-effectively-with-c-and-the-net-frameworks#Phone-number-validation-in-strongSystemComponentDataAnnotationsstrong
        try
        {
            var phoneNumberUtil = PhoneNumberUtil.GetInstance();
            var parsedPhoneNumber = phoneNumberUtil.Parse(phoneNumber, RegionConstants.PhoneNumberValidatorRegion);
            var isValid = phoneNumberUtil.IsValidNumber(parsedPhoneNumber);
            return isValid;
        }
        catch (NumberParseException)
        {
            return false;
        }
    }
}
