namespace LinkLayer
{
    public interface IPhysical
    {
        void Write(byte[] buffer, int buffersize);
        void EnableTimeout();
        void DisableTimeout();
        void ClearBuffer();
        byte Read();
        int Timeout { get; }
        void Open();
        void Close();
    }
}