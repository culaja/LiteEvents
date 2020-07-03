using System;
using System.Diagnostics;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Threading;

namespace AppendToFileTestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            unsafe
            {
                var info = args[0] == "one" ? 0x11 : 0x22;
            
                var data = Enumerable.Range(0, 4).Select(_ => (byte)info).ToArray();

                using var mmf = Open();
                using var view = mmf.CreateViewAccessor(0, 8);
                var positionInStream = view.SafeMemoryMappedViewHandle.DangerousGetHandle();
                
            
                using var fileWriter = new BinaryWriter(File.Open("myfile.bin", FileMode.OpenOrCreate, FileAccess.Write, FileShare.Write));
                var i = 0;
                var sw = new Stopwatch();
                sw.Start();
                while (true)
                {
                    var newPosition = Interlocked.Add(ref *(long*) positionInStream, data.Length);
                    try
                    {
                        fileWriter.BaseStream.Seek(newPosition - data.Length, SeekOrigin.Begin);
                        fileWriter.Write(data);
                        fileWriter.Flush();
                    }
                    catch (IOException)
                    {
                        Console.WriteLine(newPosition);
                        Console.WriteLine(int.MaxValue);
                        throw;
                    }

                    if (++i % 100000 == 0)
                    {
                        sw.Stop();
                        Console.WriteLine(sw.ElapsedMilliseconds / 1000m);
                        sw.Restart();
                    }
                        
                }
            }
        }

        private static MemoryMappedFile Open()
        {
            try
            {
                return MemoryMappedFile.OpenExisting("control", MemoryMappedFileRights.ReadWrite,HandleInheritability.Inheritable);
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("Ops");
                return CreateFile();
            }
        }

        private static MemoryMappedFile CreateFile()
        {
            var controlStream = new FileStream("control", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
            return MemoryMappedFile.CreateFromFile(controlStream, "control", 8, MemoryMappedFileAccess.ReadWrite, HandleInheritability.Inheritable, false);
        }
    }
}