using Microsoft.Azure.Storage.Blob;
using StorageTool.Lib.Interfaces;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace StorageTool.Lib.AzureBlob
{
    public class AzureStorageBlob : IDataObject
    {
        private readonly ICloudBlob _underlyingBlob;

        public string ShortName => Uri.UnescapeDataString(_underlyingBlob.Uri.Segments.Last());

        public string FullAddress => _underlyingBlob.Uri.ToString();

        public byte[] Hash => Convert.FromBase64String(_underlyingBlob.Properties.ContentMD5);

        public long Size => _underlyingBlob.Properties.Length;

        public string ContentType => _underlyingBlob.Properties.ContentType;

        public DateTime UpdateTimestamp => GetBlobLastModifiedTime();

        private string BaseHash => _underlyingBlob.Properties.ContentMD5;

        public AzureStorageBlob(ICloudBlob blob)
        {
            _underlyingBlob = blob;
        }

        public async Task<Stream> GetReadStreamAsync()
        {
            return await _underlyingBlob.OpenReadAsync().ConfigureAwait(false);
        }

        public bool HashEquals(IDataObject other)
        {
            if (other is AzureStorageBlob otherBlob)
            {
                return BaseHash == otherBlob.BaseHash;
            }
            if (other.Hash == null)
            {
                return Hash == null;
            }
            if (Hash == null)
            {
                return other.Hash == null;
            }
            return Hash.SequenceEqual(other.Hash);
        }

        public async Task DeleteAsync(IUserFeedback feedback)
        {
            feedback.ObjectDeleteStarted(FullAddress);
            await _underlyingBlob.DeleteAsync().ConfigureAwait(false);
            feedback.ObjectDeleteFinished(FullAddress);
        }

        private DateTime GetBlobLastModifiedTime()
        {
            if (_underlyingBlob.Properties.LastModified == null)
            {
                _underlyingBlob.FetchAttributes();
            }
            if (_underlyingBlob.Properties.LastModified == null)
            {
                return DateTime.MinValue;
            }
            return _underlyingBlob.Properties.LastModified.Value.UtcDateTime;
        }
    }
}
