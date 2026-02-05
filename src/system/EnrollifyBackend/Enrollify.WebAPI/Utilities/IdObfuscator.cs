using Sqids;

namespace Enrollify.WebAPI.Utilities;

public static class IdObfuscatorExtensions
{
    private static readonly SqidsEncoder<int> _IdsEncoder = new SqidsEncoder<int>(
        new SqidsOptions
        {
            MinLength = 10,
            Alphabet = "WBIv7N6ujRwdiHyaQoA4gEMsOmLeJ1z8xh0GrZPDcFk5fVtTSUCq9YnK23Xblp"
        });

    public static string ToObfuscatedId(this int id)
    {
        return _IdsEncoder.Encode(id);
    }

    public static int FromObfuscatedId(this string encodedId)
    {
        if (_IdsEncoder.Decode(encodedId) is [var decodedId] &&
            decodedId >= 0 &&
            encodedId == _IdsEncoder.Encode(decodedId))
        {
            return decodedId;
        }

        return 0;
    }
}

