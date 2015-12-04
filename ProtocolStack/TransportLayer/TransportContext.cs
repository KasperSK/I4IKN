using System;
using System.Threading;
using log4net;
using LinkLayer;
using TransportLayer.TransportStates;

namespace TransportLayer
{
    public class TransportContext : ITransportStm
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof (TransportContext));
        private readonly IChecksum _checksum;
        private readonly ManualResetEvent _communicateEvent;
        private readonly AutoResetEvent   _linkReadyEvent;

        private readonly Message _inputMessage;
        private readonly ILink _link;
        private readonly int _maxMessageDataSize;
        private readonly Message _outputMessage;
        private readonly ISequenceGenerator _sequence;

        private readonly object _stateLock = new object();

        private readonly Thread communicatorThread;
        private volatile bool _shutdown;
        private TransportBaseState _state;

        public TransportContext(IChecksum checksum, ISequenceGenerator sequence, ILink link, int maxMessageDataSize)
        {
            _checksum = checksum;
            _sequence = sequence;
            _link = link;
            _maxMessageDataSize = maxMessageDataSize;

            _outputMessage = new Message(_maxMessageDataSize);

            _communicateEvent = new ManualResetEvent(false);
            _linkReadyEvent = new AutoResetEvent(false);

            _shutdown = false;


            SetState(new Idle());

            communicatorThread = new Thread(Communicator);
            communicatorThread.Start();
        }


        // Public Interface
        public void SendData(byte[] buffer, int size)
        {
            var offset = 0;
            // While we have data to send
            while (size - offset > 0)
            {
                // Set length to remaining, but not larger than max
                var length = size - offset > _maxMessageDataSize ? _maxMessageDataSize : size - offset;

                // Send the data
                _linkReadyEvent.WaitOne();
                
                _state.SendData(this, buffer, offset, length);
                
                // Move the offset with the amount we send
                offset += length;
            }
        }

        public int ReceiveData(byte[] buffer, int size)
        {
            throw new NotImplementedException();
        }


        // States Interface
        public void SetState(TransportBaseState state)
        {
            _state = state;
            _state.OnEnter(this);
        }

        public void LinkIsReady()
        {
            _linkReadyEvent.Set();
        }

         

        public void StartCommunication()
        {
            _communicateEvent.Set();
        }

        public void StopCommunication()
        {
            _communicateEvent.Reset();
        }

        public void SendMessage()
        {
            _link.SendMessage(_outputMessage.Buffer, _outputMessage.MessageSize);
        }

        public void ReceiveMessage()
        {
            _inputMessage.MessageSize = _link.GetMessage(_inputMessage.Buffer);

            if (!_inputMessage.ValidMessageForm() || !_checksum.VerifyChecksum(_inputMessage))
            {
                _state.ReceivedInvalidMessage(this, _inputMessage);
                return;
            }

            switch (_inputMessage.DataType)
            {
                case DataType.Ack:
                    _state.ReceivedAckMessage(this, _inputMessage);
                    break;
                case DataType.Data:
                    _state.ReceivedDataMessage(this, _inputMessage);
                    break;
                case DataType.Syn:
                    _state.ReceivedSynMessage(this, _inputMessage);
                    break;
                default:
                    _state.ReceivedInvalidMessage(this, _inputMessage);
                    break;
            }
        }

        // Private Helper Methods
        private void Communicator()
        {
            while (!_shutdown)
            {
                _communicateEvent.WaitOne();
                _state.Communicate(this);
            }
        }
    }

    public abstract class TransportBaseState
    {
        public virtual void OnEnter(TransportContext context)
        {
        }

        public virtual void Communicate(TransportContext context)
        {
        }

        public virtual bool ReceivedAckMessage(TransportContext context, Message message)
        {
            return false;
        }

        public virtual bool ReceivedDataMessage(TransportContext context, Message message)
        {
            return false;
        }

        public virtual bool ReceivedSynMessage(TransportContext context, Message message)
        {
            return false;
        }

        public virtual bool ReceivedInvalidMessage(TransportContext context, Message message)
        {
            return false;
        }

        public virtual void SendData(TransportContext context, byte[] buffer, int offset, int length)
        {
        }
    }
}