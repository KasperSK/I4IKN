using System;
using System.IO.Ports;
using LinkLayer;

namespace TransportLayer
{
    public class Transport : ITransport
    {
        private IReceiver _receiver;
        private ISender _sender;
        private readonly IChecksum _checksum;
        private ILink _link;
        private bool _connected;
        private bool _synced;

        public bool Synced { get { return _synced;} set { _synced = value; } }

        public const int MaxMessageDataSize = 1000;
        private const int MessageTimeoutModifier = 1;

        private const int LinkBufferSize = MaxMessageDataSize + Message.HeaderSize;
        private const int LinkTimeoutModifier = 1;

        public Transport()
        {
            _connected = false;
            _synced = false;
            _checksum = new Checksum();
        }

        public void SendMessage(byte[] message, int size)
        {
            if (!_connected)
                throw new ApplicationException();
            if (!_synced)
            {
                _sender.SyncUp();
                _synced = true;
            }
            _sender.SendData(message, size);
        }

        public int ReceiveMessage(byte[] message, int size)
        {
            if (!_connected)
                throw new ApplicationException();
            return _receiver.ReceiveData(message, size);
        }

        public void Connect(string portName, int baud, int databits)
        {
            var port = new SerialPort(portName, baud, Parity.None, databits, StopBits.One);
            _link = Factory.GetLink(port, LinkBufferSize, LinkTimeoutModifier);
            _sender = new SenderStmContext(_link, _checksum, new SequenceGenerator(), MaxMessageDataSize, MessageTimeoutModifier);
            _receiver = new ReceiverStmContext(_link, _checksum, new SequenceGenerator(), MaxMessageDataSize);
            _connected = true;
        }

        public void Disconnect()
        {
            _sender = null;
            _receiver = null;
            _link.Close();
            _link = null;
            _connected = false;
        }
    }
}