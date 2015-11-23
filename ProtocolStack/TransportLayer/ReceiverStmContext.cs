using System;
using LinkLayer;
using TransportLayer.ReceivingStates;

namespace TransportLayer
{
    public abstract class ReceiverSuperState
    {
        public virtual void OnEnter(ReceiverStmContext context)
        {

        }

        public virtual void ReceiveMessage(ReceiverStmContext context)
        {

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
            SetState(new ReceivingZero());
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
}