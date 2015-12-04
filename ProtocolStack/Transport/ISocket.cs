namespace Transport
{
    public interface ISocket : IMessageReceiver
    {
        void Connect(int comPort, byte port);
        void Close();
        int Send(byte[] buffer);
        int Receive(byte[] buffer);


        Message GetMessage();

        byte SourcePort { get; set; }

        ushort PortId { get; }

        void Reset();
    }
}