namespace LinkLayer
{
    public interface IDecrypt
    {
        int BufferSize { get; }
        bool ParseByte(byte b);
        void NewMessage(byte[] buffer);
        void Reset();
    }
}