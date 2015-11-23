namespace LinkLayer
{
    public interface IPhysical
    {
        void Write(byte[] buffer, int buffersize);
        void EnableTimeout();
        void DisableTimeout();
        byte Read();
    }
}