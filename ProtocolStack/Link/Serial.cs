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

        private SerialPort _port;

        public Serial(string portName, int baud, int databits)
        {
            _port = new SerialPort(portName, baud, Parity.None, databits, StopBits.One);
            _port.Open();
        }

        public int InfiniteTimeout => SerialPort.InfiniteTimeout;

        public void Write(byte[] buffer, int buffersize)
        {
            _port.Write(buffer, 0, buffersize);
        }

        public int Read(byte[] buffer, int buffersize, int timeout)
        {
            if (timeout == -1)
                timeout = System.IO.Ports.SerialPort.InfiniteTimeout;
            return _port.Read(buffer, 0, buffersize);
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
