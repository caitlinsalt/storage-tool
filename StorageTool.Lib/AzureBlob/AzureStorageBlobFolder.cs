using Microsoft.Azure.Storage.Blob;
using StorageTool.Lib.Interfaces;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace StorageTool.Lib.AzureBlob
{
    /// <summary>
    /// A superclass to contain common code for cloud folder operations
    /// </summary>
    public abstract class AzureStorageBlobFolder
    {
        protected async Task<IDataObject> StoreItemAsync(IDataObject source, CloudBlockBlob dest, IUserFeedback feedback, SemaphoreSlim semaphore, OneWaySynchronisationOptions options)
        {
            await semaphore.WaitAsync();
            try
            {
                AzureStorageBlob asb = new AzureStorageBlob(dest);
                bool alreadyExists = await dest.ExistsAsync();
                // We skip an upload (but return an object as if it was done) in the following conditions:
                // 1) options.Existing is set, options.ForceExisting is not set, the blob exists, and the source is older than the destination.
                // 2) either options.Existing or options.ForceExisting is set, the blob exists, and the blob's hash and size are the same as the local copy
                if (alreadyExists && ((options & (OneWaySynchronisationOptions.Existing | OneWaySynchronisationOptions.ForceExisting)) != 0))
                {
                    if ((((options & OneWaySynchronisationOptions.ForceExisting) == 0) && source.UpdateTimestamp <= asb.UpdateTimestamp) ||
                        (asb.HashEquals(source) && asb.Size == source.Size))
                    {
                        feedback.ObjectUploadSkipped(asb.FullAddress);
                        return asb;
                    }
                }
                // We do an upload under the following conditions:
                // 1) options.New is set and the blob does not exist
                // 2) options.ForceExisting is set, the blob does exist, and we have already checked above that either size or hash (or both) differ
                // 3) options.Existng is set, the blob exists, the source version is newer than the destination version, and we have already checked above that either size or hash (or both) differ
                if (((options & OneWaySynchronisationOptions.New) != 0 && !alreadyExists) || ((options & OneWaySynchronisationOptions.ForceExisting) != 0 && alreadyExists) ||
                    (((options & OneWaySynchronisationOptions.Existing) != 0) && alreadyExists && (!asb.HashEquals(source) || asb.Size != source.Size) && source.UpdateTimestamp > asb.UpdateTimestamp))
                {
                    feedback.ObjectUploadStarted(asb.FullAddress);
                    using (Stream sourceStream = await source.GetReadStreamAsync())
                    {
                        await dest.UploadFromStreamAsync(sourceStream);
                    }
                    dest.Properties.ContentType = source.ContentType;
                    await dest.SetPropertiesAsync();
                    feedback.ObjectUploadFinished(asb.FullAddress);
                    return asb;
                }
                feedback.ObjectUploadSkipped(asb.FullAddress);
                return null;
            }
            finally
            {
                semaphore.Release();
            }
        }

        protected IList<IFolderMember> GetMemberList(IEnumerable<IListBlobItem> items)
        {
            List<IFolderMember> output = new List<IFolderMember>();
            output.AddRange(items
                .Select(b =>
                {
                    if (b is CloudBlobDirectory subDir)
                    {
                        return (IFolderMember)new AzureStorageBlobDirectory(subDir);
                    }
                    if (b is ICloudBlob blob)
                    {
                        return new AzureStorageBlob(blob);
                    }
                    return null;
                })
                .Where(b => b != null));
            return output;
        }

        protected async Task DeleteContentsAsync(IEnumerable<IFolderMember> items, IUserFeedback feedback)
        {
            List<Task> taskList = new List<Task>();
            foreach (IFolderMember item in items)
            {
                if (item is AzureStorageBlobDirectory directory)
                {
                    taskList.Add(directory.DeleteFolderAsync(feedback));
                }
                else if (item is AzureStorageBlob blob)
                {
                    taskList.Add(blob.DeleteAsync(feedback));
                }
            }
            await Task.WhenAll(taskList);
        }
    }
}
