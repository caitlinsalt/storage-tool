using CommandLine;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using StorageTool.Feedback;
using StorageTool.Lib;
using StorageTool.Lib.AzureBlob;
using StorageTool.Lib.Interfaces;
using StorageTool.Lib.Local;
using System;
using System.IO;
using System.Threading;

namespace StorageTool
{
    class Program
    {
        static void Main(string[] args)
        {
            ParserResult<Options> cliParser = Parser.Default.ParseArguments<Options>(args)
                .WithParsed(opts => RunTool(opts))
                .WithNotParsed(err => ExitWithError());
        }

        private static void ExitWithError()
        {
            Environment.Exit(0);
        }

        private static void RunTool(Options options)
        {            
            string folderName = options.LocalFolder;
            if (string.IsNullOrEmpty(folderName))
            {
                folderName = Environment.CurrentDirectory;
            }
            else if (!Path.IsPathFullyQualified(folderName))
            {
                folderName = Path.GetFullPath(folderName);
            }            
            LocalFolder folder = new LocalFolder(folderName);
            SynchronisationOptions syncOptions = options.GetSynchronisationOptions();
            if (syncOptions == 0)
            {
                Environment.Exit(0);
            }

            string blobStorageConnectionString = $"DefaultEndpointsProtocol=https;AccountName={options.StorageAccount};AccountKey={options.Key};EndpointSuffix=core.windows.net";
            CloudStorageAccount.TryParse(blobStorageConnectionString, out CloudStorageAccount storageAccount);
            CloudBlobClient client = storageAccount.CreateCloudBlobClient();
            client.DefaultRequestOptions.StoreBlobContentMD5 = true;
            AzureStorageBlobContainer ctr = AzureStorageBlobContainer.GetContainer(client, options.Container);

            IUserFeedback feedback;
            if (options.Quiet)
            {
                feedback = new NullFeedback();
            }
            else if (options.Console || Console.IsOutputRedirected)
            {
                feedback = new BasicFeedback();
            }
            else
            {
                feedback = new FancyFeedback();
            }

            Synchroniser syncer = new Synchroniser();
            SemaphoreSlim semaphore = new SemaphoreSlim(options.ThreadCount);
            syncer.SyncAsync(folder, ctr, feedback, options.GetSynchronisationOptions(), semaphore).Wait();
            feedback.Finished();
        }
    }
}
