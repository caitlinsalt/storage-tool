using StorageTool.Lib.Interfaces;
using System;

namespace StorageTool.Feedback
{
    class BasicFeedback : IUserFeedback
    {
        public void Init(int objectCount, string title)
        {
            Console.WriteLine($"{title} - {objectCount} objects in source location.");
        }

        public void Error(string name, string message)
        {
            Console.Error.WriteLine($"ERROR: {name}: {message}");
        }

        public void FolderDeleteFinished(string fullName, bool suppressMessage = false)
        {
            if (!suppressMessage)
            {
                Console.WriteLine($"Deleted folder  {fullName}");
            }
        }

        public void FolderDeleteStarted(string fullName)
        {
            Console.WriteLine($"Deleting folder {fullName}");
        }

        public void FolderUploadFinished(string fullName, bool suppressMessage = false)
        {
            if (!suppressMessage)
            {
                Console.WriteLine($"Stored folder   {fullName}");
            }
        }

        public void FolderUploadStarted(string fullName)
        {
            Console.WriteLine($"Storing folder  {fullName}");
        }

        public void ObjectDeleteFinished(string fullName, bool suppressMessage = false)
        {
            if (!suppressMessage)
            {
                Console.WriteLine($"Deleted object  {fullName}");
            }
        }

        public void ObjectDeleteStarted(string fullName)
        {
            Console.WriteLine($"Deleting object {fullName}");
        }

        public void ObjectUploadFinished(string fullName, bool suppressMessage = false)
        {
            if (!suppressMessage)
            {
                Console.WriteLine($"Stored object   {fullName}");
            }
        }

        public void ObjectUploadSkipped(string fullName)
        {
            Console.WriteLine($"Skipped object  {fullName}");
        }

        public void ObjectUploadStarted(string fullName)
        {
            Console.WriteLine($"Storing object  {fullName}");
        }

        public void Finished()
        {

        }
    }
}
