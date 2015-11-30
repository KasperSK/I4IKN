using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkLayer
{
    public class Serial : IPhysical
    {

        private readonly SerialPort _port;
        private readonly byte[] _buffer;
        private int _bufferEnd;
        private int _bufferPtr;

        public Serial(SerialPort port, int bufferSize, int timeoutmodifier)
        {
            _bufferEnd = 0;
            _bufferPtr = 0;
            _port = port;
            Timeout = timeoutmodifier * (20000 / (port.BaudRate / 12));
            _buffer = new byte[bufferSize];
            
            _port.Open();
        }

        public void Write(byte[] buffer, int buffersize)
        {
            _port.Write(buffer, 0, buffersize);
        }

        public void EnableTimeout()
        {
            _port.ReadTimeout = Timeout;
        }

        public void DisableTimeout()
        {
            _port.ReadTimeout = SerialPort.InfiniteTimeout;
        }

        public byte Read()
        {
            if (_bufferPtr == _bufferEnd)
            {
                _bufferPtr = 0;
                _bufferEnd = _port.Read(_buffer, 0, _buffer.Length);
            }
            return _buffer[_bufferPtr++];
        }

        public int Timeout { get; }
        public void Close()
        {
            _port.Close();
        }

        /*
        public int Read(byte[] buffer, int buffersize)
        {
           Task<int> x = ReadAsync(buffer, buffersize);
            return x.Result;
        }
        
        private async Task<int> ReadAsync(byte[] buffer, int buffersize)
        {
            var stream = _port.BaseStream;
            return await stream.ReadAsync(buffer, 0, buffersize);
        }
        */
    }
}
