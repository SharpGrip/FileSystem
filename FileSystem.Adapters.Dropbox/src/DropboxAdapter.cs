using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dropbox.Api;
using Dropbox.Api.Files;
using SharpGrip.FileSystem.Exceptions;
using SharpGrip.FileSystem.Extensions;
using SharpGrip.FileSystem.Models;
using SharpGrip.FileSystem.Utilities;
using DirectoryNotFoundException = SharpGrip.FileSystem.Exceptions.DirectoryNotFoundException;
using FileNotFoundException = SharpGrip.FileSystem.Exceptions.FileNotFoundException;

namespace SharpGrip.FileSystem.Adapters.Dropbox
{
    public class DropboxAdapter : Adapter<DropboxAdapterConfiguration, string, string>
    {
        private readonly DropboxClient client;

        public DropboxAdapter(string prefix, string rootPath, DropboxClient client, Action<DropboxAdapterConfiguration>? configuration = null) : base(prefix, rootPath, configuration)
        {
            this.client = client;
        }

        public override void Dispose()
        {
            client.Dispose();
        }

        public override async Task ConnectAsync(CancellationToken cancellationToken = default)
        {
            Logger.LogStartConnectingAdapter(this);
            await Task.CompletedTask;
            Logger.LogFinishedConnectingAdapter(this);
        }

        public override async Task<IFile> GetFileAsync(string virtualPath, CancellationToken cancellationToken = default)
        {
            var path = GetPath(virtualPath).EnsureLeadingForwardSlash().RemoveTrailingForwardSlash();

            try
            {
                var file = await client.Files.GetMetadataAsync(path);

                if (file.IsFolder)
                {
                    throw new FileNotFoundException(path, Prefix);
                }

                return ModelFactory.CreateFile(file, virtualPath);
            }
            catch (ApiException<GetMetadataError> exception)
            {
                if (exception.Message.Contains("path/not_found"))
                {
                    throw new FileNotFoundException(path, Prefix);
                }

                throw Exception(exception);
            }
            catch (Exception exception)
            {
                throw Exception(exception);
            }
        }

        public override async Task<IDirectory> GetDirectoryAsync(string virtualPath, CancellationToken cancellationToken = default)
        {
            var path = GetPath(virtualPath).EnsureLeadingForwardSlash().RemoveTrailingForwardSlash();

            try
            {
                if (path == "")
                {
                    return ModelFactory.CreateDirectory("", path, virtualPath);
                }

                var directory = await client.Files.GetMetadataAsync(path);

                if (directory.IsFile)
                {
                    throw new DirectoryNotFoundException(path, Prefix);
                }

                return ModelFactory.CreateDirectory(directory, virtualPath);
            }
            catch (ApiException<GetMetadataError> exception)
            {
                if (exception.Message.Contains("path/not_found"))
                {
                    throw new DirectoryNotFoundException(path, Prefix);
                }

                throw Exception(exception);
            }
            catch (Exception exception)
            {
                throw Exception(exception);
            }
        }

        public override async Task<IEnumerable<IFile>> GetFilesAsync(string virtualPath = "", CancellationToken cancellationToken = default)
        {
            await GetDirectoryAsync(virtualPath, cancellationToken);

            var path = GetPath(virtualPath).EnsureLeadingForwardSlash().RemoveTrailingForwardSlash();

            try
            {
                if (path == "/")
                {
                    path = "";
                }

                var result = await client.Files.ListFolderAsync(path);

                return result.Entries.Where(item => !item.IsFolder).Select(file => ModelFactory.CreateFile(file, GetVirtualPath(file.PathDisplay))).ToList();
            }
            catch (Exception exception)
            {
                throw Exception(exception);
            }
        }

        public override async Task<IEnumerable<IDirectory>> GetDirectoriesAsync(string virtualPath = "", CancellationToken cancellationToken = default)
        {
            await GetDirectoryAsync(virtualPath, cancellationToken);

            var path = GetPath(virtualPath).EnsureLeadingForwardSlash().RemoveTrailingForwardSlash();

            try
            {
                if (path == "/")
                {
                    path = "";
                }

                var result = await client.Files.ListFolderAsync(path);

                return result.Entries.Where(item => item.IsFolder).Select(directory => ModelFactory.CreateDirectory(directory, GetVirtualPath(directory.PathDisplay))).ToList();
            }
            catch (Exception exception)
            {
                throw Exception(exception);
            }
        }

        public override async Task CreateDirectoryAsync(string virtualPath, CancellationToken cancellationToken = default)
        {
            if (await DirectoryExistsAsync(virtualPath, cancellationToken))
            {
                throw new DirectoryExistsException(GetPath(virtualPath), Prefix);
            }

            var path = GetPath(virtualPath).EnsureLeadingForwardSlash().RemoveTrailingForwardSlash();

            try
            {
                await client.Files.CreateFolderV2Async(path);
            }
            catch (Exception exception)
            {
                throw Exception(exception);
            }
        }

        public override async Task DeleteFileAsync(string virtualPath, CancellationToken cancellationToken = default)
        {
            await GetFileAsync(virtualPath, cancellationToken);

            var path = GetPath(virtualPath).EnsureLeadingForwardSlash().RemoveTrailingForwardSlash();

            try
            {
                await client.Files.DeleteV2Async(path);
            }
            catch (Exception exception)
            {
                throw Exception(exception);
            }
        }

        public override async Task DeleteDirectoryAsync(string virtualPath, CancellationToken cancellationToken = default)
        {
            await GetDirectoryAsync(virtualPath, cancellationToken);

            var path = GetPath(virtualPath).EnsureLeadingForwardSlash().RemoveTrailingForwardSlash();

            try
            {
                await client.Files.DeleteV2Async(path);
            }
            catch (Exception exception)
            {
                throw Exception(exception);
            }
        }

        public override async Task<Stream> ReadFileStreamAsync(string virtualPath, CancellationToken cancellationToken = default)
        {
            await GetFileAsync(virtualPath, cancellationToken);

            var path = GetPath(virtualPath).EnsureLeadingForwardSlash().RemoveTrailingForwardSlash();

            try
            {
                // Performance issue:
                // The stream returned from the service does not support seeking and therefore cannot determine the content length (required when creating files from this stream).
                // Copy the response stream to a new memory stream and return that one instead.

                var response = await client.Files.DownloadAsync(path);
                var memoryStream = await StreamUtilities.CopyContentsToMemoryStreamAsync(await response.GetContentAsStreamAsync(), true, cancellationToken);

                return memoryStream;
            }
            catch (Exception exception)
            {
                throw Exception(exception);
            }
        }

        public override async Task WriteFileAsync(string virtualPath, Stream contents, bool overwrite = false, CancellationToken cancellationToken = default)
        {
            if (!overwrite && await FileExistsAsync(virtualPath, cancellationToken))
            {
                throw new FileExistsException(GetPath(virtualPath), Prefix);
            }

            var path = GetPath(virtualPath).EnsureLeadingForwardSlash().RemoveTrailingForwardSlash();

            try
            {
                await client.Files.UploadAsync(path, WriteMode.Overwrite.Instance, body: contents);
            }
            catch (Exception exception)
            {
                throw Exception(exception);
            }
        }

        protected override Exception Exception(Exception exception)
        {
            if (exception is FileSystemException)
            {
                return exception;
            }

            if (exception is AuthException authException)
            {
                throw new ConnectionException(authException);
            }

            return new AdapterRuntimeException(exception);
        }
    }
}