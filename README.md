# Tools for Azure Blob Storage

(currently for the command-line only)

This project was started partly for study purposes, and partly because I wanted a tool to back up local folders to Azure storage accounts.

This is at present very much alpha-release software; as yet I have not lost any data to it, but there will almost certainly be bugs I have not yet found and code paths I have not yet exercised.

The project was developed using Visual Studio 2019 and currently consists of:

- `StorageTool.Lib`, a .NET Standard 2.0 compatible library that contains abstractions for Azure Blob Storage and local filesystem concepts, and logic for syncing the two.
- `StorageTool`, a .NET Core 2.1 command-line app which can be used to synchronise a local folder to an Azure Blob Storage container or vice-versa
- `StorageTool.Gui`, an empty placeholder.

## Dependencies

The project currently only has the following non-framework dependencies:

- `Microsoft.Azure.Storage.Blob` v10.0.1
- `CommandLineParser` v2.4.3 [source code here](https://github.com/commandlineparser/commandline)

## Usage

The command-line tool has the following options:

### Source, destination and credentials

- `-l <path>` or `--local=<path>`: path of a local folder to synchronise.
- `-s <account>` or `--storageaccount=<account>`: the name of the Azure Storage account to synchronise.
- `-c <container>` or `--container=<container>`: the name of the container within the given Azure Storage account to synchronise.
- `-k <key>` or `--key=<key>`: the Azure Storage account key.
- `-u` or `--upload`: treat the local folder as a source and the Azure Storage container as a destination.
- `-d` or `--download`: treat the Azure Storage container as a source and the local folder as a destination.

It is possible to use both `-u` and `-d` together, with limitations on what actions can then be used.

### Actions

- `-n` or `--new=[true|false]`: Transfer new files, or those that are present in the source location but not in the destination location.
- `-g` or `--changed=[true|false]`: Transfer changed files, or those that are present in both locations but have different content.
- `-x` or `--deleted=[true|false]`: If a file is present in the destination location but not in the source location, delete it from the destination location.

The `-n` and `-g` options are selected by default.  The `--deleted` option is selected by default if `--upload` or `--download` are used separately, but is disabled entirely if `--upload` and `--download` are used together to prevent any conflict with the `-n` option.

If the `-g` option is selected, the program will compare the metadata of the local file with that of the remote blob and only transfer the files if, firstly, the MD5 checksum of the local file differs from the `Content-MD5` header of the remote blob; and secondly, if the last changed date of the source file is more recent than the last changed date of the destination file.

### Other options

- `-q` or `--quiet`: Do not output any success messages to the console, only errors.
- `-e` or `--console`: Output progress information in a "plain console" format, consisting of plain text only.  This option is selected by default if the program detects that output has been redirected to a file.
- `-t` or `--threads=<n>`: The maximum number of "expensive" operations such as file or directory reads, or uploads and downloads, that are allowed to occur simultaneously.  The default value is an arbitrary 32.

If `-q` or `-e` are not specified and the program thinks it is outputting to a normal console or Powershell window, it will display a progress bar.

## Possible future developments

- Set the `Content-Type` header of blobs properly on upload.
- Develop a GUI.
- Enable blob-container-to-blob-container copying, using the Azure Blob Storage Client API to carry out a cloud-side transfer.
- Enable syncing of blob directories, not just containers.
- Allow RBAC account-based authentication.
- Allow users to control more properties of uploaded blobs, such as block size.
- Enable upload of only the changed portions of large files, using block checksums.
