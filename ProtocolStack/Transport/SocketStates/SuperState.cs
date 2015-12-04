using System;
using System.Linq;

namespace Transport.SocketStates
{
    public abstract class SuperState
    {
        public virtual void OnEnter(Socket socket)
        {
            
        }
        public virtual void ReceivedSyn(Socket socket, Message message)
        {
            
        }

        public virtual void ReceivedSynAck(Socket socket, Message message)
        {

        }

        public virtual void ReceivedAck(Socket socket, Message message)
        {

        }

        public virtual void Connect(Socket socket)
        {

        }

        public virtual Message TransmitMessage(Socket socket)
        {
            var message = socket.MessageBuffer.FirstOrDefault(p => p.Seq == socket.NextSeq);
            
            if (message != null)
                ++socket.NextSeq;
            return message;
        }

    }
}