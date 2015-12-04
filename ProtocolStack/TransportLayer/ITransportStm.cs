namespace TransportLayer
{
    public interface ITransportStm
    {
        void SendData(byte[] buffer, int size);
        int ReceiveData(byte[] buffer, int size);
    }
}