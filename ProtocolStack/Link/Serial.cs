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
        private readonly int _bufferSize;
        private readonly byte[] _buffer;
        private int _bufferEnd;
        private int _bufferPtr;
        private readonly int _timeout;

        public Serial(string portName, int baud, int databits, int bufferSize, int timeout)
        {
            _bufferSize = bufferSize;
            _bufferEnd = 0;
            _bufferPtr = 0;
            _timeout = timeout;
            _buffer = new byte[bufferSize];
            _port = new SerialPort(portName, baud, Parity.None, databits, StopBits.One);
            _port.Open();
        }

        public void Write(byte[] buffer, int buffersize)
        {
            _port.Write(buffer, 0, buffersize);
        }

        public void EnableTimeout()
        {
            _port.ReadTimeout = _timeout;
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
                _bufferEnd = _port.Read(_buffer, 0, _bufferSize);
            }
            return _buffer[_bufferPtr++];
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
