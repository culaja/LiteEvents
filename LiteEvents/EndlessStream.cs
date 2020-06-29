using System;
using System.IO.MemoryMappedFiles;
using System.Threading;

namespace LiteEvents
{
    public sealed class EndlessStream : IDisposable
    {
        private readonly MemoryMappedFile _memoryMappedFile;
        private readonly MemoryMappedViewAccessor _startingFrameViewAccessor;
        private MemoryMappedViewAccessor _appendFrameViewAccessor;

        private readonly IntPtr _positionCounter;

        public EndlessStream(MemoryMappedFile memoryMappedFile)
        {
            _memoryMappedFile = memoryMappedFile;
            _startingFrameViewAccessor = _memoryMappedFile.CreateViewAccessor(0, 8);
            _appendFrameViewAccessor = _memoryMappedFile.CreateViewAccessor(8, 1024);
            
            _positionCounter = _startingFrameViewAccessor.SafeMemoryMappedViewHandle.DangerousGetHandle();
        }

        public unsafe void AppendBlock(byte[] block)
        {
            var newPosition = Interlocked.Add(ref *(long*) _positionCounter, block.Length);
            
            ReallocateAppendViewAccessor(newPosition);

            var pointer = _appendFrameViewAccessor.SafeMemoryMappedViewHandle.DangerousGetHandle();
            var pointerOffset = newPosition + 8 - _appendFrameViewAccessor.PointerOffset - block.Length;
            var writeOffset = (long)pointer + pointerOffset + 8;

            fixed (void* sourceBuffer = block)
            {
                Buffer.MemoryCopy(sourceBuffer, (void*)writeOffset, block.Length, block.Length);    
            }
        }

        private void ReallocateAppendViewAccessor(long newPosition)
        {
            if (_appendFrameViewAccessor.PointerOffset + _appendFrameViewAccessor.Capacity - newPosition < 0)
            {
                _appendFrameViewAccessor.Dispose();
                _appendFrameViewAccessor = _memoryMappedFile.CreateViewAccessor(8 + _appendFrameViewAccessor.PointerOffset + _appendFrameViewAccessor.Capacity, 1024);
            }
        }

        public void Dispose()
        {
            _appendFrameViewAccessor.Dispose();
            _startingFrameViewAccessor.Dispose();
            _memoryMappedFile.Dispose();
        }
    }
}