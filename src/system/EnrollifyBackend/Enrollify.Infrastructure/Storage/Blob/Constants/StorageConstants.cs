namespace Enrollify.Infrastructure.Storage.Blob.Constants;

public static class StorageConstants
{
    public static class Blob
    {
        public static class Size
        {
            /// <summary>1 MB in bytes — used to convert block size config (in MB) to bytes.</summary>
            public const long MegaBytes = 1_048_576;
        }
    }
}
