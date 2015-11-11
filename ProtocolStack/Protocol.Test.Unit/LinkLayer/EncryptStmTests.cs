using System;
using LinkLayer;
using NUnit.Framework;

namespace Protocol.Test.Unit.LinkLayer
{
    [TestFixture]
    public class EncryptStmTests
    {
        private IEncrypt _uut;

        private void ParseChar(char ch)
        {
            _uut.ParseByte(Convert.ToByte(ch));
        }

        [SetUp]
        public void EncryptStmSetup()
        {
            _uut = new EncryptStm();
        }

        [Test]
        public void EncryptStm_EncryptString()
        {
            const string msg = "KALLE er BAGUD";
            _uut.NewMessage(msg.Length);
            foreach (var ch in msg)
            {
                ParseChar(ch);
            }
            
            byte[] buffer;
            var length = _uut.GetEncryptedMessage(out buffer);
            for (var i = 0; i < length; i++)
            {
                Console.Write(Convert.ToChar(buffer[i]));
            }
            Assert.That(length, Is.EqualTo(19));
        }

        [Test]
        public void EncryptStm_GetEncryptedExection()
        {

            Assert.That(() =>
            {
                byte[] buffer;
                _uut.GetEncryptedMessage(out buffer);
            }, Throws.Exception);
        }

        [Test]
        public void EncryptStm_ParseByteExection()
        {

            Assert.That(() =>  _uut.ParseByte(0), Throws.Exception);
        }
    }
}