namespace StorageTool.Lib.Interfaces
{
    public interface IUserFeedback
    {
        void Init(int objectCount, string title);

        void Error(string name, string error);

        void FolderUploadStarted(string fullName);

        void FolderUploadFinished(string fullName, bool suppressMessage = false);

        void ObjectUploadStarted(string fullName);

        void ObjectUploadFinished(string fullName, bool suppressMessage = false);

        void ObjectUploadSkipped(string fullName);

        void FolderDeleteStarted(string fullName);

        void FolderDeleteFinished(string fullName, bool suppressMessage = false);

        void ObjectDeleteStarted(string fullName);

        void ObjectDeleteFinished(string fullName, bool suppressMessage = false);

        void Finished();
    }
}
