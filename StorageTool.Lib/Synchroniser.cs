using StorageTool.Lib.Extensions;
using StorageTool.Lib.Interfaces;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace StorageTool.Lib
{
    public class Synchroniser
    {
        public async Task SyncAsync(IFolder source, IFolder dest, IUserFeedback feedback, SynchronisationOptions options, SemaphoreSlim semaphore)
        {
            OneWaySynchronisationOptions upOptions = options.ToOneWaySynchronisationOptions(SynchronisationDirection.Up);
            OneWaySynchronisationOptions downOptions = options.ToOneWaySynchronisationOptions(SynchronisationDirection.Down);
            if (upOptions != 0)
            {
                int objectCount = await source.GetItemCountAsync().ConfigureAwait(false);
                feedback.Init(objectCount, "Uploading...");
                await PushAsync(source, dest, feedback, upOptions, semaphore).ConfigureAwait(false);
            }
            if (downOptions != 0)
            {
                int objectCount = await dest.GetItemCountAsync().ConfigureAwait(false);
                feedback.Init(objectCount, "Downloading...");
                await PullAsync(source, dest, feedback, downOptions, semaphore).ConfigureAwait(false);
            }
        }

        private async Task PushAsync(IFolder source, IFolder dest, IUserFeedback feedback, OneWaySynchronisationOptions options, SemaphoreSlim semaphore)
        {
            ConcurrentDictionary<string, IDataObject> uploadedObjects = new ConcurrentDictionary<string, IDataObject>();
            
            if ((options & (OneWaySynchronisationOptions.New | OneWaySynchronisationOptions.Existing | OneWaySynchronisationOptions.ForceExisting)) != 0)
            {
                feedback.FolderUploadStarted(dest.FullAddress);

                IList<IFolderMember> sourceMembers = new IFolderMember[0];
                await semaphore.WaitAsync();
                try
                {
                    sourceMembers = await source.GetMembersAsync();
                }
                finally
                {
                    semaphore.Release();
                }

                var taskList = sourceMembers.Select(async member =>
                {
                    if (member is IDataObject memberObject)
                    {
                        IDataObject result = await dest.StoreItemAsync(memberObject, memberObject.ShortName, feedback, semaphore, options);
                        if (result != null && !uploadedObjects.ContainsKey(result.FullAddress))
                        {
                            uploadedObjects.TryAdd(result.FullAddress, result);
                        }
                    }
                    if (member is IFolder folderObject)
                    {
                        IFolder destSubfolder = dest.CreateSubfolder(member.ShortName);
                        await PushAsync(folderObject, destSubfolder, feedback, options, semaphore);
                    }
                });
                await Task.WhenAll(taskList);
                feedback.FolderUploadFinished(dest.FullAddress);
            }
            if ((options & OneWaySynchronisationOptions.Deletions) != 0)
            {
                Task<IList<IFolderMember>> sourceMembersTask = source.GetMembersAsync();
                Task<IList<IFolderMember>> destMembersTask = dest.GetMembersAsync();
                await Task.WhenAll(sourceMembersTask, destMembersTask);
                IList<IFolderMember> sourceMembers = sourceMembersTask.Result;
                IList<IFolderMember> destMembers = destMembersTask.Result;
                IEnumerable<Task> taskList = destMembers.Select(async member =>
                {
                    if (member is IDataObject memberObject)
                    {
                        if ((!uploadedObjects.ContainsKey(memberObject.FullAddress)) && (!sourceMembers.Any(m => m is IDataObject && m.ShortName == memberObject.ShortName)))
                        {
                            await memberObject.DeleteAsync(feedback);
                        }
                    }
                    if (member is IFolder folderObject)
                    {
                        if (!sourceMembers.Any(m => m is IFolder && m.ShortName == folderObject.ShortName))
                        {
                            await folderObject.DeleteFolderAsync(feedback);
                        }
                    }
                });
                await Task.WhenAll(taskList);
            }
        }

        private async Task PullAsync(IFolder source, IFolder dest, IUserFeedback feedback, OneWaySynchronisationOptions options, SemaphoreSlim semaphore)
        {
            await PushAsync(dest, source, feedback, options, semaphore);
        }
    }
}
