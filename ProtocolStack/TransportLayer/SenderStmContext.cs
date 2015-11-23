using System;
using System.Threading;
using LinkLayer;
using TransportLayer.SenderStates;

namespace TransportLayer
{
    

    public class SenderStmContext
    {
        //State Machine internals
        private SenderSuperState _state;
        public void SetState(SenderSuperState state)
        {
            _state = state;
            _state.OnEnter(this);
        }

        private readonly Checksum _checksum;
        private readonly ILink _link;
        private readonly Timer _timer;
        private readonly TimerCallback _timerCallback;
        private byte[] _messagebuffer;
        private byte[] _recvBuffer;
        private int _lenght;

        public SenderStmContext(ILink link)
        {
            _timerCallback += MessageTimeout;
            _checksum = new Checksum();
            _timer = new Timer(_timerCallback, null, Timeout.Infinite, Timeout.Infinite);
            _link = link;
            SetState(new SendingZero());
        }

        //Events
        public void SendMessage(byte[] buffer, int size)
        {

            _state.SendMessage(this, buffer, size);
        }

        public void ReceiveMessage()
        {
            _state.ReceiveMessage(this);
        }

        public void MessageTimeout(object obj)
        {
            _state.Timeout(this);
        }

        //Actions

        public void SetUpBuffer(byte[] buffer, int size)
        {
            _messagebuffer = new byte[size + 4];
            Array.Copy(buffer,0,_messagebuffer,4,size);
            _lenght = size + 4;
        }

        public void SetUpHeader(byte seq, DataType type)
        {
            _messagebuffer[(int)HeaderPosition.Sequence] = seq;
            _messagebuffer[(int)HeaderPosition.Type] = (byte)type;
        }

        public void MakeCheckSum()
        {
            _checksum.CalculateCheckSum(_messagebuffer);
        }

        public bool VerifyCheckSum()
        {
            return _checksum.VerifyCheckSum(_recvBuffer);
        }

        public bool IsAck(byte seq)
        {
            return _recvBuffer[(int) HeaderPosition.Sequence] == seq && _recvBuffer[(int)HeaderPosition.Type] == (byte)DataType.Ack;
        }

        public void SendSegment()
        {
            _messagebuffer[0] = 75;
            _messagebuffer[1] = 75;
            _messagebuffer[2] = 75;
            _messagebuffer[3] = 75;
            _link.SendMessage(_messagebuffer, _lenght);
        }

        public int ReceiveAck()
        {
            _recvBuffer = new byte[4];
            return _link.GetMessage(_recvBuffer);
        }

        public void StartTimer()
        {
            _timer.Change(10000, Timeout.Infinite);
        }

        public void StopTimer()
        {
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
        }
    }

    public abstract class SenderSuperState
    {
        public virtual void OnEnter(SenderStmContext context)
        {

        }

        public virtual void SendMessage(SenderStmContext context, byte[] buffer, int size)
        {

        }

        public virtual void ReceiveMessage(SenderStmContext context)
        {

        }

        public virtual void Timeout(SenderStmContext context)
        {

        }
    }
}