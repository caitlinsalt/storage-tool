using System;

namespace StorageTool.Lib
{
    [Flags]
    public enum SynchronisationOptions
    {
        UploadNew = 1,
        UploadExisting = 2,
        UploadDeletions = 4,
        UploadForceExisting = 8,
        UploadAll = 15,
        DownloadNew = 16,
        DownloadExisting = 32,
        DownloadDeletions = 64,
        DownloadForceExisting = 128,
        DownloadAll = 240,
        Synchronise = 119
    }
}
