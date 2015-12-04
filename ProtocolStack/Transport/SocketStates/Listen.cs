using log4net;

namespace Transport.SocketStates
{
    public class Listen : SuperState
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(Listen));
        public override void OnEnter(Socket socket)
        {
            socket.MessageBuffer.Clear();
            socket.LocalSeq = 0;
            socket.RemoteSeq = 0x4545;
            socket.RemoteAck = 0x4646;
            socket.NextSeq = 0;
            socket.RemoteWindow = 0x47;
        }

        public override void Connect(Socket socket)
        {
            Logger.Debug("Connecting");

            socket.LocalSeq = 0x6161;
            
            socket.NextSeq = socket.LocalSeq;
            socket.SendSyn();
            socket.SetState(new SynSent());
        }

        public override void ReceivedSyn(Socket socket, Message message)
        {
            Logger.Debug("Someone wants to connect");

            socket.LocalSeq = 0x6161;
            socket.RemoteSeq = message.Seq;
            socket.NextSeq = socket.LocalSeq;

            Logger.Debug($"Seq {socket.LocalSeq:X} Ack {socket.RemoteSeq:X}");
            socket.SendSynAck();
            socket.SetState(new SynReceived());
        }
    }
}