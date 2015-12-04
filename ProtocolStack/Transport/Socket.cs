using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using log4net;
using Transport.Annotations;
using Transport.SocketStates;

namespace Transport
{
    public class Socket : ISocket
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof (Socket));
        private readonly Fifo _receiveBuffer;
        private readonly Fifo _sendBuffer;

        
        

        //What remote has acked
        public ushort RemoteAck;
        //Remotes window
        public byte RemoteWindow;
        //Data only allowed when our seq < remoteack + window

        //Sequence we are at
        public ushort LocalSeq;
        //Next Sequence to send
        public ushort NextSeq;
        //Last ack we have made
        public ushort RemoteSeq;

        public byte LocalWindow => 0x48;


       

        private byte _sourcePort;
        private byte _destPort;

        private SuperState _state;

        private readonly object _lock;
        private readonly object _lockState;

        public readonly List<Message> MessageBuffer;

        private IPort _port;

        private readonly IPortController _portController;

        public void SetState(SuperState state)
        {
            _state = state;
            state.OnEnter(this);
        }

        public Socket(int bufferSize, IPortController portController)
        {
            _lock = new object();
            _lockState = new object();
            _receiveBuffer = new Fifo(bufferSize);
            _sendBuffer = new Fifo(bufferSize);
            _portController = portController;
            SourcePort = 0;
            MessageBuffer = new List<Message>();
            SetState(new Listen());
        }

        public Socket(int bufferSize, IPortController portController, byte sourcePort, byte destPort, IPort port) : this(bufferSize, portController)
        {
            SourcePort = sourcePort;
            DestinationPort = destPort;
            _port = port;
            Logger.Debug($"PortID: {PortId:X}");
            _port.AddSocket(this);
        }

        public void Reset()
        {
            RemoteSeq = 0;
            LocalSeq = 0;
        }

        public byte SourcePort
        {
            get
            {
                return _sourcePort;
            }
            set
            {
                _sourcePort = value;
                PortId = SourcePort;
                PortId = (ushort) (PortId << 8);
                PortId += DestinationPort;
            }
        }
        
        public byte DestinationPort {
            get
            {
                return _destPort;
            }
            set
            {
                _destPort = value;
                PortId = SourcePort;
                PortId = (ushort)(PortId << 8);
                PortId += DestinationPort;
            }
        }

        public ushort PortId { get; private set; }


        public int Send(byte[] buffer)
        {
            _sendBuffer.Write(buffer, 0, buffer.Length);
            return buffer.Length;
        }

        public int Receive(byte[] buffer)
        {
            return _receiveBuffer.Read(buffer, 0, buffer.Length);
        }



        public void Connect(int comport, byte port)
        {
            DestinationPort = port;
            _port = _portController.GetPort(comport);
            _port.AddSocket(this);
            _state.Connect(this);
        }

        public void Close()
        {
            _port.RemoveSocket(this);
            _port = null;
        }

        

        public void PutMessage(Message message)
        {
            Logger.Debug("Got Message");
            switch (message.Type)
            {
                case DataType.Syn:
                    _state.ReceivedSyn(this, message);
                    break;
                case DataType.SynAck:
                    _state.ReceivedSynAck(this, message);
                    break;
                case DataType.Ack:
                    _state.ReceivedAck(this, message);
                    break;
            }
        }

        public Message GetMessage()
        {
            lock (_lockState)
            {
                var message = _state.TransmitMessage(this);

                //We got nothing return null
                if (message == null) return null;

                //We got message, update headers and transmit
                message.Ack = (ushort)(RemoteSeq + 1);
                message.Window = LocalWindow;
                message.SourcePort = SourcePort;
                message.DestinationPort = DestinationPort;
                message.GenerateChecksum();

                return message;
            }
        }


        private void SendMessage(Message message)
        {
            lock (_lock)
            {
                message.Seq = LocalSeq;
                MessageBuffer.Add(message);
                ++LocalSeq;
            }
        }


        public void SendSyn()
        {
            var message = new Message(0)
            {
                Size = 10,
                Type = DataType.Syn,
            };
            SendMessage(message);
        }

        public void SendSynAck()
        {
            var message = new Message(0)
            {
                Size = 10,
                Type = DataType.SynAck,
            };
            SendMessage(message);
        }

        public void SendAck()
        {
            var message = new Message(0)
            {
                Size = 10,
                Type = DataType.Ack,
            };
            SendMessage(message);
        }


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}