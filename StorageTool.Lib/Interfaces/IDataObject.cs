using System;
using System.IO;
using System.Threading.Tasks;

namespace StorageTool.Lib.Interfaces
{
    public interface IDataObject : IFolderMember
    {
        byte[] Hash { get; }

        long Size { get; }

        string ContentType { get; }

        DateTime UpdateTimestamp { get; }

        Task<Stream> GetReadStreamAsync();

        bool HashEquals(IDataObject other);

        Task DeleteAsync(IUserFeedback feedback);
    }
}
