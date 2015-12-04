using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using LinkLayer;

namespace Transport
{
    public class Port : IPort
    {
        private readonly ILink _link;
        
        private readonly PortTransmitter _transmitter;
        private readonly PortReceiver _receiver;
        private readonly byte[] _usedPorts; 

        public Port(ILink link)
        {
            _link = link;
            _receiver = new PortReceiver(_link);
            _transmitter = new PortTransmitter(_link);
            _usedPorts = new byte[byte.MaxValue];
        }

        public void Open()
        {
            _link.Open();
        }

        public void Close()
        {
            _link.Close();
        }

        public void Run()
        {
            var receiverThread = new Thread(_receiver.Run);
            var transmitterThread = new Thread(_transmitter.Run);
            receiverThread.Start();
            transmitterThread.Start();
        }

        public void AddSocket(ISocket socket)
        {
            if (socket.SourcePort == 0)
            {
                for (byte i = 1; i < _usedPorts.Length; i++)
                {
                    if (_usedPorts[i] != 0) continue;
                    socket.SourcePort = i;
                }
            }
            _usedPorts[socket.SourcePort] = 1;
            _receiver.AddSocket(socket);
            _transmitter.AddSender(socket);
        }

        public void RemoveSocket(ISocket socket)
        {
            if (_usedPorts[socket.SourcePort] == 1)
                _usedPorts[socket.SourcePort] = 0;
            _receiver.RemoveSocket(socket);
            _transmitter.RemoveSender(socket);
        }

        public void AddListener(ISocketListener listener)
        {
            _usedPorts[listener.DestinationPort] = 2;
            _receiver.AddListener(listener);
        }

        public void RemoveListener(ISocketListener listener)
        {
            _usedPorts[listener.DestinationPort] = 0;
            _receiver.RemoveListener(listener);
        }


    }
}