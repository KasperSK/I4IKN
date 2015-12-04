using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Transport
{
    public class SocketListener : ISocketListener
    {
        private readonly IPortController _portController;
        private readonly Queue<Message> _messages;
        private IPort _port;
        private object _lock;
        private int _comPort;
        private ManualResetEvent _gotMessageEvent;
        public SocketListener(int comPort, byte port, IPortController portController)
        {
            DestinationPort = port;
            _portController = portController;
            _messages = new Queue<Message>();
            _lock = new object();
            _comPort = comPort;
            _gotMessageEvent = new ManualResetEvent(false);
        }
        public void PutMessage(Message message)
        {
            lock (_lock)
            {
                _messages.Enqueue(message);
            }
            _gotMessageEvent.Set();
        }

        public byte DestinationPort { get; set; }
        public void Start()
        {
            _port = _portController.GetPort(_comPort);
            _port.AddListener(this);
        }

        public void Stop()
        {
            _port.RemoveListener(this);
            _port = null;
        }

        public ISocket AcceptSocket()
        {
            Message message;
            _gotMessageEvent.WaitOne();
            lock (_lock)
            {
                message = _messages.Dequeue();
                if (_messages.Count == 0)
                    _gotMessageEvent.Reset();
            }
            ISocket socket = new Socket(1000, _portController, sourcePort:message.DestinationPort, destPort:message.SourcePort, port:_port);
            socket.PutMessage(message);
            return socket;
        }
    }
}