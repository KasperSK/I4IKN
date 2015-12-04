using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using log4net;
using LinkLayer;

namespace Transport
{
    public class PortTransmitter
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(PortReceiver));
        private readonly ILink _link;
        private readonly List<ISocket> _senders;
        private object _lock;

        public PortTransmitter(ILink link)
        {
            _link = link;
            _lock = new object();
            _senders = new List<ISocket>();
        }

        public void AddSender(ISocket socket)
        {
            lock (_lock)
            {
                _senders.Add(socket);
            }
            
        }

        public void RemoveSender(ISocket socket)
        {
            lock (_lock)
            {
                _senders.Remove(socket);
            }
            
        }

        public void Run()
        {
            while (true)
            {
                lock (_lock)
                {
                    foreach (var sender in _senders)
                    {
                        var message = sender.GetMessage();
                        if (message != null)
                        {
                            _link.SendMessage(message.Buffer, message.Size);
                        }
                    }
                }
                
                Thread.Sleep(10);
            }
        }
    }
}