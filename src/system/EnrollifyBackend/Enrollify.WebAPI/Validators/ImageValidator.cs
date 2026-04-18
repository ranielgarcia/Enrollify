using System.Collections.Frozen;
using Enrollify.Core.Aggregates.RoleAggregate;

namespace Enrollify.WebAPI.Validators;

public static class ImageValidator
{
    private const long MaxFileSizeInBytes = 5 * 1024 * 1024; // 5 MB

    private static readonly FrozenSet<string> AllowedExtensions = FrozenSet.ToFrozenSet(
        [".png", ".jpeg", ".jpg"],
        StringComparer.OrdinalIgnoreCase);

    private static readonly FrozenSet<string> AllowedContentTypes = FrozenSet.ToFrozenSet(
        ["image/png", "image/jpeg"],
        StringComparer.OrdinalIgnoreCase);

    private static readonly FrozenDictionary<string, byte[]> MagicBytes =
        new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase)
        {
            { ".jpg",  [0xFF, 0xD8, 0xFF] },
            { ".jpeg", [0xFF, 0xD8, 0xFF] },
            { ".png",  [0x89, 0x50, 0x4E, 0x47] },
        }.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

    public static (bool ErrorOccurred, string ErrorMessage) ValidateFile(IFormFile file)
    {
        if (file is null)
        {
            return (ErrorOccurred: true, ErrorMessage: "No file added");
        }

        var extension = Path.GetExtension(file.FileName);

        if (!AllowedExtensions.Contains(extension))
        {
            return (ErrorOccurred: true, ErrorMessage: "File is not an image");
        }

        if (!AllowedContentTypes.Contains(file.ContentType))
        {
            return (ErrorOccurred: true, ErrorMessage: "File content type is not allowed");
        }

        if (file.Length < 1)
        {
            return (ErrorOccurred: true, ErrorMessage: "File has no content");
        }

        if (file.Length > MaxFileSizeInBytes)
        {
            return (ErrorOccurred: true, ErrorMessage: "File size exceeds the 5 MB limit");
        }

        if (!HasValidMagicBytes(file, extension))
        {
            return (ErrorOccurred: true, ErrorMessage: "File content does not match a valid image format");
        }

        return (ErrorOccurred: false, ErrorMessage: string.Empty);
    }

    private static bool HasValidMagicBytes(IFormFile file, string extension)
    {
        // Magic bytes (aka file signatures) are the first few bytes of a file that identify its format.
        // They're hardcoded by the file format spec — not the OS or file system.
        // Format       Magic Bytes (hex)       ASCII
        // PNG          89 50 4E 47             ‰PNG
        // JPEG         FF D8 FF                ÿØÿ
        // PDF          25 50 44 46             %PDF
        // ZIP          50 4B 03 04             PK..
        //A file extension is just part of the filename — anyone can rename malware.exe to photo.jpg.
        // The magic bytes are in the actual binary content, so they're much harder to fake accidentally,
        // and immediately reveal what the file truly is.

        if (!MagicBytes.TryGetValue(extension, out var magic))
            return false;

        Span<byte> buffer = stackalloc byte[magic.Length];

        // Extension says .jpg — but what does the file actually say?
        // FF D8 FF = real JPEG
        // 4D 5A 90 00 = that's an EXE header (MZ header)
        using var stream = file.OpenReadStream();
        stream.ReadExactly(buffer);
        return buffer.SequenceEqual(magic);
    }
}
