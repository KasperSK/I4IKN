using log4net;

namespace Transport.SocketStates
{
    public class SynSent : SuperState
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(SynSent));
        public override void ReceivedSynAck(Socket socket, Message message)
        {
            if (message.Ack == socket.LocalSeq)
            {
                Logger.Debug("Got my valid SynAck");
                socket.RemoteSeq = message.Seq;
                ++socket.RemoteSeq;
                socket.SendAck();
                socket.SetState(new Established());
            }
            else
            {
                Logger.Debug($"Got my invalid SynAck was {message.Ack:X} should be {socket.LocalSeq:X}");
            }
            
        }
    }
}