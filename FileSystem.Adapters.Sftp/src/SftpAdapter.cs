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
                throw new ConnectionException(exception);
            }
        }

        public override IFile GetFile(string path)
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

        public override IDirectory GetDirectory(string path)
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

        public override IEnumerable<IFile> GetFiles(string path = "")
        {
            GetDirectory(path);
            path = PrependRootPath(path);

            return client.ListDirectory(path).Where(item => !item.IsDirectory).Select(ModelFactory.CreateFile).ToList();
        }

        public override IEnumerable<IDirectory> GetDirectories(string path = "")
        {
            GetDirectory(path);
            path = PrependRootPath(path);

            return client.ListDirectory(path).Where(item => item.IsDirectory).Select(ModelFactory.CreateDirectory).ToList();
        }

        public override void CreateDirectory(string path)
        {
            client.CreateDirectory(PrependRootPath(path));
        }

        public override void DeleteFile(string path)
        {
            client.DeleteFile(PrependRootPath(path));
        }

        public override void DeleteDirectory(string path)
        {
            client.DeleteDirectory(PrependRootPath(path));
        }

        public override async Task<byte[]> ReadFileAsync(string path)
        {
            await using var fileStream = client.OpenRead(PrependRootPath(path));
            var fileContents = new byte[fileStream.Length];

            await fileStream.ReadAsync(fileContents, 0, (int) fileStream.Length);

            return fileContents;
        }

        public override async Task<string> ReadTextFileAsync(string path)
        {
            using var streamReader = new StreamReader(client.OpenRead(PrependRootPath(path)));

            return await streamReader.ReadToEndAsync();
        }

        public override async Task WriteFileAsync(string path, byte[] contents, bool overwrite = false)
        {
            if (!overwrite && FileExists(path))
            {
                throw new FileExistsException(PrependRootPath(path), Prefix);
            }

            await Task.Factory.StartNew(() => client.WriteAllBytes(PrependRootPath(path), contents));
        }

        public override async Task AppendFileAsync(string path, byte[] contents)
        {
            if (!FileExists(path))
            {
                throw new FileExistsException(PrependRootPath(path), Prefix);
            }

            var stringContents = Encoding.UTF8.GetString(contents, 0, contents.Length);

            await Task.Factory.StartNew(() => client.AppendAllText(PrependRootPath(path), stringContents));
        }
    }
}