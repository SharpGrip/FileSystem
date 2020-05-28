using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Renci.SshNet;
using Renci.SshNet.Common;
using SharpGrip.FileSystem.Exceptions;
using SharpGrip.FileSystem.Models;
using DirectoryNotFoundException = SharpGrip.FileSystem.Exceptions.DirectoryNotFoundException;
using FileNotFoundException = SharpGrip.FileSystem.Exceptions.FileNotFoundException;

namespace SharpGrip.FileSystem.Adapters.Sftp
{
    public class SftpAdapter : Adapter
    {
        private readonly SftpClient client;

        public SftpAdapter(string prefix, string rootPath, SftpClient client) : base(prefix, rootPath)
        {
            this.client = client;
        }

        public override void Dispose()
        {
            client.Dispose();
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

        public override async Task<IFile> GetFileAsync(string path, CancellationToken cancellationToken = default)
        {
            path = PrependRootPath(path);

            try
            {
                var file = await Task.Run(() => client.Get(path), cancellationToken);

                if (file.IsDirectory)
                {
                    throw new FileNotFoundException(path, Prefix);
                }

                return ModelFactory.CreateFile(file);
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

        public override async Task<IDirectory> GetDirectoryAsync(string path, CancellationToken cancellationToken = default)
        {
            path = PrependRootPath(path);

            try
            {
                var directory = await Task.Run(() => client.Get(path), cancellationToken);

                if (!directory.IsDirectory)
                {
                    throw new DirectoryNotFoundException(path, Prefix);
                }

                return ModelFactory.CreateDirectory(directory);
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

        public override async Task<IEnumerable<IFile>> GetFilesAsync(string path = "", CancellationToken cancellationToken = default)
        {
            await GetDirectoryAsync(path, cancellationToken);
            path = PrependRootPath(path);

            try
            {
                return await Task.Run(
                    () => client.ListDirectory(path).Where(item => !item.IsDirectory).Select(ModelFactory.CreateFile).ToList(),
                    cancellationToken
                );
            }
            catch (Exception exception)
            {
                throw Exception(exception);
            }
        }

        public override async Task<IEnumerable<IDirectory>> GetDirectoriesAsync(
            string path = "",
            CancellationToken cancellationToken = default
        )
        {
            await GetDirectoryAsync(path, cancellationToken);
            path = PrependRootPath(path);

            try
            {
                return await Task.Run(
                    () => client.ListDirectory(path).Where(item => item.IsDirectory).Select(ModelFactory.CreateDirectory).ToList(),
                    cancellationToken
                );
            }
            catch (Exception exception)
            {
                throw Exception(exception);
            }
        }

        public override async Task CreateDirectoryAsync(string path, CancellationToken cancellationToken = default)
        {
            if (await DirectoryExistsAsync(path, cancellationToken))
            {
                throw new DirectoryExistsException(PrependRootPath(path), Prefix);
            }

            try
            {
                await Task.Run(() => client.CreateDirectory(PrependRootPath(path)), cancellationToken);
            }
            catch (Exception exception)
            {
                throw Exception(exception);
            }
        }

        public override async Task DeleteFileAsync(string path, CancellationToken cancellationToken = default)
        {
            await GetFileAsync(path, cancellationToken);

            try
            {
                await Task.Run(() => client.DeleteFile(PrependRootPath(path)), cancellationToken);
            }
            catch (Exception exception)
            {
                throw Exception(exception);
            }
        }

        public override async Task DeleteDirectoryAsync(string path, CancellationToken cancellationToken = default)
        {
            await GetDirectoryAsync(path, cancellationToken);

            try
            {
                await Task.Run(() => client.DeleteDirectory(PrependRootPath(path)), cancellationToken);
            }
            catch (Exception exception)
            {
                throw Exception(exception);
            }
        }

        public override async Task<byte[]> ReadFileAsync(string path, CancellationToken cancellationToken = default)
        {
            await GetFileAsync(path, cancellationToken);

            try
            {
                await using var fileStream = client.OpenRead(PrependRootPath(path));
                var fileContents = new byte[fileStream.Length];

                await fileStream.ReadAsync(fileContents, 0, (int) fileStream.Length, cancellationToken);

                return fileContents;
            }
            catch (Exception exception)
            {
                throw Exception(exception);
            }
        }

        public override async Task<string> ReadTextFileAsync(string path, CancellationToken cancellationToken = default)
        {
            await GetFileAsync(path, cancellationToken);

            try
            {
                using var streamReader = new StreamReader(client.OpenRead(PrependRootPath(path)));

                return await streamReader.ReadToEndAsync();
            }
            catch (Exception exception)
            {
                throw Exception(exception);
            }
        }

        public override async Task WriteFileAsync(
            string path,
            byte[] contents,
            bool overwrite = false,
            CancellationToken cancellationToken = default
        )
        {
            if (!overwrite && await FileExistsAsync(path, cancellationToken))
            {
                throw new FileExistsException(PrependRootPath(path), Prefix);
            }

            try
            {
                await Task.Run(() => client.WriteAllBytes(PrependRootPath(path), contents), cancellationToken);
            }
            catch (Exception exception)
            {
                throw Exception(exception);
            }
        }

        public override async Task AppendFileAsync(string path, byte[] contents, CancellationToken cancellationToken = default)
        {
            await GetFileAsync(path, cancellationToken);

            try
            {
                var stringContents = Encoding.UTF8.GetString(contents, 0, contents.Length);

                await Task.Run(() => client.AppendAllText(PrependRootPath(path), stringContents), cancellationToken);
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

        private static Exception Exception(Exception exception)
        {
            if (exception is FileSystemException)
            {
                return exception;
            }

            if (exception is SshConnectionException sshConnectionException)
            {
                return new ConnectionException(sshConnectionException);
            }

            return new AdapterRuntimeException(exception);
        }
    }
}