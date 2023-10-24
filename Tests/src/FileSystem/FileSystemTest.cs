using System.Collections.Generic;
using SharpGrip.FileSystem.Adapters;
using SharpGrip.FileSystem.Exceptions;
using Xunit;

namespace SharpGrip.FileSystem.Tests.FileSystem
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

            Assert.Empty(fileSystem1.Adapters);

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

            Assert.Single(fileSystem3.Adapters);
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
            var localAdapter = new LocalAdapter("prefix-1", "/");

            Assert.Empty(fileSystem.Adapters);
            Assert.Throws<NoAdaptersRegisteredException>(() => fileSystem.GetAdapter("prefix-1"));

            fileSystem.Adapters.Add(localAdapter);

            Assert.Single(fileSystem.Adapters);
            Assert.Equal(localAdapter, fileSystem.GetAdapter("prefix-1"));
            Assert.Throws<AdapterNotFoundException>(() => fileSystem.GetAdapter("prefix-2"));

            fileSystem.Adapters.Add(localAdapter);
            Assert.Equal(2, fileSystem.Adapters.Count);
            Assert.Throws<DuplicateAdapterPrefixException>(() => fileSystem.GetAdapter("prefix-3"));
        }
    }
}