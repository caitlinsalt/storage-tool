using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace StorageTool.Lib.Interfaces
{
    public interface IFolder : IFolderMember
    {
        Task<IList<IFolderMember>> GetMembersAsync();

        IFolder CreateSubfolder(string name);

        Task<IDataObject> StoreItemAsync(IDataObject source, string name, IUserFeedback feedback, SemaphoreSlim semaphore, OneWaySynchronisationOptions options = OneWaySynchronisationOptions.All);

        Task DeleteFolderAsync(IUserFeedback feedback);

        Task<int> GetItemCountAsync();
    }
}
