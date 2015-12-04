using log4net;
using log4net.Repository.Hierarchy;

namespace Transport.SocketStates
{
    public class SynReceived : SuperState
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(SynReceived));
        public override void ReceivedAck(Socket socket, Message message)
        {
            if (socket.LocalSeq == message.Ack && socket.RemoteSeq == message.Seq)
            {
                socket.SetState(new Established());
            }
        }
    }
}