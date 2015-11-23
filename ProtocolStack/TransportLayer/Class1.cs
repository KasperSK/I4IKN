using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LinkLayer;

namespace TransportLayer
{
    public enum DataType
    {
        Data = 0,
        Ack = 1
    }

    public enum HeaderPosition
    {
        ChecksumHigh = 0,
        ChecksumLow = 1,
        Sequence = 2,
        Type = 3
    }

    public enum Sequence
    {
        Zero = 0,
        One = 1
    }

    public interface ITransport
    {
        void SendMessage(byte[] message, int size);
        int ReceiveMessage(byte[] message);
    }

    public class Transport : ITransport
    {
        private const int Buffer = 1000;
        private ILink _link;
        private SenderStmContext _sendingStateMachine;
        private ReceiverStmContext _receivingStateMachine;


        public Transport(string portName, int baud, int databits)
        {
            _link = new Link(Buffer, new DecryptStm(), new EncryptStm(), new Serial(portName, baud, databits));
            _sendingStateMachine = new SenderStmContext(_link);
            _receivingStateMachine = new ReceiverStmContext(_link);
        }

        public void SendMessage(byte[] message, int size)
        {
            _sendingStateMachine.SendMessage(message, size);
        }

        public int ReceiveMessage(byte[] message)
        {
            return _receivingStateMachine.ReceiveMessage(message);
        }
    }

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

    public class SendingZero : SenderSuperState
    {
        public override void SendMessage(SenderStmContext context, byte[] buffer, int size)
        {
            context.SetUpBuffer(buffer, size);
            context.MakeCheckSum();
            context.SetUpHeader(0, DataType.Data);
            context.SendSegment();
            context.StartTimer();
            context.SetState(new WaitingZero());
        }
    }

    public class WaitingZero : SenderSuperState
    {
        public override void OnEnter(SenderStmContext context)
        {
            if(context.ReceiveAck() == 4)
                context.ReceiveMessage();
        }

        public override void ReceiveMessage(SenderStmContext context)
        {
            if(context.VerifyCheckSum())
                if (context.IsAck(0))
                {
                    context.StopTimer();
                    context.SetState(new SendingOne());
                }
        }

        public override void Timeout(SenderStmContext context)
        {
            context.StopTimer();
            context.SendSegment();
            context.StartTimer();
            context.SetState(this);
        }
    }

    public class SendingOne : SenderSuperState
    {
        public override void SendMessage(SenderStmContext context, byte[] buffer, int size)
        {
            context.SetUpBuffer(buffer, size);
            context.MakeCheckSum();
            context.SetUpHeader(1, DataType.Data);
            context.SendSegment();
            context.StartTimer();
            context.SetState(new WaitingOne());
        }
    }

    public class WaitingOne : SenderSuperState
    {
        public override void OnEnter(SenderStmContext context)
        {
            if (context.ReceiveAck() == 4)
                context.ReceiveMessage();
        }

        public override void ReceiveMessage(SenderStmContext context)
        {
            if (context.VerifyCheckSum())
                if (context.IsAck(1))
                {
                    context.StopTimer();
                    context.SetState(new SendingZero());
                }
        }

        public override void Timeout(SenderStmContext context)
        {
            context.StopTimer();
            context.SendSegment();
            context.StartTimer();
            context.SetState(this);
        }
    }

    public class ReceiverStmContext
    {
        //State Machine internals
        private ReceiverSuperState _state;
        public void SetState(ReceiverSuperState state)
        {
            _state = state;
            _state.OnEnter(this);
        }

        private readonly Checksum _checksum;
        private readonly ILink _link;
        private byte[] _recvBuffer;
        private byte[] _sendBuffer;
        private byte[] _returnBuffer;
        private int _receiveBufferLenght;

        public ReceiverStmContext(ILink link)
        {
            _recvBuffer = new byte[1004];
            _sendBuffer = new byte[4];
            _link = link;
        }

        //Events
        public int ReceiveMessage(byte[] receiveBuffer)
        {
            _returnBuffer = receiveBuffer;
            _receiveBufferLenght = receiveBuffer.Length;
            _state.ReceiveMessage(this);
            return _receiveBufferLenght;
        }

        //Actions
        public bool VerifyCheckSum()
        {
            return _checksum.VerifyCheckSum(_recvBuffer);
        }

        public void ReadSerial()
        {
            _receiveBufferLenght = _link.GetMessage(_recvBuffer);
        }

        public bool CheckSequence(int seq)
        {
            return _recvBuffer[(int) HeaderPosition.Sequence] == seq;
        }

        public void SetUpAck(byte seq)
        {
            _sendBuffer[(int)HeaderPosition.Sequence] = seq;
            _sendBuffer[(int)HeaderPosition.Type] = 1;
            _checksum.CalculateCheckSum(_sendBuffer);
        }

        public void SendAck()
        {
            _link.SendMessage(_sendBuffer, 4);
        }

        public void SetReturnZero()
        {
            _receiveBufferLenght = 0;
        }

        public void CopyToReturnBuffer()
        {
            Array.Copy(_recvBuffer, 4, _returnBuffer, 0, _receiveBufferLenght - 4);
        }
    }

    public abstract class ReceiverSuperState
    {
        public virtual void OnEnter(ReceiverStmContext context)
        {
            
        }

        public virtual void ReceiveMessage(ReceiverStmContext context)
        {
            
        }
    }

    public class ReceivingZero : ReceiverSuperState
    {
        public override void ReceiveMessage(ReceiverStmContext context)
        {
            context.ReadSerial();
            if (context.VerifyCheckSum() && context.CheckSequence(0))
            {
                context.SetUpAck(0);
                context.SendAck();
                context.CopyToReturnBuffer();
            }
            else
            {
                context.SetUpAck(1);
                context.SendAck();
                context.SetReturnZero();
            }
        }
    }

    public class ReceivingOne : ReceiverSuperState
    {
        public override void ReceiveMessage(ReceiverStmContext context)
        {
            context.ReadSerial();
            if (context.VerifyCheckSum() && context.CheckSequence(1))
            {
                context.SetUpAck(1);
                context.SendAck();
                context.CopyToReturnBuffer();
            }
            else
            {
                context.SetUpAck(0);
                context.SendAck();
                context.SetReturnZero();
            }
        }
    }

    public class Checksum
    {
        private int MakeCheckSum(byte[] buffer)
        {
            // TO DO FIX THIS SHIT
            return 10;
        }

        public bool VerifyCheckSum(byte[] buffer)
        {
            return true;
        }

        public void CalculateCheckSum(byte[] buffer)
        {
            byte[] internalBuffer = new byte[buffer.Length - 2];

            Array.Copy(buffer, 2, internalBuffer, 0, internalBuffer.Length);

            var chsum = MakeCheckSum(internalBuffer);

            buffer[0] = (byte)((chsum >> 8) & 0xFF);
            buffer[1] = (byte)(chsum & 0xFF);
        }
    }
}