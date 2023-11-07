using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Renci.SshNet;
using Renci.SshNet.Common;
using Renci.SshNet.Sftp;
using SharpGrip.FileSystem.Adapters;
using SharpGrip.FileSystem.Adapters.Sftp;
using SharpGrip.FileSystem.Exceptions;
using Xunit;
using DirectoryNotFoundException = SharpGrip.FileSystem.Exceptions.DirectoryNotFoundException;
using FileNotFoundException = SharpGrip.FileSystem.Exceptions.FileNotFoundException;

namespace SharpGrip.FileSystem.Tests.FileSystem.Adapters.Sftp
{
    public class SftpAdapterTest : IAdapterTests
    {
        [Fact]
        public void Test_Instantiation()
        {
            var sftpClient = Substitute.For<ISftpClient>();
            var sftpAdapter = new SftpAdapter("prefix", "/root-path", sftpClient);

            Assert.Equal("prefix", sftpAdapter.Prefix);
            Assert.Equal("/root-path", sftpAdapter.RootPath);
        }

        [Fact]
        public Task Test_Connect()
        {
            var sftpClient = Substitute.For<ISftpClient>();
            var sftpAdapter = new SftpAdapter("prefix", "/root-path", sftpClient);

            sftpAdapter.Connect();

            return Task.CompletedTask;
        }

        [Fact]
        public async Task Test_Get_File_Async()
        {
            var sftpClient = Substitute.For<ISftpClient>();
            var sftpAdapter = new SftpAdapter("prefix-1", "root-path-1", sftpClient);
            var fileSystem = new SharpGrip.FileSystem.FileSystem(new List<IAdapter> {sftpAdapter});

            var sftpFile = Substitute.For<ISftpFile>();

            sftpFile.Name.Returns("test1.txt");
            sftpFile.FullName.Returns("root-path-1/test1.txt");
            sftpFile.Length.Returns(1);
            sftpFile.LastWriteTime.Returns(new DateTime(1970, 1, 1));
            sftpFile.IsDirectory.Returns(false);

            sftpClient.Get("root-path-1/test1.txt").Returns(sftpFile);
            sftpClient.Get("root-path-1/test2.txt").Throws(new SftpPathNotFoundException());
            sftpClient.Get("root-path-1/test3.txt").Throws(new SshConnectionException());
            sftpClient.Get("root-path-1/test4.txt").Throws(new SocketException());
            sftpClient.Get("root-path-1/test5.txt").Throws(new SshAuthenticationException());
            sftpClient.Get("root-path-1/test6.txt").Throws(new ProxyException());

            var file = await fileSystem.GetFileAsync("prefix-1://test1.txt");

            Assert.Equal("test1.txt", file.Name);
            Assert.Equal("root-path-1/test1.txt", file.Path);
            Assert.Equal("prefix-1://test1.txt", file.VirtualPath);
            Assert.Equal(1, file.Length);
            Assert.Equal(new DateTime(1970, 1, 1), file.LastModifiedDateTime);

            await Assert.ThrowsAsync<FileNotFoundException>(() => fileSystem.GetFileAsync("prefix-1://test2.txt"));
            await Assert.ThrowsAsync<ConnectionException>(() => fileSystem.GetFileAsync("prefix-1://test3.txt"));
            await Assert.ThrowsAsync<ConnectionException>(() => fileSystem.GetFileAsync("prefix-1://test4.txt"));
            await Assert.ThrowsAsync<ConnectionException>(() => fileSystem.GetFileAsync("prefix-1://test5.txt"));
            await Assert.ThrowsAsync<ConnectionException>(() => fileSystem.GetFileAsync("prefix-1://test6.txt"));
        }

        [Fact]
        public async Task Test_Get_Directory_Async()
        {
            var sftpClient = Substitute.For<ISftpClient>();
            var sftpAdapter = new SftpAdapter("prefix-1", "root-path-1", sftpClient);
            var fileSystem = new SharpGrip.FileSystem.FileSystem(new List<IAdapter> {sftpAdapter});

            var sftpFile = Substitute.For<ISftpFile>();

            sftpFile.Name.Returns("test1");
            sftpFile.FullName.Returns("root-path-1/test1");
            sftpFile.Length.Returns(1);
            sftpFile.LastWriteTime.Returns(new DateTime(1970, 1, 1));
            sftpFile.IsDirectory.Returns(true);

            sftpClient.Get("root-path-1/test1").Returns(sftpFile);
            sftpClient.Get("root-path-1/test2").Throws(new SftpPathNotFoundException());
            sftpClient.Get("root-path-1/test3").Throws(new SshConnectionException());
            sftpClient.Get("root-path-1/test4").Throws(new SocketException());
            sftpClient.Get("root-path-1/test5").Throws(new SshAuthenticationException());
            sftpClient.Get("root-path-1/test6").Throws(new ProxyException());

            var directory = await fileSystem.GetDirectoryAsync("prefix-1://test1");

            Assert.Equal("test1", directory.Name);
            Assert.Equal("root-path-1/test1", directory.Path);
            Assert.Equal("prefix-1://test1", directory.VirtualPath);
            Assert.Equal(new DateTime(1970, 1, 1), directory.LastModifiedDateTime);

            await Assert.ThrowsAsync<DirectoryNotFoundException>(() => fileSystem.GetDirectoryAsync("prefix-1://test2"));
            await Assert.ThrowsAsync<ConnectionException>(() => fileSystem.GetDirectoryAsync("prefix-1://test3"));
            await Assert.ThrowsAsync<ConnectionException>(() => fileSystem.GetDirectoryAsync("prefix-1://test4"));
            await Assert.ThrowsAsync<ConnectionException>(() => fileSystem.GetDirectoryAsync("prefix-1://test5"));
            await Assert.ThrowsAsync<ConnectionException>(() => fileSystem.GetDirectoryAsync("prefix-1://test6"));
        }

        [Fact]
        public async Task Test_Get_Files_Async()
        {
            var sftpClient = Substitute.For<ISftpClient>();
            var sftpAdapter = new SftpAdapter("prefix-1", "root-path-1", sftpClient);
            var fileSystem = new SharpGrip.FileSystem.FileSystem(new List<IAdapter> {sftpAdapter});

            var sftpFiles = new List<ISftpFile>();
            var sftpFile = Substitute.For<ISftpFile>();
            var sftpDirectory = Substitute.For<ISftpFile>();

            sftpFiles.Add(sftpFile);
            sftpFiles.Add(sftpDirectory);

            sftpFile.Name.Returns("test1.txt");
            sftpFile.FullName.Returns("root-path-1/test1.txt");
            sftpFile.Length.Returns(1);
            sftpFile.LastWriteTime.Returns(new DateTime(1970, 1, 1));
            sftpFile.IsDirectory.Returns(false);

            sftpDirectory.Name.Returns("test1");
            sftpDirectory.FullName.Returns("root-path-1/test1");
            sftpFile.Length.Returns(1);
            sftpDirectory.LastWriteTime.Returns(new DateTime(1970, 1, 1));
            sftpDirectory.IsDirectory.Returns(true);

            sftpClient.Get("root-path-1/test1").Returns(sftpDirectory);
            sftpClient.ListDirectory("root-path-1/test1").Returns(sftpFiles);
            sftpClient.Get("root-path-1/test2").Returns(sftpDirectory);
            sftpClient.ListDirectory("root-path-1/test2").Throws(new SshConnectionException());
            sftpClient.Get("root-path-1/test3").Returns(sftpDirectory);
            sftpClient.ListDirectory("root-path-1/test3").Throws(new SocketException());
            sftpClient.Get("root-path-1/test4").Returns(sftpDirectory);
            sftpClient.ListDirectory("root-path-1/test4").Throws(new SshAuthenticationException());
            sftpClient.Get("root-path-1/test5").Returns(sftpDirectory);
            sftpClient.ListDirectory("root-path-1/test5").Throws(new ProxyException());

            var files = await fileSystem.GetFilesAsync("prefix-1://test1");
            var file = files.First();

            Assert.Equal("test1.txt", file.Name);
            Assert.Equal("root-path-1/test1.txt", file.Path);
            Assert.Equal("prefix-1://test1.txt", file.VirtualPath);
            Assert.Equal(1, file.Length);
            Assert.Equal(new DateTime(1970, 1, 1), file.LastModifiedDateTime);

            await Assert.ThrowsAsync<ConnectionException>(() => fileSystem.GetFilesAsync("prefix-1://test2"));
            await Assert.ThrowsAsync<ConnectionException>(() => fileSystem.GetFilesAsync("prefix-1://test3"));
            await Assert.ThrowsAsync<ConnectionException>(() => fileSystem.GetFilesAsync("prefix-1://test4"));
            await Assert.ThrowsAsync<ConnectionException>(() => fileSystem.GetFilesAsync("prefix-1://test5"));
        }

        [Fact]
        public async Task Test_Get_Directories_Async()
        {
            var sftpClient = Substitute.For<ISftpClient>();
            var sftpAdapter = new SftpAdapter("prefix-1", "root-path-1", sftpClient);
            var fileSystem = new SharpGrip.FileSystem.FileSystem(new List<IAdapter> {sftpAdapter});

            var sftpDirectories = new List<ISftpFile>();
            var sftpDirectory1 = Substitute.For<ISftpFile>();
            var sftpDirectory2 = Substitute.For<ISftpFile>();

            sftpDirectories.Add(sftpDirectory1);
            sftpDirectories.Add(sftpDirectory2);

            sftpDirectory1.Name.Returns("test1");
            sftpDirectory1.FullName.Returns("root-path-1/test1");
            sftpDirectory1.Length.Returns(1);
            sftpDirectory1.LastWriteTime.Returns(new DateTime(1970, 1, 1));
            sftpDirectory1.IsDirectory.Returns(true);

            sftpDirectory2.Name.Returns("test2");
            sftpDirectory2.FullName.Returns("root-path-1/test2");
            sftpDirectory2.Length.Returns(1);
            sftpDirectory2.LastWriteTime.Returns(new DateTime(1970, 1, 1));
            sftpDirectory2.IsDirectory.Returns(true);

            sftpClient.Get("root-path-1/test1").Returns(sftpDirectory1);
            sftpClient.ListDirectory("root-path-1/test1").Returns(sftpDirectories);
            sftpClient.Get("root-path-1/test2").Returns(sftpDirectory1);
            sftpClient.ListDirectory("root-path-1/test2").Throws(new SshConnectionException());
            sftpClient.Get("root-path-1/test3").Returns(sftpDirectory1);
            sftpClient.ListDirectory("root-path-1/test3").Throws(new SocketException());
            sftpClient.Get("root-path-1/test4").Returns(sftpDirectory1);
            sftpClient.ListDirectory("root-path-1/test4").Throws(new SshAuthenticationException());
            sftpClient.Get("root-path-1/test5").Returns(sftpDirectory1);
            sftpClient.ListDirectory("root-path-1/test5").Throws(new ProxyException());

            var directories = (await fileSystem.GetDirectoriesAsync("prefix-1://test1")).ToList();
            var directory1 = directories[0];
            var directory2 = directories[1];

            Assert.Equal("test1", directory1.Name);
            Assert.Equal("root-path-1/test1", directory1.Path);
            Assert.Equal("prefix-1://test1", directory1.VirtualPath);
            Assert.Equal(new DateTime(1970, 1, 1), directory1.LastModifiedDateTime);

            Assert.Equal("test2", directory2.Name);
            Assert.Equal("root-path-1/test2", directory2.Path);
            Assert.Equal("prefix-1://test2", directory2.VirtualPath);
            Assert.Equal(new DateTime(1970, 1, 1), directory2.LastModifiedDateTime);

            await Assert.ThrowsAsync<ConnectionException>(() => fileSystem.GetDirectoriesAsync("prefix-1://test2"));
            await Assert.ThrowsAsync<ConnectionException>(() => fileSystem.GetDirectoriesAsync("prefix-1://test3"));
            await Assert.ThrowsAsync<ConnectionException>(() => fileSystem.GetDirectoriesAsync("prefix-1://test4"));
            await Assert.ThrowsAsync<ConnectionException>(() => fileSystem.GetDirectoriesAsync("prefix-1://test5"));
        }

        [Fact]
        public async Task Test_File_Exists_Async()
        {
            var sftpClient = Substitute.For<ISftpClient>();
            var sftpAdapter = new SftpAdapter("prefix-1", "root-path-1", sftpClient);
            var fileSystem = new SharpGrip.FileSystem.FileSystem(new List<IAdapter> {sftpAdapter});

            var sftpFile = Substitute.For<ISftpFile>();

            sftpFile.Name.Returns("test1.txt");
            sftpFile.FullName.Returns("root-path-1/test1.txt");
            sftpFile.Length.Returns(1);
            sftpFile.LastWriteTime.Returns(new DateTime(1970, 1, 1));
            sftpFile.IsDirectory.Returns(false);

            sftpClient.Get("root-path-1/test1.txt").Returns(sftpFile);
            sftpClient.Get("root-path-1/test2.txt").Throws(new SftpPathNotFoundException());
            sftpClient.Get("root-path-1/test3.txt").Throws(new SshConnectionException());
            sftpClient.Get("root-path-1/test4.txt").Throws(new SocketException());
            sftpClient.Get("root-path-1/test5.txt").Throws(new SshAuthenticationException());
            sftpClient.Get("root-path-1/test6.txt").Throws(new ProxyException());

            Assert.True(await fileSystem.FileExistsAsync("prefix-1://test1.txt"));
            Assert.False(await fileSystem.FileExistsAsync("prefix-1://test2.txt"));

            await Assert.ThrowsAsync<ConnectionException>(() => fileSystem.FileExistsAsync("prefix-1://test3.txt"));
            await Assert.ThrowsAsync<ConnectionException>(() => fileSystem.FileExistsAsync("prefix-1://test4.txt"));
            await Assert.ThrowsAsync<ConnectionException>(() => fileSystem.FileExistsAsync("prefix-1://test5.txt"));
            await Assert.ThrowsAsync<ConnectionException>(() => fileSystem.FileExistsAsync("prefix-1://test6.txt"));
        }

        [Fact]
        public async Task Test_Directory_Exists_Async()
        {
            var sftpClient = Substitute.For<ISftpClient>();
            var sftpAdapter = new SftpAdapter("prefix-1", "root-path-1", sftpClient);
            var fileSystem = new SharpGrip.FileSystem.FileSystem(new List<IAdapter> {sftpAdapter});

            var sftpFile = Substitute.For<ISftpFile>();

            sftpFile.Name.Returns("test1");
            sftpFile.FullName.Returns("root-path-1/test1");
            sftpFile.Length.Returns(1);
            sftpFile.LastWriteTime.Returns(new DateTime(1970, 1, 1));
            sftpFile.IsDirectory.Returns(true);

            sftpClient.Get("root-path-1/test1").Returns(sftpFile);
            sftpClient.Get("root-path-1/test2").Throws(new SftpPathNotFoundException());
            sftpClient.Get("root-path-1/test3").Throws(new SshConnectionException());
            sftpClient.Get("root-path-1/test4").Throws(new SocketException());
            sftpClient.Get("root-path-1/test5").Throws(new SshAuthenticationException());
            sftpClient.Get("root-path-1/test6").Throws(new ProxyException());

            Assert.True(await fileSystem.DirectoryExistsAsync("prefix-1://test1"));
            Assert.False(await fileSystem.DirectoryExistsAsync("prefix-1://test2"));

            await Assert.ThrowsAsync<ConnectionException>(() => fileSystem.DirectoryExistsAsync("prefix-1://test3"));
            await Assert.ThrowsAsync<ConnectionException>(() => fileSystem.DirectoryExistsAsync("prefix-1://test4"));
            await Assert.ThrowsAsync<ConnectionException>(() => fileSystem.DirectoryExistsAsync("prefix-1://test5"));
            await Assert.ThrowsAsync<ConnectionException>(() => fileSystem.DirectoryExistsAsync("prefix-1://test6"));
        }

        [Fact]
        public async Task Test_Create_Directory_Async()
        {
            var sftpClient = Substitute.For<ISftpClient>();
            var sftpAdapter = new SftpAdapter("prefix-1", "root-path-1", sftpClient);
            var fileSystem = new SharpGrip.FileSystem.FileSystem(new List<IAdapter> {sftpAdapter});

            var sftpFile = Substitute.For<ISftpFile>();

            sftpFile.Name.Returns("test1");
            sftpFile.FullName.Returns("root-path-1/test1");
            sftpFile.Length.Returns(1);
            sftpFile.LastWriteTime.Returns(new DateTime(1970, 1, 1));
            sftpFile.IsDirectory.Returns(true);

            sftpClient.Get("root-path-1/test1").Returns(sftpFile);

            await Assert.ThrowsAsync<DirectoryExistsException>(() => fileSystem.CreateDirectoryAsync("prefix-1://test1"));
            await fileSystem.CreateDirectoryAsync("prefix-1://test2");
        }

        [Fact]
        public async Task Test_Delete_File_Async()
        {
            var sftpClient = Substitute.For<ISftpClient>();
            var sftpAdapter = new SftpAdapter("prefix-1", "root-path-1", sftpClient);
            var fileSystem = new SharpGrip.FileSystem.FileSystem(new List<IAdapter> {sftpAdapter});

            var sftpFile = Substitute.For<ISftpFile>();

            sftpFile.Name.Returns("test6.txt");
            sftpFile.FullName.Returns("root-path-1/test6.txt");
            sftpFile.Length.Returns(1);
            sftpFile.LastWriteTime.Returns(new DateTime(1970, 1, 1));
            sftpFile.IsDirectory.Returns(false);

            sftpClient.Get("root-path-1/test1.txt").Throws(new SftpPathNotFoundException());
            sftpClient.Get("root-path-1/test2.txt").Throws(new SshConnectionException());
            sftpClient.Get("root-path-1/test3.txt").Throws(new SocketException());
            sftpClient.Get("root-path-1/test4.txt").Throws(new SshAuthenticationException());
            sftpClient.Get("root-path-1/test5.txt").Throws(new ProxyException());
            sftpClient.Get("root-path-1/test6.txt").Returns(sftpFile);

            await Assert.ThrowsAsync<FileNotFoundException>(() => fileSystem.DeleteFileAsync("prefix-1://test1.txt"));
            await Assert.ThrowsAsync<ConnectionException>(() => fileSystem.DeleteFileAsync("prefix-1://test2.txt"));
            await Assert.ThrowsAsync<ConnectionException>(() => fileSystem.DeleteFileAsync("prefix-1://test3.txt"));
            await Assert.ThrowsAsync<ConnectionException>(() => fileSystem.DeleteFileAsync("prefix-1://test4.txt"));
            await Assert.ThrowsAsync<ConnectionException>(() => fileSystem.DeleteFileAsync("prefix-1://test5.txt"));
            await fileSystem.DeleteFileAsync("prefix-1://test6.txt");
        }

        [Fact]
        public async Task Test_Delete_Directory_Async()
        {
            var sftpClient = Substitute.For<ISftpClient>();
            var sftpAdapter = new SftpAdapter("prefix-1", "root-path-1", sftpClient);
            var fileSystem = new SharpGrip.FileSystem.FileSystem(new List<IAdapter> {sftpAdapter});

            var sftpFile = Substitute.For<ISftpFile>();

            sftpFile.Name.Returns("test6");
            sftpFile.FullName.Returns("root-path-1/test6");
            sftpFile.Length.Returns(1);
            sftpFile.LastWriteTime.Returns(new DateTime(1970, 1, 1));
            sftpFile.IsDirectory.Returns(true);

            sftpClient.Get("root-path-1/test1").Throws(new SftpPathNotFoundException());
            sftpClient.Get("root-path-1/test2").Throws(new SshConnectionException());
            sftpClient.Get("root-path-1/test3").Throws(new SocketException());
            sftpClient.Get("root-path-1/test4").Throws(new SshAuthenticationException());
            sftpClient.Get("root-path-1/test5").Throws(new ProxyException());
            sftpClient.Get("root-path-1/test6").Returns(sftpFile);

            await Assert.ThrowsAsync<DirectoryNotFoundException>(() => fileSystem.DeleteDirectoryAsync("prefix-1://test1"));
            await Assert.ThrowsAsync<ConnectionException>(() => fileSystem.DeleteDirectoryAsync("prefix-1://test2"));
            await Assert.ThrowsAsync<ConnectionException>(() => fileSystem.DeleteDirectoryAsync("prefix-1://test3"));
            await Assert.ThrowsAsync<ConnectionException>(() => fileSystem.DeleteDirectoryAsync("prefix-1://test4"));
            await Assert.ThrowsAsync<ConnectionException>(() => fileSystem.DeleteDirectoryAsync("prefix-1://test5"));
            await fileSystem.DeleteDirectoryAsync("prefix-1://test6");
        }

        [Fact]
        public async Task Test_Read_File_Stream_Async()
        {
            var sftpClient = Substitute.For<ISftpClient>();
            var sftpAdapter = new SftpAdapter("prefix-1", "root-path-1", sftpClient);
            var fileSystem = new SharpGrip.FileSystem.FileSystem(new List<IAdapter> {sftpAdapter});

            sftpClient.Get("root-path-1/test1.txt").Throws(new SftpPathNotFoundException());
            sftpClient.Get("root-path-1/test2.txt").Throws(new SshConnectionException());
            sftpClient.Get("root-path-1/test3.txt").Throws(new SocketException());
            sftpClient.Get("root-path-1/test4.txt").Throws(new SshAuthenticationException());
            sftpClient.Get("root-path-1/test5.txt").Throws(new ProxyException());

            await Assert.ThrowsAsync<FileNotFoundException>(() => fileSystem.ReadFileStreamAsync("prefix-1://test1.txt"));
            await Assert.ThrowsAsync<ConnectionException>(() => fileSystem.ReadFileStreamAsync("prefix-1://test2.txt"));
            await Assert.ThrowsAsync<ConnectionException>(() => fileSystem.ReadFileStreamAsync("prefix-1://test3.txt"));
            await Assert.ThrowsAsync<ConnectionException>(() => fileSystem.ReadFileStreamAsync("prefix-1://test4.txt"));
            await Assert.ThrowsAsync<ConnectionException>(() => fileSystem.ReadFileStreamAsync("prefix-1://test5.txt"));
        }

        [Fact]
        public async Task Test_Read_File_Async()
        {
            var sftpClient = Substitute.For<ISftpClient>();
            var sftpAdapter = new SftpAdapter("prefix-1", "root-path-1", sftpClient);
            var fileSystem = new SharpGrip.FileSystem.FileSystem(new List<IAdapter> {sftpAdapter});

            sftpClient.Get("root-path-1/test1.txt").Throws(new SftpPathNotFoundException());
            sftpClient.Get("root-path-1/test2.txt").Throws(new SshConnectionException());
            sftpClient.Get("root-path-1/test3.txt").Throws(new SocketException());
            sftpClient.Get("root-path-1/test4.txt").Throws(new SshAuthenticationException());
            sftpClient.Get("root-path-1/test5.txt").Throws(new ProxyException());

            await Assert.ThrowsAsync<FileNotFoundException>(() => fileSystem.ReadFileAsync("prefix-1://test1.txt"));
            await Assert.ThrowsAsync<ConnectionException>(() => fileSystem.ReadFileAsync("prefix-1://test2.txt"));
            await Assert.ThrowsAsync<ConnectionException>(() => fileSystem.ReadFileAsync("prefix-1://test3.txt"));
            await Assert.ThrowsAsync<ConnectionException>(() => fileSystem.ReadFileAsync("prefix-1://test4.txt"));
            await Assert.ThrowsAsync<ConnectionException>(() => fileSystem.ReadFileAsync("prefix-1://test5.txt"));
        }

        [Fact]
        public async Task Test_Read_Text_File_Async()
        {
            var sftpClient = Substitute.For<ISftpClient>();
            var sftpAdapter = new SftpAdapter("prefix-1", "root-path-1", sftpClient);
            var fileSystem = new SharpGrip.FileSystem.FileSystem(new List<IAdapter> {sftpAdapter});

            sftpClient.Get("root-path-1/test1.txt").Throws(new SftpPathNotFoundException());
            sftpClient.Get("root-path-1/test2.txt").Throws(new SshConnectionException());
            sftpClient.Get("root-path-1/test3.txt").Throws(new SocketException());
            sftpClient.Get("root-path-1/test4.txt").Throws(new SshAuthenticationException());
            sftpClient.Get("root-path-1/test5.txt").Throws(new ProxyException());

            await Assert.ThrowsAsync<FileNotFoundException>(() => fileSystem.ReadFileAsync("prefix-1://test1.txt"));
            await Assert.ThrowsAsync<ConnectionException>(() => fileSystem.ReadFileAsync("prefix-1://test2.txt"));
            await Assert.ThrowsAsync<ConnectionException>(() => fileSystem.ReadFileAsync("prefix-1://test3.txt"));
            await Assert.ThrowsAsync<ConnectionException>(() => fileSystem.ReadFileAsync("prefix-1://test4.txt"));
            await Assert.ThrowsAsync<ConnectionException>(() => fileSystem.ReadFileAsync("prefix-1://test5.txt"));
        }

        [Fact]
        public async Task Test_Write_File_Async()
        {
            var sftpClient = Substitute.For<ISftpClient>();
            var sftpAdapter = new SftpAdapter("prefix-1", "root-path-1", sftpClient);
            var fileSystem = new SharpGrip.FileSystem.FileSystem(new List<IAdapter> {sftpAdapter});

            sftpClient.OpenWrite("root-path-1/test1.txt").Throws(new SftpPathNotFoundException());
            sftpClient.OpenWrite("root-path-1/test2.txt").Throws(new SshConnectionException());
            sftpClient.OpenWrite("root-path-1/test3.txt").Throws(new SocketException());
            sftpClient.OpenWrite("root-path-1/test4.txt").Throws(new SshAuthenticationException());
            sftpClient.OpenWrite("root-path-1/test5.txt").Throws(new ProxyException());

            await Assert.ThrowsAsync<FileExistsException>(() => fileSystem.WriteFileAsync("prefix-1://test1.txt", new MemoryStream()));
            await Assert.ThrowsAsync<ConnectionException>(() => fileSystem.WriteFileAsync("prefix-1://test2.txt", new MemoryStream(), true));
            await Assert.ThrowsAsync<ConnectionException>(() => fileSystem.WriteFileAsync("prefix-1://test3.txt", new MemoryStream(), true));
            await Assert.ThrowsAsync<ConnectionException>(() => fileSystem.WriteFileAsync("prefix-1://test4.txt", new MemoryStream(), true));
            await Assert.ThrowsAsync<ConnectionException>(() => fileSystem.WriteFileAsync("prefix-1://test5.txt", new MemoryStream(), true));
        }

        [Fact]
        public async Task Test_Append_File_Async()
        {
            var sftpClient = Substitute.For<ISftpClient>();
            var sftpAdapter = new SftpAdapter("prefix-1", "root-path-1", sftpClient);
            var fileSystem = new SharpGrip.FileSystem.FileSystem(new List<IAdapter> {sftpAdapter});

            await Assert.ThrowsAsync<AdapterRuntimeException>(() => fileSystem.AppendFileAsync("prefix-1://test1.txt", new MemoryStream()));
        }
    }
}