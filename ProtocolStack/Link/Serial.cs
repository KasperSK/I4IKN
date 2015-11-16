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
        }
        public void Write(byte[] buffer, int buffersize)
        {
            _port.Write(buffer, 0, buffersize);
        }

        public int Read(byte[] buffer, int buffersize)
        {
            return _port.Read(buffer, 0, buffersize);
        }
    }
}
