using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Renci.SshNet;
using Renci.SshNet.Common;
using SharpGrip.FileSystem.Exceptions;
using SharpGrip.FileSystem.Models;
using DirectoryNotFoundException = SharpGrip.FileSystem.Exceptions.DirectoryNotFoundException;
using FileNotFoundException = SharpGrip.FileSystem.Exceptions.FileNotFoundException;

namespace SharpGrip.FileSystem.Adapters.Sftp
{
    public class SftpAdapter : Adapter, IAdapter
    {
        private readonly SftpClient client;

        public SftpAdapter(string prefix, string rootPath, ConnectionInfo connectionInfo) : base(prefix, rootPath)
        {
            client = new SftpClient(connectionInfo);
        }

        public void Dispose()
        {
            client.Dispose();
        }

        public void Connect()
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
                throw new ConnectionException(exception);
            }
        }

        public IFile GetFile(string path)
        {
            path = PrependRootPath(path);

            try
            {
                var file = client.Get(path);

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
                throw new AdapterRuntimeException(exception);
            }
        }

        public IDirectory GetDirectory(string path)
        {
            path = PrependRootPath(path);

            try
            {
                var directory = client.Get(path);

                if (!directory.IsDirectory)
                {
                    throw new DirectoryNotFoundException(path, Prefix);
                }

                return ModelFactory.CreateDirectory(directory);
            }
            catch (SftpPathNotFoundException)
            {
                throw new FileNotFoundException(path, Prefix);
            }
            catch (Exception exception)
            {
                throw new AdapterRuntimeException(exception);
            }
        }

        public IEnumerable<IFile> GetFiles(string path = "")
        {
            path = PrependRootPath(path);
            var directory = client.Get(path);

            if (!directory.IsDirectory)
            {
                throw new DirectoryNotFoundException(path, Prefix);
            }

            return client.ListDirectory(path).Where(item => !item.IsDirectory).Select(ModelFactory.CreateFile).ToList();
        }

        public IEnumerable<IDirectory> GetDirectories(string path = "")
        {
            path = PrependRootPath(path);
            var directory = client.Get(path);

            if (!directory.IsDirectory)
            {
                throw new DirectoryNotFoundException(path, Prefix);
            }

            return client.ListDirectory(path).Where(item => item.IsDirectory).Select(ModelFactory.CreateDirectory).ToList();
        }

        public bool FileExists(string path)
        {
            try
            {
                GetFile(path);
            }
            catch (FileNotFoundException)
            {
                return false;
            }
            catch (SftpPathNotFoundException)
            {
                return false;
            }

            return true;
        }

        public bool DirectoryExists(string path)
        {
            try
            {
                GetDirectory(path);
            }
            catch (DirectoryNotFoundException)
            {
                return false;
            }
            catch (SftpPathNotFoundException)
            {
                return false;
            }

            return true;
        }

        public Stream CreateFile(string path)
        {
            return client.Create(PrependRootPath(path));
        }

        public void CreateDirectory(string path)
        {
            client.CreateDirectory(PrependRootPath(path));
        }

        public void DeleteFile(string path)
        {
            client.DeleteFile(PrependRootPath(path));
        }

        public void DeleteDirectory(string path)
        {
            client.DeleteDirectory(PrependRootPath(path));
        }

        public async Task<byte[]> ReadFile(string path)
        {
            await using var fileStream = client.OpenRead(PrependRootPath(path));
            var fileContents = new byte[fileStream.Length];

            await fileStream.ReadAsync(fileContents, 0, (int) fileStream.Length);

            return fileContents;
        }

        public async Task<string> ReadTextFile(string path)
        {
            using var streamReader = new StreamReader(client.OpenRead(PrependRootPath(path)));

            return await streamReader.ReadToEndAsync();
        }

        public async Task WriteFile(string path, byte[] contents, bool overwrite = false)
        {
            if (!overwrite && FileExists(path))
            {
                throw new FileExistsException(PrependRootPath(path), Prefix);
            }

            await Task.Factory.StartNew(() => client.WriteAllBytes(PrependRootPath(path), contents));
        }

        public async Task WriteFile(string path, string contents, bool overwrite = false)
        {
            if (!overwrite && FileExists(path))
            {
                throw new FileExistsException(PrependRootPath(path), Prefix);
            }

            await Task.Factory.StartNew(() => client.WriteAllText(PrependRootPath(path), contents));
        }

        public async Task AppendFile(string sourcePath, byte[] contents)
        {
            var stringContents = Encoding.UTF8.GetString(contents, 0, contents.Length);

            await Task.Factory.StartNew(() => client.AppendAllText(PrependRootPath(sourcePath), stringContents));
        }

        public async Task AppendFile(string sourcePath, string contents)
        {
            await Task.Factory.StartNew(() => client.AppendAllText(PrependRootPath(sourcePath), contents));
        }
    }
}