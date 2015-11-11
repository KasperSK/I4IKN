namespace LinkLayer
{
    public interface IEncrypt
    {
        void ParseByte(byte b);
        int GetEncryptedMessage(out byte[] buffer);
        void NewMessage(int length);
    }
}