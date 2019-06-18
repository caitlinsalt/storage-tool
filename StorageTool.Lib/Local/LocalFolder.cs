using StorageTool.Lib.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace StorageTool.Lib.Local
{
    public class LocalFolder : IFolder
    {
        public string ShortName => Path.GetFileName(FullAddress);

        public string FullAddress { get; }

        public LocalFolder(string path)
        {
            FullAddress = path;
        }

        public IFolder CreateSubfolder(string name)
        {
            string path = Path.Combine(FullAddress, name);
            Directory.CreateDirectory(path);
            return new LocalFolder(path);
        }

        public async Task<IDataObject> StoreItemAsync(IDataObject source, string name, IUserFeedback feedback, SemaphoreSlim semaphore, OneWaySynchronisationOptions options)
        {
            string outputPath = Path.Combine(FullAddress, name);
            FileMode openMode;
            if ((options & OneWaySynchronisationOptions.New) != 0 && (options & (OneWaySynchronisationOptions.Existing | OneWaySynchronisationOptions.ForceExisting)) != 0)
            {
                openMode = FileMode.Create;
            }
            else if ((options & OneWaySynchronisationOptions.New) != 0)
            {
                openMode = FileMode.CreateNew;
            }
            else if ((options & (OneWaySynchronisationOptions.Existing | OneWaySynchronisationOptions.ForceExisting)) != 0)
            {
                openMode = FileMode.Truncate;
            }
            else
            {
                feedback.ObjectUploadSkipped(outputPath);
                return null;
            }

            await semaphore.WaitAsync();
            try
            {
                if (File.Exists(outputPath))
                {
                    LocalFile existingLocal = new LocalFile(outputPath);
                    if (existingLocal.HashEquals(source) && existingLocal.Size == source.Size)
                    {
                        feedback.ObjectUploadSkipped(outputPath);
                        return existingLocal;
                    }
                    if ((options & OneWaySynchronisationOptions.ForceExisting) == 0 && existingLocal.UpdateTimestamp > source.UpdateTimestamp)
                    {
                        feedback.ObjectUploadSkipped(outputPath);
                        return existingLocal;
                    }
                }

                feedback.ObjectUploadStarted(outputPath);
                try
                {
                    using (Stream outputStream = File.Open(outputPath, openMode))
                    using (Stream sourceStream = await source.GetReadStreamAsync().ConfigureAwait(false))
                    {
                        await sourceStream.CopyToAsync(outputStream).ConfigureAwait(false);
                    }
                    feedback.ObjectUploadFinished(outputPath);
                    return new LocalFile(outputPath);
                }
                catch (Exception ex)
                {
                    feedback.Error(outputPath, ex.Message);
                    feedback.ObjectUploadFinished(outputPath, true);
                    return null;
                }
            }
            finally
            {
                semaphore.Release();
            }
        }

        public async Task DeleteFolderAsync(IUserFeedback feedback)
        {
            await Task.Run(() => Directory.Delete(FullAddress, true)).ConfigureAwait(false);
        }

        public async Task<IList<IFolderMember>> GetMembersAsync()
        {
            try
            {
                DirectoryInfo dirInfo = new DirectoryInfo(FullAddress);
                ConcurrentBag<IFolderMember> theItems = new ConcurrentBag<IFolderMember>();
                Task dirTask = Task.Run(() =>
                {
                    try
                    {
                        foreach (DirectoryInfo subdir in dirInfo.GetDirectories())
                        {
                            theItems.Add(new LocalFolder(subdir.FullName));
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                });
                Task fileTask = Task.Run(() =>
                {
                    try
                    {
                        foreach (FileInfo fileInfo in dirInfo.GetFiles())
                        {
                            theItems.Add(new LocalFile(fileInfo.FullName));
                        }
                    }
                    catch (Exception)
                    {

                    }
                });
                await Task.WhenAll(dirTask, fileTask);
                return theItems.ToArray();
            }
            catch (Exception)
            {
                return new IFolderMember[0];
            }
        }

        public async Task<int> GetItemCountAsync()
        {
            DirectoryInfo dirInfo = new DirectoryInfo(FullAddress);
            Task<int> fileTask = Task.Run(() =>
            {
                return dirInfo.GetFiles().Length;
            });
            var subDirTaskList = dirInfo.GetDirectories().Select(async d => await new LocalFolder(d.FullName).GetItemCountAsync().ConfigureAwait(false)).ToList();
            await Task.WhenAll(subDirTaskList).ConfigureAwait(false);
            await fileTask.ConfigureAwait(false);
            return fileTask.Result + subDirTaskList.Sum(t => t.Result);
        }
    }
}
