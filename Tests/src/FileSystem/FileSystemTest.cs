using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using SharpGrip.FileSystem.Adapters;
using SharpGrip.FileSystem.Exceptions;
using SharpGrip.FileSystem.Models;
using Xunit;

namespace Tests.FileSystem
{
    public class FileSystemTest
    {
        [Fact]
        public void Test_Instantiation()
        {
            var localAdapter1 = new LocalAdapter("test1", "/");
            var localAdapter2 = new LocalAdapter("test2", "/");
            var localAdapter3 = new LocalAdapter("test3", "/");

            var fileSystem1 = new SharpGrip.FileSystem.FileSystem();

            Assert.Equal(0, fileSystem1.Adapters.Count);

            var adapters = new List<IAdapter>
            {
                localAdapter1,
                localAdapter2,
                localAdapter3
            };

            var fileSystem2 = new SharpGrip.FileSystem.FileSystem(adapters);

            Assert.Equal(3, fileSystem2.Adapters.Count);
            Assert.Equal(localAdapter1, fileSystem2.Adapters[0]);
            Assert.Equal(localAdapter2, fileSystem2.Adapters[1]);
            Assert.Equal(localAdapter3, fileSystem2.Adapters[2]);

            var fileSystem3 = new SharpGrip.FileSystem.FileSystem();
            fileSystem3.Adapters.Add(localAdapter1);

            Assert.Equal(1, fileSystem3.Adapters.Count);
            Assert.Equal(localAdapter1, fileSystem2.Adapters[0]);

            fileSystem3.Adapters.Add(localAdapter2);

            Assert.Equal(2, fileSystem3.Adapters.Count);
            Assert.Equal(localAdapter1, fileSystem2.Adapters[0]);
            Assert.Equal(localAdapter2, fileSystem2.Adapters[1]);

            fileSystem3.Adapters.Add(localAdapter3);

            Assert.Equal(3, fileSystem3.Adapters.Count);
            Assert.Equal(localAdapter1, fileSystem2.Adapters[0]);
            Assert.Equal(localAdapter2, fileSystem2.Adapters[1]);
            Assert.Equal(localAdapter3, fileSystem2.Adapters[2]);
        }

        [Fact]
        public void Test_Get_Adapter()
        {
            var fileSystem = new SharpGrip.FileSystem.FileSystem();
            var localAdapter = new LocalAdapter("test", "/");

            Assert.Equal(0, fileSystem.Adapters.Count);
            Assert.Throws<NoAdaptersRegisteredException>(() => fileSystem.GetAdapter("test"));

            fileSystem.Adapters.Add(localAdapter);

            Assert.Equal(1, fileSystem.Adapters.Count);
            Assert.Equal(localAdapter, fileSystem.GetAdapter("test"));
            Assert.Throws<AdapterNotFoundException>(() => fileSystem.GetAdapter("test-test"));

            fileSystem.Adapters.Add(localAdapter);
            Assert.Equal(2, fileSystem.Adapters.Count);
            Assert.Throws<DuplicateAdapterPrefixException>(() => fileSystem.GetAdapter("test"));
        }

        [Fact]
        public void Test_Get_File()
        {
            var fileSystem = new SharpGrip.FileSystem.FileSystem();

            var localAdapter = new Mock<LocalAdapter>("test1", "/");
            localAdapter.SetupAllProperties();

            var fileModel = new FileModel
            {
                Name = "test"
            };

            localAdapter.Setup(o => o.GetFileAsync("test.txt", CancellationToken.None)).Returns(Task.FromResult<IFile>(fileModel));

            var adapters = new List<IAdapter>
            {
                localAdapter.Object
            };

            fileSystem.Adapters = adapters;

            Assert.True(fileSystem.DirectoryExists("test1://test.txt"));
            Assert.Equal(fileModel, fileSystem.GetFileAsync("test1://test.txt", CancellationToken.None).Result);
        }
    }
}