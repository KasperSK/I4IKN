using System.Security.Cryptography.X509Certificates;

namespace TransportLayer
{
    public interface IChecksum
    {
        bool VerifyChecksum(Message message);
        void GenerateChecksum(Message message);
    }
}