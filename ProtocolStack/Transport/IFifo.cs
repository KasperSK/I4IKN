namespace Transport
{
    public interface IFifo
    {
        // Blocking
        void Write(byte[] buffer, int offset, int count);

        // NonBlocking
        int Read(byte[] buffer, int offset, int count);

        int RemainingBufferSize { get; }

        bool Empty { get; }

        bool Full { get; }
    }
}