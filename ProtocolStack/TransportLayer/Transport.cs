using System;
using LinkLayer;

namespace TransportLayer
{
    public enum DataType : byte
    {
        Syn = 97,
        Data = 98,
        Ack = 99
    }

    public class Message
    {

        private const int _checksumHigh = 0;
        private const int _checksumLow = 1;
        private const int _sequence = 2;
        private const int _type = 3;

        private const int HeaderSize = 4;

        public const int DataOffset = 4;

        public Message(int size)
        {
            Buffer = new byte[size + HeaderSize];
            DataSize = size;
        }

        public byte[] Buffer { get; }

        public int MessageSize
        {
            get { return DataSize + HeaderSize; }
            set { DataSize = value - HeaderSize; }
        }

        public int DataSize { get; set; }

        public ushort Checksum
        {
            get { return (ushort) ((Buffer[_checksumHigh] << 8) + Buffer[_checksumLow]); }
            set
            {
                Buffer[_checksumLow] = (byte) value;
                Buffer[_checksumHigh] = (byte) (value >> 8);
            }
        }

        public DataType DataType
        {
            get { return (DataType) Buffer[_type]; }
            set { Buffer[_type] = (byte) value; }
        }

        public byte Sequence
        {
            get { return Buffer[_sequence]; }
            set { Buffer[_sequence] = value; }
        }

        public void SetData(byte[] buffer, int offset, int length)
        {
            Array.Copy(buffer, offset, Buffer, DataOffset, length);
            DataSize = length;
        }

        public bool ValidMessageSize()
        {
            return MessageSize >= HeaderSize;
        }
    }

    public interface ITransport
    {
        void SendMessage(byte[] message, int size);
        int ReceiveMessage(byte[] message);
    }

    public class Transport : ITransport
    {
        private const int Buffer = 1004;
        private readonly ReceiverStmContext _receivingStateMachine;
        private readonly SenderStmContext _sendingStateMachine;


        public Transport(string portName, int baud, int databits)
        {
            var link = Factory.GetLink(portName, 1000, 10000);
            var checksum = new Checksum();
            _sendingStateMachine = new SenderStmContext(link, checksum, new SequenceGenerator(), 1000, 20000);
            _receivingStateMachine = new ReceiverStmContext(link, checksum, new SequenceGenerator(), 1000);
        }

        public void SendMessage(byte[] message, int size)
        {
            _sendingStateMachine.SendData(message, size);
        }

        public int ReceiveMessage(byte[] message)
        {
            return _receivingStateMachine.ReceiveData(message);
        }
    }


}