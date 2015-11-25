namespace TransportLayer
{
    public interface ITransport
    {
        void SendMessage(byte[] message, int size);
        int ReceiveMessage(byte[] message);
        void Connect(string port, int baud, int databits);
        void Disconnect();
    }
}