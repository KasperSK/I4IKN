using System;

namespace Transport
{
    public enum DataType : byte
    {
        Syn = 97,
        SynAck,
        Ack,
        Fin,
        FinAck,
    }

    public class Message
    {

        public Message(int size)
        {
            Buffer = new byte[size+HeaderSize];
        }
        public const int HeaderSize = 10;

        private void SetUshort(int position, ushort value)
        {
            Buffer[position] = (byte) (value >> 8);
            Buffer[position + 1] = (byte) (value);
        }

        private ushort GetUshort(int position)
        {
            int value = Buffer[position];
            value = value << 8;
            value += Buffer[position + 1];
            return (ushort) value;
        }

        public byte DestinationPort { get { return Buffer[0]; } set { Buffer[0] = value; } }

        public byte SourcePort { get { return Buffer[1]; } set { Buffer[1] = value; } }

        public ushort Seq
        {
            get { return GetUshort(2); }
            set { SetUshort(2,value); }
        }

        public ushort Ack {
            get { return GetUshort(4); }
            set { SetUshort(4, value); }
        }

        public byte Window { get { return Buffer[6]; } set { Buffer[6] = value; } }

        public ushort Checksum {
            get { return GetUshort(7); }
            set { SetUshort(7, value); }
        }

        public DataType Type { get { return (DataType)Buffer[9]; } set { Buffer[9] = (byte)value; } }

        public byte[] Data { get; set; }

        public byte[] Buffer { get; set; }

        public int Size { get; set; }

        public bool HasBeenTransmitted { get; set; }

        public ushort PortId
        {
            get { return GetUshort(0); }
            set { SetUshort(0, value); }
        }

        public bool IsValid()
        {
            return HasValidSize() && HasValidChecksum();
        }

        public bool HasValidSize()
        {
            return Size >= HeaderSize;
        }

        public bool HasValidChecksum()
        {
            return Checksum == 0x6565;
        }

        public void GenerateChecksum()
        {
            Checksum = 0x6565;
        }
    }
}