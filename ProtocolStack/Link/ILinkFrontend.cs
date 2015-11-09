namespace Link
{
    public interface ILinkFrontend
    {
        void SendMessage(byte[] msg);
        byte[] GetMessage();
    }
}