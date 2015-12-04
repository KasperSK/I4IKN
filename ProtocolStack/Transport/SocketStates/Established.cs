using log4net;

namespace Transport.SocketStates
{
    public class Established : SuperState
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(Established));
        public override void OnEnter(Socket socket)
        {
            Logger.Debug("Connection Established");
        }
    }
}