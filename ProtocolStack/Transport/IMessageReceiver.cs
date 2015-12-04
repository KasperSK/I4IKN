namespace Transport
{
    public interface IMessageReceiver
    {
        void PutMessage(Message message);

        byte DestinationPort { get; set; }
    }
}