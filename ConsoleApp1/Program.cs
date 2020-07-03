using System;
using System.IO.MemoryMappedFiles;
using System.Threading;
using LiteEvents;

namespace ConsoleApp1
{
    class Program
    {
        private static MemoryMappedFile _mmf;
        private static MemoryMappedViewStream _mmvs;

        static void Main(string[] args)
        {
            using (var memoryMappedFile = MemoryMappedFile.CreateOrOpen("test", 1024*1024))
            {
                var endlessStream = new EndlessStream(memoryMappedFile);

                for (int i = 0; i < 1000; ++i)
                {
                    endlessStream.AppendBlock(new byte[32]);
                }
            }
        }
    }
}