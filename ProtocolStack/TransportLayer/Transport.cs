using System;
using LinkLayer;

namespace TransportLayer
{
    public enum DataType
    {
        Data = 0,
        Ack = 1
    }

    public enum HeaderPosition
    {
        ChecksumHigh = 0,
        ChecksumLow = 1,
        Sequence = 2,
        Type = 3
    }

    public enum Sequence
    {
        Zero = 0,
        One = 1
    }

    public interface ITransport
    {
        void SendMessage(byte[] message, int size);
        int ReceiveMessage(byte[] message);
    }

    public class Transport : ITransport
    {
        private const int Buffer = 1004;
        private readonly SenderStmContext _sendingStateMachine;
        private readonly ReceiverStmContext _receivingStateMachine;


        public Transport(string portName, int baud, int databits)
        {
            ILink link = new Link(Buffer, new DecryptStm(), new EncryptStm(), new Serial(portName, baud, databits));
            _sendingStateMachine = new SenderStmContext(link);
            _receivingStateMachine = new ReceiverStmContext(link);
        }

        public void SendMessage(byte[] message, int size)
        {
            _sendingStateMachine.SendMessage(message, size);
        }

        public int ReceiveMessage(byte[] message)
        {
            return _receivingStateMachine.ReceiveMessage(message);
        }
    }

    public class Checksum
    {
        private ushort MakeCheckSum(byte[] buffer)
        {
            ushort sum = 0;
            for (int i = 0; i < buffer.Length; i++)
            {
                sum += buffer[i];
            }

            return sum;
        }

        public bool VerifyCheckSum(byte[] buffer)
        {
            byte[] internalBuffer = new byte[buffer.Length - 2];

            Array.Copy(buffer, 2, internalBuffer, 0, internalBuffer.Length);
            int sum = (buffer[0] << 8) | buffer[1];

            return MakeCheckSum(internalBuffer) == (ushort)sum;
        }

        public void CalculateCheckSum(byte[] buffer)
        {
            byte[] internalBuffer = new byte[buffer.Length - 2];

            Array.Copy(buffer, 2, internalBuffer, 0, internalBuffer.Length);

            var chsum = MakeCheckSum(internalBuffer);

            buffer[0] = (byte)((chsum >> 8) & 0xFF);
            buffer[1] = (byte)(chsum & 0xFF);
        }
    }
}