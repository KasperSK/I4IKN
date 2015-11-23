namespace LinkLayer
{
    public interface IPhysical
    {
        int InfiniteTimeout { get; }
        void Write(byte[] buffer, int buffersize);
        int Read(byte[] buffer, int buffersize, int timeout);
    }
}