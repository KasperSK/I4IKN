using System;
using LinkLayer;
using TransportLayer.ReceivingStates;

namespace TransportLayer
{
    public class ReceiverStmContext : IReceiver
    {
        private readonly IChecksum _checksum;
        private readonly ILink _link;
        private readonly Message _message;
        private readonly Message _reply;
        private readonly ISequenceGenerator _sequence;
        //State Machine internals
        private ReceiverSuperState _state;

        public bool Ready;

        public ReceiverStmContext(ILink link, IChecksum cheksum, ISequenceGenerator sequenceGenerator,
            int maxMessageDataSize)
        {
            _message = new Message(maxMessageDataSize);
            _reply = new Message(0);
            _link = link;
            _sequence = sequenceGenerator;
            _checksum = cheksum;
            SetState(new MissingSync());
        }

        // Actions

        public DataType MessageType => _message.DataType;

        public void SetState(ReceiverSuperState state)
        {
            _state = state;
            _state.OnEnter(this);
        }

        // Public Interface
        public int ReceiveData(byte[] buffer)
        {
            while (!Ready)
            {
                Console.WriteLine("Waiting for message");
                ReceiveMessage();
                _state.MessageReceived(this);
            }
            Array.Copy(_message.Buffer, Message.DataOffset, buffer, 0, _message.DataSize);
            Ready = false;
            return _message.DataSize;
        }

        public bool ValidateMessage()
        {
            Console.WriteLine("Validating Length");
            if (!_message.ValidMessageSize())
                return false;

            Console.WriteLine("Checksum");
            if (!_checksum.VerifyChecksum(_message))
                return false;

            return true;
        }

        public bool ValidSync()
        {
            Console.WriteLine("Validating Size");
            if (_message.DataSize != 0)
                return false;

            return true;
        }

        public bool ValidData()
        {
            Console.WriteLine("Validating Size: " + _message.DataSize);
            if (_message.DataSize <= 0)
                return false;

            return true;
        }

        public bool ValidSequence()
        {
            return _message.Sequence == _sequence.Sequence;
        }

        public void UpdateSequence()
        {
            _sequence.Sequence = _message.Sequence;
        }

        public void IncrementSequence()
        {
            _sequence.Increment();
        }

        public void SetAckReply()
        {
            _reply.Sequence = _sequence.Sequence;
            _reply.DataSize = 0;
            _reply.DataType = DataType.Ack;
            _checksum.GenerateChecksum(_reply);
        }

        public void SendReply()
        {
            _link.SendMessage(_reply.Buffer, _reply.MessageSize);
        }

        // Internal Helper Functions
        private void ReceiveMessage()
        {
            _message.MessageSize = _link.GetMessage(_message.Buffer);
        }
    }

    public abstract class ReceiverSuperState
    {
        public virtual void OnEnter(ReceiverStmContext context)
        {
        }

        public virtual void MessageReceived(ReceiverStmContext context)
        {
        }
    }
}