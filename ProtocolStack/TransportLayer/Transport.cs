using LinkLayer;

namespace TransportLayer
{
    public enum DataType : byte
    {
        Syn = 97,
        Data = 98,
        Ack = 99
    }

    public class Transport : ITransport
    {
        private ReceiverStmContext _receivingStateMachine;
        private SenderStmContext _sendingStateMachine;
        private bool _connected;


        public Transport()
        {
            _connected = false;
        }


        public void SendMessage(byte[] message, int size)
        {
            _sendingStateMachine.SendData(message, size);
        }

        public int ReceiveMessage(byte[] message)
        {
            return _receivingStateMachine.ReceiveData(message);
        }

        public void Connect(string port, int baud, int databits)
        {
            var link = Factory.GetLink(port, 1000, 10000);
            var checksum = new Checksum();
            _sendingStateMachine = new SenderStmContext(link, checksum, new SequenceGenerator(), 1000, 20000);
            _receivingStateMachine = new ReceiverStmContext(link, checksum, new SequenceGenerator(), 1000);
        }

        public void Disconnect()
        {
            throw new System.NotImplementedException();
        }
    }


}