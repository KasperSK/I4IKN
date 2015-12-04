namespace LinkLayer
{
    public interface ILink
    {
        void SendMessage(byte[] msg, int length);
        int GetMessage(byte[] buffer);
        int Timeout { get; }
        void Open();
        void Close();
    }
}