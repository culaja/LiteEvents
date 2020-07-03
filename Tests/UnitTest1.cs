using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
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

        [Theory]
        [InlineData(1000)]
        [InlineData(10000)]
        [InlineData(100000)]
        [InlineData(1000000)]
        [InlineData(10000000)]
        public void Test2(int numberOfEvents)
        {   
            using var fileWriter = new BinaryWriter(File.Open("testfile.bin", FileMode.Create));
            var data = Enumerable.Range(0, 1024).Select(_ => (byte)0xa2).ToArray();
            for (var i = numberOfEvents; i > 1; --i)
            {
                fileWriter.Seek(i * 1024 - 1024, SeekOrigin.Begin);
                fileWriter.Write(data);
                fileWriter.Flush();
            }
        }
    }
}