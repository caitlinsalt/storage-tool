using StorageTool.Lib.Interfaces;
using System;

namespace StorageTool.Feedback
{
    class NullFeedback : IUserFeedback
    {
        public void Init(int objectCount, string title)
        {

        }

        public void Error(string name, string message)
        {
            Console.Error.WriteLine($"ERROR: {name}: {message}");
        }

        public void FolderDeleteFinished(string fullName, bool suppressMessage = false)
        {
            
        }

        public void FolderDeleteStarted(string fullName)
        {
            
        }

        public void FolderUploadFinished(string fullName, bool suppressMessage = false)
        {
            
        }

        public void FolderUploadStarted(string fullName)
        {
            
        }

        public void ObjectDeleteFinished(string fullName, bool suppressMessage = false)
        {

        }

        public void ObjectDeleteStarted(string fullName)
        {

        }

        public void ObjectUploadFinished(string fullName, bool suppressMessage = false)
        {

        }

        public void ObjectUploadSkipped(string fullName)
        {

        }

        public void ObjectUploadStarted(string fullName)
        {

        }

        public void Finished()
        {

        }
    }
}
