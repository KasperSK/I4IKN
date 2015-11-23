using System;

namespace LinkLayer
{
    public static class Factory
    {
        public static ILink GetLink(string portName, int buffer, int timeout)
        {
            return new Link(new DecryptStm(), new EncryptStm(), new Serial(portName, 115200, 8, buffer * 2 + 2, timeout)  );
        }
    }
    public class Link : ILink
    {
        private readonly IDecrypt _decrypt;
        private readonly IEncrypt _encrypt;
        private readonly IPhysical _physical;


        public Link(IDecrypt decrypt, IEncrypt encrypt, IPhysical physical)
        {
            _decrypt = decrypt;
            _encrypt = encrypt;
            _physical = physical;
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

                _decrypt.Reset();
                return -1;
            }
            

            // Timeout for the rest
            _physical.EnableTimeout();
            try
            {
                while (!_decrypt.ParseByte(_physical.Read()))
                {
                }
            }
            catch (TimeoutException)
            {
                _decrypt.Reset();
                return -1;
            }
            catch (IndexOutOfRangeException)
            {
                _decrypt.Reset();
                return -1;
            }
            catch (ArgumentException)
            {
                _decrypt.Reset();
                return -1;
            }
            
            return _decrypt.BufferSize;
        }
    }
}