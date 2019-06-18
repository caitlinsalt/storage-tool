using System;

namespace StorageTool.Lib.Extensions
{
    public static class SynchronisationOptionsExtensions
    {
        public static OneWaySynchronisationOptions ToOneWaySynchronisationOptions(this SynchronisationOptions options, SynchronisationDirection direction)
        {
            switch (direction)
            {
                case SynchronisationDirection.Up:
                    return (OneWaySynchronisationOptions)(options & SynchronisationOptions.UploadAll);
                case SynchronisationDirection.Down:
                    return (OneWaySynchronisationOptions)((int)options / 16);
                default:
                    throw new InvalidOperationException("Cannot convert to SynchronisationOptions to OneWaySynchronisationOptions if both directions are selected.");
            }
        }
    }
}
