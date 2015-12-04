using System;

namespace Transport
{
    public interface IPort
    {
        void AddSocket(ISocket socket);
        void RemoveSocket(ISocket socket);
        void AddListener(ISocketListener listener);
        void RemoveListener(ISocketListener listener);

        void Run();
        void Open();
        void Close();
    }
}