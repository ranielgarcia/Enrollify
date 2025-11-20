using System.Security.Cryptography;
using System.Text;

namespace Enrollify.WebAPI.Infrastructure.Exceptions;

//https://medium.com/@AntonAntonov88/handling-errors-with-iexceptionhandler-in-asp-net-core-8-0-48c71654cc2e
public static class ExceptionExtensions
{
    public const string ErrorCodeKey = "errorCode";

    public static Exception AddErrorCode(this Exception exception)
    {
        using var sha1 = SHA1.Create();
        var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(exception.Message));
        var errorCode = string.Concat(hash[..5].Select(b => b.ToString("x")));
        exception.Data[ErrorCodeKey] = errorCode;
        return exception;
    }

    public static string GetErrorCode(this Exception exception)
    {
        return (string)(exception.Data[ErrorCodeKey] ?? "");
    }

    public static Exception DetailData(this Exception exception, in string key, in object value)
    {
        try
        {
            exception.Data[key] = ExceptionDataEntry.FromValue(value);
        }
        catch
        {
            // ignored, because we use it inside another exception catch block
            // so, we should avoid throwing a new exception to keep the original exception
        }

        return exception;
    }
}
