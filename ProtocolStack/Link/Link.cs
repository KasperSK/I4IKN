using System;

namespace LinkLayer
{
    public class Link : ILink
    {
        private readonly IDecrypt _decrypt;
        private readonly byte[] _inputbuffer;
        private readonly int _messageSize;
        private IEncrypt _encrypt;
        private readonly IPhysical _physical;

        private int _inputbufferend;
        private int _inputbufferptr;

        public Link(int messageSize, IDecrypt decrypt, IEncrypt encrypt, IPhysical physical)
        {
            _decrypt = decrypt;
            _encrypt = encrypt;
            _physical = physical;
            _messageSize = messageSize;
            _inputbuffer = new byte[messageSize*2 + 2];
            _inputbufferend = 0;
            _inputbufferptr = 0;
        }

        public void SendMessage(byte[] msg, int length)
        {
            byte[] buffer;

            _encrypt.NewMessage(length);
            for (var i = 0; i < length; i++)
            {
                _encrypt.ParseByte(msg[i]);
            }

            var encryptedLength = _encrypt.GetEncryptedMessage(out buffer);
            _physical.Write(buffer, encryptedLength);
        }

        public int GetMessage(byte[] msg)
        {
            _decrypt.NewMessage(msg);
            bool frameIsComplete;
            do
            {
                if (_inputbufferptr == _inputbufferend)
                {
                    _inputbufferend = _physical.Read(_inputbuffer, _messageSize*2 + 2);
                    _inputbufferptr = 0;
                }
                frameIsComplete = _decrypt.ParseByte(_inputbuffer[_inputbufferptr]);
                ++_inputbufferptr;
            } while (!frameIsComplete);
            return _decrypt.BufferSize;
        }
    }
}