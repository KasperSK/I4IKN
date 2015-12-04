using System.Collections.Generic;
using System.IO.Ports;
using log4net;
using LinkLayer;
using Transport;

namespace Transport
{
    public class PortController : IPortController
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(PortController));
        private readonly Dictionary<int, IPort> _ports;

        public PortController()
        {
            _ports = new Dictionary<int, IPort>();
            Logger.Debug("Making a new PortController");
        }
        public IPort GetPort(int comPort)
        {
            if (_ports.ContainsKey(comPort))
            {
                Logger.Debug("Using already working Port: " + comPort);
                return _ports[comPort];
            }

            Logger.Debug("Creating new Port: " + comPort);
            var link = Factory.GetLink(new SerialPort("COM" + comPort, 115200, Parity.None, 8, StopBits.One), 1010, 1000 );
            IPort port = new Port(link);
            _ports[comPort] = port;
            port.Open();
            port.Run();
            return port;
        }
    }
}