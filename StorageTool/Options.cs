using CommandLine;
using StorageTool.Lib;

namespace StorageTool
{
    public class Options
    {
        [Option('q', "quiet", Required = false, HelpText = "Mute output.")]
        public bool Quiet { get; set; }

        [Option('s', "storageaccount", Required = true, HelpText = "The name of the Azure storage account.")]
        public string StorageAccount { get; set; }

        [Option('c', "container", Required = true, HelpText = "The name of the Azure storage container (created if it does not exist).")]
        public string Container { get; set; }

        [Option('k', "key", Required = true, HelpText = "The Azure Storage account key.")]
        public string Key { get; set; }

        [Option('l', "local", Required = false, HelpText = "Local folder to use as source/or destination (defaults to current working directory).")]
        public string LocalFolder { get; set; }

        [Option('u', "upload", Required = false, HelpText = "Upload files to Azure.")]
        public bool Upload { get; set; }

        [Option('d', "download", Required = false, HelpText = "Download files from Azure.")]
        public bool Download { get; set; }

        [Option('n', "new", Required = false, Default = true, HelpText = "Transfer files that are missing on the destination side")]
        public bool New { get; set; }

        [Option('g', "changed", Required = false, Default = true, HelpText = "Transfer files that exist but with different content on the destination side.  If both --upload and --download are specified, the newest version is chosen.")]
        public bool Changed { get; set; }

        [Option('x', "deleted", Required = false, Default = true, HelpText = "If files on the destination side do not exist on the source side, delete them from the destionation side.  Ignored if both --upload and --download are specified")]
        public bool Deleted { get; set; }

        [Option('e', "console", Required = false, Default = false, HelpText = "Force output into 'plain console' format.")]
        public bool Console { get; set; }

        [Option('t', "threads", Required = false, Default = 32, HelpText = "Maximum number of concurrent expensive operations (disk reads and object transfers)")]
        public int ThreadCount { get; set; }

        internal SynchronisationOptions GetSynchronisationOptions()
        {
            SynchronisationOptions syncOpts = 0;
            if (Upload)
            {
                if (New)
                {
                    syncOpts |= SynchronisationOptions.UploadNew;
                }
                if (Changed)
                {
                    if (Download)
                    {
                        syncOpts |= SynchronisationOptions.UploadExisting;
                    }
                    else
                    {
                        syncOpts |= SynchronisationOptions.UploadForceExisting;
                    }
                }
                if (Deleted && !Download)
                {
                    syncOpts |= SynchronisationOptions.UploadDeletions;
                }
            }
            if (Download)
            {
                if (New)
                {
                    syncOpts |= SynchronisationOptions.DownloadNew;
                }
                if (Changed)
                {
                    if (Upload)
                    {
                        syncOpts |= SynchronisationOptions.DownloadExisting;
                    }
                    else
                    {
                        syncOpts |= SynchronisationOptions.DownloadForceExisting;
                    }
                }
                if (Deleted && !Upload)
                {
                    syncOpts |= SynchronisationOptions.DownloadDeletions;
                }
            }

            return syncOpts;
        }
    }
}
