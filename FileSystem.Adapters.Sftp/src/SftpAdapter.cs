using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Renci.SshNet;
using Renci.SshNet.Common;
using SharpGrip.FileSystem.Models;

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
            if (!client.IsConnected)
            {
                client.Connect();
            }
        }

        public IFile GetFile(string path)
        {
            var file = client.Get(PrependRootPath(path));

            if (file.IsDirectory)
            {
                throw new FileNotFoundException();
            }

            return ModelFactory.CreateFile(file);
        }

        public IDirectory GetDirectory(string path)
        {
            var directory = client.Get(PrependRootPath(path));

            if (!directory.IsDirectory)
            {
                throw new DirectoryNotFoundException();
            }

            return ModelFactory.CreateDirectory(directory);
        }

        public IEnumerable<IFile> GetFiles(string path = "")
        {
            path = PrependRootPath(path);
            var directory = client.Get(path);

            if (!directory.IsDirectory)
            {
                throw new DirectoryNotFoundException();
            }

            return client.ListDirectory(path).Where(item => !item.IsDirectory).Select(ModelFactory.CreateFile).ToList();
        }

        public IEnumerable<IDirectory> GetDirectories(string path = "")
        {
            path = PrependRootPath(path);
            var directory = client.Get(path);

            if (!directory.IsDirectory)
            {
                throw new DirectoryNotFoundException();
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

        public DirectoryInfo CreateDirectory(string path)
        {
            throw new NotImplementedException();
        }

        public Task DeleteFile(string path)
        {
            throw new NotImplementedException();
        }

        public Task DeleteDirectory(string path, bool recursive)
        {
            throw new NotImplementedException();
        }

        public Task<byte[]> ReadFile(string path)
        {
            throw new NotImplementedException();
        }

        public Task<string> ReadTextFile(string path)
        {
            throw new NotImplementedException();
        }

        public Task WriteFile(string path, byte[] contents, bool overwrite = false)
        {
            throw new NotImplementedException();
        }

        public Task WriteFile(string path, string contents, bool overwrite = false)
        {
            throw new NotImplementedException();
        }

        public Task AppendFile(string sourcePath, byte[] contents)
        {
            throw new NotImplementedException();
        }

        public Task AppendFile(string sourcePath, string contents)
        {
            throw new NotImplementedException();
        }
    }
}