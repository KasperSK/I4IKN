namespace LinkLayer
{
    public interface IPhysical
    {
        void Write(byte[] buffer, int buffersize);
        int Read(byte[] buffer, int buffersize);
    }
}