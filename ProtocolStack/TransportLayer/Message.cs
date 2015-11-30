using System;

namespace TransportLayer
{
    public class Message
    {

        private const int ChecksumHighPos = 0;
        private const int ChecksumLowPos = 1;
        private const int SequencePos = 2;
        private const int TypePos = 3;

        public const int DataOffset = 4;
        public const int HeaderSize = 4;

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
            get { return (ushort) ((Buffer[ChecksumHighPos] << 8) + Buffer[ChecksumLowPos]); }
            set
            {
                Buffer[ChecksumLowPos] = (byte) value;
                Buffer[ChecksumHighPos] = (byte) (value >> 8);
            }
        }

        public DataType DataType
        {
            get { return (DataType) Buffer[TypePos]; }
            set { Buffer[TypePos] = (byte) value; }
        }

        public byte Sequence
        {
            get { return Buffer[SequencePos]; }
            set { Buffer[SequencePos] = value; }
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
}