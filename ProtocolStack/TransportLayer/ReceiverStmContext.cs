using System;
using log4net;
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

        private static readonly ILog Logger = LogManager.GetLogger(typeof(ReceiverStmContext));

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
        public int ReceiveData(byte[] buffer, int size)
        {
            while (!Ready)
            {
                
                ReceiveMessage();
                _state.MessageReceived(this);
            }
            Array.Copy(_message.Buffer, Message.DataOffset, buffer, 0, _message.DataSize);
            Ready = false;
            return _message.DataSize;
        }

        public bool ValidateMessage()
        {
            if (!_message.ValidMessageSize())
            {
                Logger.Debug("ValidateMessage\tLength\tFailed");
                return false;
            }
            Logger.Debug("ValidateMessage\tLength\t\tOK");

            if (!_checksum.VerifyChecksum(_message))
            {
                Logger.Debug("ValidateMessage\tChecksum\tFailed");
                return false;
            }
            Logger.Debug("ValidateMessage\tChecksum\tOK");
            Logger.Info("ValidateMessage\tAll\t\tOK");
            return true;
        }

        public bool ValidSync()
        {
            if (_message.DataSize != 0)
            {
                Logger.Info("ValidSync\t\tSize\tFailed: " + _message.DataSize);
                return false;
            }

            Logger.Info("ValidSync\t\tSize\t\tOK");
            return true;
        }

        public bool ValidData()
        {
            if (_message.DataSize <= 0)
            {
                Logger.Info("ValidData\t\tSize\t\tFailed: " + _message.DataSize);
                return false;
            }

            Logger.Info("ValidData\t\tSize\t\tOK");
            return true;
        }

        public bool ValidSequence()
        {
            if (_message.Sequence == _sequence.Sequence)
            {
                Logger.Info("ValidSequence\tSequence\tOK");
            }
            else
            {
                Logger.Debug($"ValidSequence\tSequence\tFailed Expected [{_sequence.Sequence}] Was [{_message.Sequence}]");
            }
            return _message.Sequence == _sequence.Sequence;
        }

        public void UpdateSequence()
        {
            Logger.Info($"UpdateSequence: [{_message.Sequence}]");
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