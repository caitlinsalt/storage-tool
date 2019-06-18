using Microsoft.Azure.Storage.Blob;
using StorageTool.Lib.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace StorageTool.Lib.AzureBlob
{
    public class AzureStorageBlobContainer : AzureStorageBlobFolder, IFolder
    {
        private readonly CloudBlobContainer _underlyingContainer;

        public string FullAddress => _underlyingContainer.Uri.ToString();

        public string ShortName => _underlyingContainer.Name;

        public AzureStorageBlobContainer(CloudBlobContainer container)
        {
            _underlyingContainer = container;
        }

        public static AzureStorageBlobContainer GetContainer(CloudBlobClient client, string name)
        {
            CloudBlobContainer container = client.GetContainerReference(name);
            container.CreateIfNotExists();
            return new AzureStorageBlobContainer(container);
        }

        public IFolder CreateSubfolder(string name)
        {
            CloudBlobDirectory dir = _underlyingContainer.GetDirectoryReference(name);
            return new AzureStorageBlobDirectory(dir);
        }

        public async Task<IDataObject> StoreItemAsync(IDataObject source, string name, IUserFeedback feedback, SemaphoreSlim semaphore, OneWaySynchronisationOptions options = OneWaySynchronisationOptions.All)
        {
            CloudBlockBlob blobRef = _underlyingContainer.GetBlockBlobReference(name);
            return await StoreItemAsync(source, blobRef, feedback, semaphore, options).ConfigureAwait(false);
        }

        public async Task DeleteFolderAsync(IUserFeedback feedback)
        {
            feedback.FolderDeleteStarted(FullAddress);
            IList<IFolderMember> members = await GetMembersAsync();
            await DeleteContentsAsync(members, feedback).ConfigureAwait(false);
            feedback.FolderDeleteFinished(FullAddress);
        }

        public async Task<IList<IFolderMember>> GetMembersAsync()
        {
            BlobContinuationToken token = null;
            List<IFolderMember> results = new List<IFolderMember>();
            do
            {
                BlobResultSegment answer = await _underlyingContainer.ListBlobsSegmentedAsync(token).ConfigureAwait(false);
                results.AddRange(GetMemberList(answer.Results));
                token = answer.ContinuationToken;
            } while (token != null);
            return results;
        }

        public async Task<int> GetItemCountAsync()
        {
            BlobContinuationToken token = null;
            int count = 0;
            do
            {
                BlobResultSegment answer = await _underlyingContainer.ListBlobsSegmentedAsync("", true, BlobListingDetails.All, null, token, null, null);
                count += answer.Results.Count();
                token = answer.ContinuationToken;
            } while (token != null);
            return count;
        }
    }
}
