using System.Collections.Generic;
using log4net;
using LinkLayer;

namespace Transport
{
    public class PortReceiver
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof (PortReceiver));
        private readonly ILink _link;
        private readonly Dictionary<byte, IMessageReceiver> _listeners;
        private readonly Dictionary<ushort, IMessageReceiver> _receivers;
        private readonly object _lock;

        public PortReceiver(ILink link)
        {
            _link = link;
            _receivers = new Dictionary<ushort, IMessageReceiver>();
            _listeners = new Dictionary<byte, IMessageReceiver>();
            _lock = new object();
        }

        public void AddSocket(ISocket socket)
        {
            lock (_lock)
            {
                _receivers.Add(socket.PortId, socket);
            }
            Logger.Debug($"Added Receiver: {socket.PortId:X}");
        }

        public void RemoveSocket(ISocket socket)
        {
            lock (_lock)
            {
                _receivers.Remove(socket.PortId);
            }
        }

        public void AddListener(ISocketListener listener)
        {
            lock (_lock)
            {
                _listeners.Add(listener.DestinationPort, listener);
            }
        }

        public void RemoveListener(ISocketListener listener)
        {
            lock (_lock)
            {
                _listeners.Remove(listener.DestinationPort);
            }
        }

        public void Run()
        {
            // Theres still people using me
            while (true)
            {
                var message = new Message(1000);
                while (!message.IsValid())
                {
                    // Get new message
                    message.Size = _link.GetMessage(message.Buffer);
                }
                DeliverMessage(message);
            }
        }

        private void DeliverMessage(Message message)
        {
            IMessageReceiver recepiant;

            // First see if we got a connection already
            lock (_lock)
            {
                Logger.Debug($"Looking for: {message.PortId:X}");
                Logger.Debug($"Looking for Dest: {message.DestinationPort:X}");

                foreach (var key in _receivers.Keys)
                {
                    Logger.Debug($"Have: {key:X}");
                }
                

                if (_receivers.TryGetValue(message.PortId, out recepiant))
                {
                    Logger.Debug("PrevConnection");
                    recepiant.PutMessage(message);
                    return;
                }
            }

            // If not test for listening
            lock (_lock)
            {
                if (_listeners.TryGetValue(message.DestinationPort, out recepiant))
                {
                    Logger.Debug("New Connection");
                    recepiant.PutMessage(message);
                    return;
                }
            }

            // We got a mesaage with a recepiant
            Logger.Debug($"Message with invalid recepient: {message.PortId:X}");
            
        }

    }
}