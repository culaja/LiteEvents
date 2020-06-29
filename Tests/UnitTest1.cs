using System.IO.MemoryMappedFiles;
using LiteEvents;
using Xunit;

namespace Tests
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            using (var memoryMappedFile = MemoryMappedFile.CreateOrOpen("test", 1024*1024))
            using (var endlessStream = new EndlessStream(memoryMappedFile))
            {
                for (int i = 0; i < 1000; ++i)
                {
                    endlessStream.AppendBlock(new byte[32]);
                }
            }
        }
    }
}