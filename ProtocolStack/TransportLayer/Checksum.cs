namespace TransportLayer
{
    public class Checksum : IChecksum
    {
        private static ushort CalculateChecksum(Message message)
        {
            ushort sum = 0;
            // Skip checksum in call (not pretty)
            for (var i = 2; i < message.MessageSize; i++)
            {
                sum += message.Buffer[i];
            }
            return sum;
        }
        
        public bool VerifyChecksum(Message message)
        {
            return CalculateChecksum(message) == message.Checksum;
        }

        public void GenerateChecksum(Message message)
        {
            message.Checksum = CalculateChecksum(message);
        }
    }
}