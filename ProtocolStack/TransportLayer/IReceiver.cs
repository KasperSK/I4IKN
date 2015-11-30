namespace TransportLayer
{
    public interface IReceiver
    {
        int ReceiveData(byte[] buffer, int size);
    }
}