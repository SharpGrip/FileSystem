using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Renci.SshNet;
using Renci.SshNet.Common;
using SharpGrip.FileSystem.Constants;
using SharpGrip.FileSystem.Exceptions;
using SharpGrip.FileSystem.Models;
using SharpGrip.FileSystem.Utilities;
using DirectoryNotFoundException = SharpGrip.FileSystem.Exceptions.DirectoryNotFoundException;
using FileNotFoundException = SharpGrip.FileSystem.Exceptions.FileNotFoundException;

namespace SharpGrip.FileSystem.Adapters.Sftp
{
    public class SftpAdapter : Adapter<SftpAdapterConfiguration, string, string>
    {
        private readonly ISftpClient client;

        private readonly string[] excludedFileNames = {".", ".."};
        private readonly string[] excludedDirectoryNames = {".", ".."};

        public SftpAdapter(string prefix, string rootPath, ISftpClient client, Action<SftpAdapterConfiguration>? configuration = null) : base(prefix, rootPath, configuration)
        {
            this.client = client;
        }

        public override void Dispose()
        {
            ((IBaseClient) client).Dispose();
        }

        public override void Connect()
        {
            if (client.IsConnected)
            {
                return;
            }

            try
            {
                client.Connect();
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
                var file = await Task.Run(() => client.Get(path), cancellationToken);

                if (file.IsDirectory)
                {
                    throw new FileNotFoundException(path, Prefix);
                }

                return ModelFactory.CreateFile(file, virtualPath);
            }
            catch (SftpPathNotFoundException)
            {
                throw new FileNotFoundException(path, Prefix);
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
                var directory = await Task.Run(() => client.Get(path), cancellationToken);

                if (!directory.IsDirectory)
                {
                    throw new DirectoryNotFoundException(path, Prefix);
                }

                return ModelFactory.CreateDirectory(directory, virtualPath);
            }
            catch (SftpPathNotFoundException)
            {
                throw new DirectoryNotFoundException(path, Prefix);
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
                return await Task.Run(() => client.ListDirectory(path)
                    .Where(item => !item.IsDirectory && !excludedFileNames.Contains(item.Name))
                    .Select(file => ModelFactory.CreateFile(file, GetVirtualPath(file.FullName))).ToList(), cancellationToken);
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
                return await Task.Run(() => client.ListDirectory(path)
                    .Where(item => item.IsDirectory && !excludedDirectoryNames.Contains(item.Name))
                    .Select(directory => ModelFactory.CreateDirectory(directory, GetVirtualPath(directory.FullName))).ToList(), cancellationToken);
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
                await Task.Run(() => client.CreateDirectory(GetPath(virtualPath)), cancellationToken);
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
                var files = await GetFilesAsync(virtualPath, cancellationToken);

                foreach (var file in files)
                {
                    await DeleteFileAsync(file.VirtualPath, cancellationToken);
                }

                var subDirectories = await GetDirectoriesAsync(virtualPath, cancellationToken);

                foreach (var subDirectory in subDirectories)
                {
                    await DeleteDirectoryAsync(subDirectory.VirtualPath, cancellationToken);
                }

                await Task.Run(() => client.DeleteDirectory(GetPath(virtualPath)), cancellationToken);
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
                await Task.Run(() => client.DeleteFile(GetPath(virtualPath)), cancellationToken);
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
                return client.OpenRead(GetPath(virtualPath));
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

                using var writeStream = client.OpenWrite(GetPath(virtualPath));

                await contents.CopyToAsync(writeStream, AdapterConstants.DefaultMemoryStreamBufferSize, cancellationToken);
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
                using var memoryStream = await StreamUtilities.CopyContentsToMemoryStreamAsync(contents, true, cancellationToken);
                var fileContents = memoryStream.ToArray();
                var stringContents = Encoding.UTF8.GetString(fileContents, 0, fileContents.Length);

                await Task.Run(() => client.AppendAllText(GetPath(virtualPath), stringContents), cancellationToken);
            }
            catch (SshConnectionException exception)
            {
                throw new ConnectionException(exception);
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

            if (exception is SshConnectionException sshConnectionException)
            {
                return new ConnectionException(sshConnectionException);
            }

            if (exception is SocketException socketException)
            {
                return new ConnectionException(socketException);
            }

            if (exception is SshAuthenticationException sshAuthenticationException)
            {
                return new ConnectionException(sshAuthenticationException);
            }

            if (exception is ProxyException proxyException)
            {
                return new ConnectionException(proxyException);
            }

            return new AdapterRuntimeException(exception);
        }
    }
}