using System;
using LinkLayer;
using NUnit.Framework;

namespace Protocol.Test.Unit.LinkLayer
{
    [TestFixture]
    public class DecryptStmTests
    {
        [SetUp]
        public void DecryptStmSetup()
        {
            _uut = new DecryptStm(Convert.ToByte('A'), Convert.ToByte('B'), Convert.ToByte('C'), Convert.ToByte('D'));
            _buffer = new byte[100];
            _uut.NewMessage(_buffer);
        }

        private IDecrypt _uut;
        private byte[] _buffer;

        private bool ParseChar(char ch)
        {
            return _uut.ParseByte(Convert.ToByte(ch));
        }

        [Test]
        public void TestEmptyFrame()
        {
            ParseChar('A');

            var gotMessage = ParseChar('A');

            Assert.That(gotMessage, Is.True);
        }

        [Test]
        public void TestInvalidEscapeParse()
        {
            foreach (var ch in "AKB")
            {
                ParseChar(ch);
            }

            Assert.That(() => _uut.ParseByte(Convert.ToByte('q')), Throws.Exception);
        }

        [Test]
        public void TestInvalidNewMessage()
        {
            Assert.That(() => _uut.NewMessage(_buffer), Throws.Exception);
        }

        [Test]
        public void TestInvalidParseByte()
        {
            ParseChar('A');
            ParseChar('A');


            Assert.That(() => ParseChar('k'), Throws.Exception);
        }

        [Test]
        public void TestInvalidStart()
        {
            Assert.That(() => _uut.ParseByte(Convert.ToByte('q')), Throws.Exception);
        }

        [Test]
        public void TestValidEscapeString()
        {
            var gotMessage = false;

            foreach (var ch in "AKBCLLE er BDBCGUDA")
            {
                gotMessage = ParseChar(ch);
            }


            for (var i = 0; i < _uut.BufferSize; i++)
            {
                Console.Write(Convert.ToChar(_buffer[i]));
            }
            Console.WriteLine();
            Assert.That(gotMessage, Is.True);
        }

        [Test]
        public void TestValidString()
        {
            ParseChar('A');
            foreach (var ch in "holaamigo")
            {
                ParseChar(ch);
            }

            var gotMessage = ParseChar('A');
            for (var i = 0; i < _uut.BufferSize; i++)
            {
                Console.Write(Convert.ToChar(_buffer[i]));
            }
            Console.WriteLine();
            Assert.That(gotMessage, Is.True);
        }
    }
}