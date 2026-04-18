namespace Enrollify.WebAPI.Utilities;

public static class FileUploadUtility
{
    public static async Task<byte[]> GetFileContentAsync(IFormFile file)
    {
        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        return ms.ToArray();
    }
}
