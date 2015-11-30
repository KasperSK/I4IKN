namespace TransportLayer
{
    public enum DataType : byte
    {
        Syn = 97,
        Data = 98,
        Ack = 99
    }

    public interface ITransport
    {
        void SendMessage(byte[] message, int size);
        int ReceiveMessage(byte[] message, int size);
        void Connect(string port, int baud, int databits);
        void Disconnect();
        bool Synced { get; set; }
    }
}