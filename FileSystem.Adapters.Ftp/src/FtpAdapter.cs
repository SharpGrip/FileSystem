using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using FluentFTP;
using FluentFTP.Exceptions;
using SharpGrip.FileSystem.Constants;
using SharpGrip.FileSystem.Exceptions;
using SharpGrip.FileSystem.Extensions;
using SharpGrip.FileSystem.Models;
using SharpGrip.FileSystem.Utilities;
using DirectoryNotFoundException = SharpGrip.FileSystem.Exceptions.DirectoryNotFoundException;
using FileNotFoundException = SharpGrip.FileSystem.Exceptions.FileNotFoundException;

namespace SharpGrip.FileSystem.Adapters.Ftp
{
    public class FtpAdapter : Adapter<FtpAdapterConfiguration, string, string>
    {
        private readonly IAsyncFtpClient client;

        public FtpAdapter(string prefix, string rootPath, IAsyncFtpClient client, Action<FtpAdapterConfiguration>? configuration = null) : base(prefix, rootPath, configuration)
        {
            this.client = client;
        }

        public override void Dispose()
        {
            client.Dispose();
        }

        public override async Task ConnectAsync(CancellationToken cancellationToken = default)
        {
            if (client.IsConnected)
            {
                return;
            }

            try
            {
                Logger.LogStartConnectingAdapter(this);
                await client.Connect(cancellationToken);
                Logger.LogFinishedConnectingAdapter(this);
            }
            catch (Exception exception)
            {
                throw Exception(exception);
            }
        }

        public override async Task<IFile> GetFileAsync(string virtualPath, CancellationToken cancellationToken = default)
        {
            var path = GetPath(virtualPath);

            try
            {
                var file = await client.GetObjectInfo(path, token: cancellationToken);

                if (file == null || file.Type != FtpObjectType.File)
                {
                    throw new FileNotFoundException(path, Prefix);
                }

                return ModelFactory.CreateFile(file, virtualPath);
            }
            catch (Exception exception)
            {
                throw Exception(exception);
            }
        }

        public override async Task<IDirectory> GetDirectoryAsync(string virtualPath, CancellationToken cancellationToken = default)
        {
            var path = GetPath(virtualPath);

            try
            {
                var directory = await client.GetObjectInfo(path, token: cancellationToken);

                if (directory == null || directory.Type != FtpObjectType.Directory)
                {
                    throw new DirectoryNotFoundException(path, Prefix);
                }

                return ModelFactory.CreateDirectory(directory, virtualPath);
            }
            catch (Exception exception)
            {
                throw Exception(exception);
            }
        }

        public override async Task<IEnumerable<IFile>> GetFilesAsync(string virtualPath = "", CancellationToken cancellationToken = default)
        {
            await GetDirectoryAsync(virtualPath, cancellationToken);
            var path = GetPath(virtualPath);

            try
            {
                var ftpListItems = await client.GetListing(path, cancellationToken);

                return ftpListItems.Where(file => file.Type == FtpObjectType.File).Select(file => ModelFactory.CreateFile(file, GetVirtualPath(file.FullName)));
            }
            catch (Exception exception)
            {
                throw Exception(exception);
            }
        }

        public override async Task<IEnumerable<IDirectory>> GetDirectoriesAsync(string virtualPath = "", CancellationToken cancellationToken = default)
        {
            await GetDirectoryAsync(virtualPath, cancellationToken);
            var path = GetPath(virtualPath);

            try
            {
                var ftpListItems = await client.GetListing(path, cancellationToken);

                return ftpListItems.Where(file => file.Type == FtpObjectType.Directory).Select(file => ModelFactory.CreateDirectory(file, GetVirtualPath(file.FullName)));
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

            try
            {
                await client.CreateDirectory(GetPath(virtualPath), cancellationToken);
            }
            catch (Exception exception)
            {
                throw Exception(exception);
            }
        }

        public override async Task DeleteDirectoryAsync(string virtualPath, CancellationToken cancellationToken = default)
        {
            await GetDirectoryAsync(virtualPath, cancellationToken);

            try
            {
                await client.DeleteDirectory(GetPath(virtualPath), cancellationToken);
            }
            catch (Exception exception)
            {
                throw Exception(exception);
            }
        }

        public override async Task DeleteFileAsync(string virtualPath, CancellationToken cancellationToken = default)
        {
            await GetFileAsync(virtualPath, cancellationToken);

            try
            {
                await client.DeleteFile(GetPath(virtualPath), cancellationToken);
            }
            catch (Exception exception)
            {
                throw Exception(exception);
            }
        }

        public override async Task<Stream> ReadFileStreamAsync(string virtualPath, CancellationToken cancellationToken = default)
        {
            await GetFileAsync(virtualPath, cancellationToken);

            try
            {
                var fileStream = await client.OpenRead(GetPath(virtualPath), token: cancellationToken);

                return await StreamUtilities.CopyContentsToMemoryStreamAsync(fileStream, true, cancellationToken);
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

            try
            {
                contents.Seek(0, SeekOrigin.Begin);

                using var writeStream = await client.OpenWrite(GetPath(virtualPath), token: cancellationToken);

                await contents.CopyToAsync(writeStream, FileSystemConstants.Streaming.DefaultMemoryStreamBufferSize, cancellationToken);
                await writeStream.FlushAsync(cancellationToken);
            }
            catch (Exception exception)
            {
                throw Exception(exception);
            }
        }

        public override async Task AppendFileAsync(string virtualPath, Stream contents, CancellationToken cancellationToken = default)
        {
            await GetFileAsync(virtualPath, cancellationToken);

            try
            {
                using var fileStream = await client.OpenAppend(GetPath(virtualPath), token: cancellationToken);

                await contents.CopyToAsync(fileStream);
            }
            catch (Exception exception)
            {
                throw new AdapterRuntimeException(exception);
            }
        }

        protected override Exception Exception(Exception exception)
        {
            if (exception is FileSystemException)
            {
                return exception;
            }

            if (exception is SocketException socketException)
            {
                return new ConnectionException(socketException);
            }

            if (exception is FtpAuthenticationException ftpAuthenticationException)
            {
                return new ConnectionException(ftpAuthenticationException);
            }

            if (exception is FtpSecurityNotAvailableException ftpSecurityNotAvailableException)
            {
                return new ConnectionException(ftpSecurityNotAvailableException);
            }

            return new AdapterRuntimeException(exception);
        }
    }
}