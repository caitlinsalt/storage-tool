using Microsoft.Azure.Storage.Blob;
using StorageTool.Lib.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace StorageTool.Lib.AzureBlob
{
    public class AzureStorageBlobDirectory : AzureStorageBlobFolder, IFolder
    {
        private readonly CloudBlobDirectory _underlyingDirectory;

        public string FullAddress => _underlyingDirectory.Uri.ToString();

        private string _shortName;

        public string ShortName
        {
            get
            {
                if (_shortName == null)
                {
                    const string separator = "/";
                    int length = _underlyingDirectory.Prefix.Length;
                    int endOffset = 0;
                    if (_underlyingDirectory.Prefix.EndsWith(separator))
                    {
                        endOffset = 1;
                    }
                    int prevSeparator = _underlyingDirectory.Prefix.LastIndexOf(separator, length - (endOffset + 1));
                    _shortName = _underlyingDirectory.Prefix.Substring(prevSeparator + 1, length - (prevSeparator + 1 + endOffset));
                }
                return _shortName;
            }
        }

        public AzureStorageBlobDirectory(CloudBlobDirectory blobDir)
        {
            _underlyingDirectory = blobDir;
        }

        public IFolder CreateSubfolder(string name)
        {
            CloudBlobDirectory dir = _underlyingDirectory.GetDirectoryReference(name);
            var subDir = new AzureStorageBlobDirectory(dir);
            return subDir;
        }

        public async Task<IDataObject> StoreItemAsync(IDataObject source, string name, IUserFeedback feedback, SemaphoreSlim semaphore, OneWaySynchronisationOptions options)
        {
            CloudBlockBlob blobRef = _underlyingDirectory.GetBlockBlobReference(name);
            return await StoreItemAsync(source, blobRef, feedback, semaphore, options).ConfigureAwait(false);
        }

        public async Task DeleteFolderAsync(IUserFeedback feedback)
        {
            feedback.FolderDeleteStarted(FullAddress);
            IList<IFolderMember> members = await GetMembersAsync().ConfigureAwait(false);
            await DeleteContentsAsync(members, feedback).ConfigureAwait(false);
            feedback.FolderDeleteFinished(FullAddress);
        }

        public async Task<IList<IFolderMember>> GetMembersAsync()
        {
            BlobContinuationToken token = null;
            List<IFolderMember> results = new List<IFolderMember>();
            do
            {
                BlobResultSegment answer = await _underlyingDirectory.ListBlobsSegmentedAsync(token);
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
                BlobResultSegment answer = await _underlyingDirectory.ListBlobsSegmentedAsync(true, BlobListingDetails.All, null, token, null, null);
                count += answer.Results.Count();
                token = answer.ContinuationToken;
            } while (token != null);
            return count;
        }
    }
}
