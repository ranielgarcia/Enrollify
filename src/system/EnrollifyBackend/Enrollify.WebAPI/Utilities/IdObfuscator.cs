using Sqids;

namespace Enrollify.WebAPI.Utilities;

public interface IIdObfuscator
{
    int Decode(string encodedId);
    string Encode(int id);
}

public class IdObfuscator : IIdObfuscator
{
    public IdObfuscator(ILogger<IdObfuscator> logger)
    {
        _logger = logger;
    }

    private static readonly SqidsEncoder<int> _IdsEncoder = new SqidsEncoder<int>(
        new SqidsOptions
        {
            MinLength = 10,
            Alphabet = "WBIv7N6ujRwdiHyaQoA4gEMsOmLeJ1z8xh0GrZPDcFk5fVtTSUCq9YnK23Xblp"
        });
    private readonly ILogger<IdObfuscator> _logger;

    public string Encode(int id)
    {
        return _IdsEncoder.Encode(id);
    }

    public int Decode(string encodedId)
    {
        if (_IdsEncoder.Decode(encodedId) is [var decodedId] &&
            encodedId == _IdsEncoder.Encode(decodedId))
        {
            return decodedId;
        }

        _logger.LogError("Invalid encoded id {encodedId}", encodedId);

        return 0;
    }
}

