namespace TransportLayer
{
    public interface ISender
    {
        void SendData(byte[] buffer, int size);

        void SyncUp();

    }
}