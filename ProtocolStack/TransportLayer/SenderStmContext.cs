using System;
using System.Threading;
using LinkLayer;
using TransportLayer.SenderStates;

namespace TransportLayer
{
    

    public class SenderStmContext : ISender
    {
        //State Machine internals
        private SenderSuperState _state;
        


        public void SetState(SenderSuperState state)
        {
            _state = state;
            _state.OnEnter(this);
        }

        private readonly IChecksum _checksum;
        private readonly ILink _link;
        private readonly Timer _timer;
        private readonly int _maxMessageDataSize;
        private readonly Message _message;
        private readonly Message _reply;
        private readonly ISequenceGenerator _sequence;
        private readonly int _timeout;

        public bool Ready;
        
        public SenderStmContext(ILink link, IChecksum checksum, ISequenceGenerator sequenceGenerator, int maxMessageDataSize, int timeoutModifier)
        {
            
            _checksum = checksum;

            _timeout = link.Timeout * timeoutModifier;

            _timer = new Timer(MessageTimeout);
            
            _link = link;

            _maxMessageDataSize = maxMessageDataSize;
            _message = new Message(_maxMessageDataSize);
            _reply = new Message(0);

            _sequence = sequenceGenerator;

            SetState(new Sending());
            
        }

        //Public Interfaces
        public void SendData(byte[] buffer, int size)
        {
            var offset = 0;
            // While we have data to send
            while (size - offset > 0)
            {
                // Set length to remaining, but not larger than max
                var length = size - offset > _maxMessageDataSize ? _maxMessageDataSize : size - offset;

                // Send the data
                _state.SendData(this, buffer, offset, length);

                // Move the offset with the amount we send
                offset += length;

                // This is to emulate receive events, Ready is true when the message came acros correct
                while (!Ready)
                {
                    ReceiveMessage();
                    _state.ReceivedMessage(this, _reply);
                }
            }
        }

        public void SyncUp()
        {
            _state.Sync(this);
            while (!Ready)
            {
                ReceiveMessage();
                _state.ReceivedMessage(this, _reply);
            }
        }

        //Actions
        public void SetMessage(byte[] buffer, int offset, int size)
        {
            _message.SetData(buffer, offset, size);
            _message.DataType = DataType.Data;
            _message.Sequence = _sequence.Sequence;
            _checksum.GenerateChecksum(_message);
        }

        public void SetSyncMessage()
        {
            _message.DataSize = 0;
            _message.DataType = DataType.Syn;
            _message.Sequence = _sequence.Sequence;
            _checksum.GenerateChecksum(_message);
        }

        public void IncrementSequence()
        {
            _sequence.Increment();
        }

        public void ResetSequence()
        {
            _sequence.Reset();
        }

        public bool ValidateReply()
        {
            /*
             return _reply.ValidMessageSize() && 
                    _checksum.VerifyChecksum(_reply) &&
                    _reply.DataType == DataType.Ack &&
                    _reply.DataSize == 0 &&
                    _reply._sequence == _sequence;
                    */

            Console.WriteLine("SenderStateMachine Validating Length");
            if (!_reply.ValidMessageSize())
                return false;

            Console.WriteLine("SenderStateMachine Checksum");
            if (!_checksum.VerifyChecksum(_reply))
                return false;

            Console.WriteLine("SenderStateMachine Validating Datatype");
            if (_reply.DataType != DataType.Ack)
                return false;

            Console.WriteLine("SenderStateMachine Validating Size");
            if (_reply.DataSize != 0)
                return false;

            Console.WriteLine("SenderStateMachine Validating Sequence");
            if (_reply.Sequence != _sequence.Sequence)
                return false;

            return true;

        }

        public void SendMessage()
        {
            Console.WriteLine("SenderStateMachine =============Sending================");
            Console.WriteLine("SenderStateMachine Seq: " + (int)_message.Sequence);
            Console.WriteLine("SenderStateMachine Type: " + _message.DataType);
            Console.WriteLine("SenderStateMachine Size: " + _message.MessageSize);
            StartTimer();
            _link.SendMessage(_message.Buffer, _message.MessageSize);
        }




        // Internal helper functions
        private void MessageTimeout(object obj)
        {
            StopTimer();
            _state.Timeout(this);
        }

        private void ReceiveMessage()
        {
            _reply.MessageSize = _link.GetMessage(_reply.Buffer);
            Console.WriteLine("SenderStateMachine ##########Receiving###############");
            Console.WriteLine("SenderStateMachine Seq: " + (int)_reply.Sequence);
            Console.WriteLine("SenderStateMachine Type: " + _reply.DataType);
            Console.WriteLine("SenderStateMachine Size: " + _reply.MessageSize);
            StopTimer();
        }

        private void StartTimer()
        {
            _timer.Change(_timeout, Timeout.Infinite);
        }

        private void StopTimer()
        {
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
        }
        
    }

    public abstract class SenderSuperState
    {
        public virtual void OnEnter(SenderStmContext context)
        {

        }

        public virtual void Sync(SenderStmContext context)
        {
            
        }

        public virtual void SendData(SenderStmContext context, byte[] buffer, int offset, int size)
        {

        }

        public virtual void ReceivedMessage(SenderStmContext context, Message message)
        {
            
        }

        public virtual void Timeout(SenderStmContext context)
        {

        }
    }
}