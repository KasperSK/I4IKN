namespace Transport
{
    public interface ISocketListener : IMessageReceiver
    {
        // User
        void Start();
        void Stop();
        ISocket AcceptSocket();
    }
}