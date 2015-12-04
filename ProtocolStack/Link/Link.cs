using System;
using System.IO.Ports;
using log4net;

namespace LinkLayer
{
    public static class Factory
    {
        public static ILink GetLink(SerialPort port, int maxMessageSize, int timeoutmodifier)
        {
            var maxFrameSize = maxMessageSize * 2 + 2;
            return new Link(new DecryptStm(), new EncryptStm(), new Serial(port, maxFrameSize, timeoutmodifier), maxFrameSize);
        }
    }
    public class Link : ILink
    {
        private readonly IDecrypt _decrypt;
        private readonly IEncrypt _encrypt;
        private readonly IPhysical _physical;
        private readonly int _maxFrameSize;

        private static readonly ILog Logger = LogManager.GetLogger(typeof(Link));

        public Link(IDecrypt decrypt, IEncrypt encrypt, IPhysical physical, int maxFrameSize)
        {
            _decrypt = decrypt;
            _encrypt = encrypt;
            _physical = physical;
            _maxFrameSize = maxFrameSize;
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
            //Clear for new message
            _decrypt.NewMessage(msg);

            //Fetch first char without timeout
            _physical.DisableTimeout();
            try
            {
                _decrypt.ParseByte(_physical.Read());
            }
            catch (ArgumentException)
            {
                Logger.Debug("Invalid Start Char");
                _physical.ClearBuffer();
                _decrypt.Reset();
                return -1;
            }
            

            // Timeout for the rest
            Logger.Debug("Got Start Frame");
            _physical.EnableTimeout();
            try
            {
                while (!_decrypt.ParseByte(_physical.Read()))
                {
                }
            }
            catch (TimeoutException)
            {
                Logger.Debug("Timeout");
                _physical.ClearBuffer();
                _decrypt.Reset();
                return -1;
            }
            catch (IndexOutOfRangeException)
            {
                Logger.Debug("Overflow");
                _physical.ClearBuffer();

                _decrypt.Reset();
                return -1;
            }
            catch (ArgumentException)
            {
                Logger.Debug("Invalid Char");
                _physical.ClearBuffer();
                _decrypt.Reset();
                return -1;
            }
            Logger.Info("Valid Frame " + _decrypt.BufferSize + " byte(s)");
            return _decrypt.BufferSize;
        }

        public int Timeout => _physical.Timeout * _maxFrameSize;

        public void Open()
        {
            _physical.Open();
        }
        public void Close()
        {
            _physical.Close();
        }
    }
}