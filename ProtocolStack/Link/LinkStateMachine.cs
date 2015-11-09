using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Link
{
    public class LinkStates
    {
        public virtual void OnEnter(LinkStateMachine context)
        {
            
        }

        public virtual void SendMsg(LinkStateMachine context, byte[] msg)
        {
            
        }

        public virtual void GetByte(LinkStateMachine context)
        {
            
        }

        public virtual void GetCount(LinkStateMachine context)
        {
            
        }
    }

    public class Idle : LinkStates
    {
        public override void SendMsg(LinkStateMachine context, byte[] msg)
        {
            context.SendingBuffer = msg;
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

    public class LinkStateMachine
    {
        private LinkStates _state;
        public SerialPort Port { get; private set; }

        public LinkStateMachine(string portName)
        {
            Port = new SerialPort(portName, 115200, Parity.None, 8, StopBits.One);
            if(!Port.IsOpen)
                Port.Open();
        }

        public void SetState(LinkStates state)
        {
            _state = state;
        }

        public byte[] ReceivingBuffer { get; set; }
        public byte[] SendingBuffer { get; set; }



        public void SendMsg(byte[] msg)
        {
            _state.SendMsg(this, msg);
        } 

        public int GetCount()
        {
            
        }

        public byte[] ReceiveMsg()
        {
            // To do write code
            return ReceivingBuffer;
        }

        public void SendByte(byte[] msg)
        {
            
        }
    }

  


}
