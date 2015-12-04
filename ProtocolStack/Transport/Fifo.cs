using System.Threading;

namespace Transport
{
    public class Fifo : IFifo
    {
        private class FifoPointer
        {
            private readonly int _max;

            public FifoPointer(int size)
            {
                _max = size;
                Position = 0;
            }

            public int Position { get; set; }

            public static FifoPointer operator ++(FifoPointer pointer)
            {
                ++pointer.Position;
                if (pointer.Position == pointer._max)
                    pointer.Position = 0;
                return pointer;
            }
        }

        private readonly FifoPointer _back;
        private readonly byte[] _buffer;
        private readonly ManualResetEvent _bufferNotFullEvent;
        private readonly FifoPointer _front;
        private readonly object _lock;

        public Fifo(int buffersize)
        {
            _buffer = new byte[buffersize];
            _front = new FifoPointer(buffersize);
            _back = new FifoPointer(buffersize);
            RemainingBufferSize = buffersize;
            _lock = new object();
            _bufferNotFullEvent = new ManualResetEvent(true);
        }

        public bool Empty => RemainingBufferSize == _buffer.Length;

        public bool Full => RemainingBufferSize == 0;

        public void Write(byte[] buffer, int offset, int count)
        {
            var i = offset;
            while (true)
            {
                _bufferNotFullEvent.WaitOne();
                lock (_lock)
                {
                    while (!Full)
                    {
                        Write(buffer[i]);
                        ++i;
                        if (i == offset + count)
                            return;
                    }
                }
                _bufferNotFullEvent.Reset();
            }
        }

        public int Read(byte[] buffer, int offset, int count)
        {
            int actualRead;
            lock (_lock)
            {
                for (actualRead = 0; actualRead < count; actualRead++)
                {
                    if (Empty)
                        break;
                    buffer[actualRead + offset] = Read();
                }
            }
            _bufferNotFullEvent.Set();
            return actualRead;
        }

        public int RemainingBufferSize { get; private set; }

        private byte Read()
        {
            var data = _buffer[_back.Position];
            ++_back.Position;
            ++RemainingBufferSize;
            return data;
        }

        private void Write(byte data)
        {
            _buffer[_front.Position] = data;
            ++_front.Position;
            --RemainingBufferSize;
        }
    }
}