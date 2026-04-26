namespace Enrollify.Infrastructure.Storage.Blob.Constants;

public static class BlobConstants
{
    public static class BlockSize
    {
        /// <summary>Minimum block size in MB.</summary>
        public const int Min = 1;

        /// <summary>Maximum block size in MB (Azure limit is 4000 MB for block blobs).</summary>
        public const int Max = 256;
    }

    public static class Concurrent
    {
        public const int Min = 1;
        public const int Max = 64;
    }
}
