using System.IO.Ports;

namespace Link
{
    public abstract class LinkStates
    {
        public virtual void OnEnter(LinkStateMachine context)
        {
            
        }

        public virtual void ReceivedByte(LinkStateMachine context, byte b)
        {
            
        }

        public virtual void BufferCount(LinkStateMachine context)
        {
            
        }

        public virtual void SentByte(LinkStateMachine context)
        {

        }

        public virtual void SendMsg(LinkStateMachine context, byte[] msg)
        {

        }

        public virtual byte[] ReceiveMsg(LinkStateMachine context)
        {
            return null;
        }

    }

    public class Idle : LinkStates
    {
        public override void SendMsg(LinkStateMachine context, byte[] msg)
        {
            context.SetBuffer(msg);
            context.SetState(new MoreToSend());
        }
    }

    public class MoreToSend : LinkStates
    {
        public override void OnEnter(LinkStateMachine context)
        {

        }
    }

    public class Sending : LinkStates
    {
        
    }

    public class WaitForA : LinkStates
    {
        
    }

    public class Receiving : LinkStates
    {
        
    }

    public class BState : LinkStates
    {
        
    }

    public class LinkStateMachine : ILinkFrontend
    {
        private LinkStates _state;
        private readonly SerialPort _port;

        public LinkStateMachine(string portName)
        {
            _state = new Idle();
            _port = new SerialPort(portName,115200,Parity.None,8,StopBits.One);
            _port.Open();
        }

        public void SetState(LinkStates state)
        {
            _state = state;
            _state.OnEnter(this);
        }

        private byte[] Buffer { get; set; }

        public void SendMessage(byte[] msg)
        {
            _state.SendMsg(this, msg);
        }

        public byte[] GetMessage()
        {
            return _state.ReceiveMsg(this);
        }


        public int BufferSize()
        {
            return 0;
        }

        public byte GetFrontBufferByte()
        {
            return 0;
        }

        public void SetBackBufferByte(byte b)
        {
            
        }

        public void SendByte(byte s)
        {

        }

        public byte GetByte()
        {
            return 0;
        }

        public void SetBuffer(byte[] bytes)
        {
            Buffer = bytes;
        }

        
        
    }

  


}
