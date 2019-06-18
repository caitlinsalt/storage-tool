using System;

namespace StorageTool.Lib
{
    [Flags]
    public enum OneWaySynchronisationOptions
    {
        New = 1,
        Existing = 2,
        Deletions = 4,
        ForceExisting = 8,
        All = 15
    }
}
