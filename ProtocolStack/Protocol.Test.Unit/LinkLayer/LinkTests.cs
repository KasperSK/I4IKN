using System;
using LinkLayer;
using NSubstitute;
using NUnit.Framework;

namespace Protocol.Test.Unit.LinkLayer
{
    [TestFixture]
    public class LinkFrontendTests
    {
        [SetUp]
        public void LinkFrontendSetup()
        {
            var decrypter = new DecryptStm();
            var encrypter = new EncryptStm();
            _fakePhysical = Substitute.For<IPhysical>();
            _uut = new Link(1004, decrypter, encrypter, _fakePhysical);
        }

        private ILink _uut;
        private IPhysical _fakePhysical;
        private CharEnumerator _dataEnumerator;
        private string _serialData;

        private void SetPhysicalResponse(int split)
        {
            _dataEnumerator = _serialData.GetEnumerator();
            _dataEnumerator.Reset();

            _fakePhysical.InfiniteTimeout.Returns(100);

            _fakePhysical
                .Read(Arg.Any<byte[]>(), Arg.Any<int>(), 100)
                .Returns(x =>
                {
                    var buffer = x.Arg<byte[]>();
                    var j = 0;
                    for (var i = 0; i < split; ++i)
                    {
                        if (_dataEnumerator.MoveNext())
                        {
                            buffer[i] = Convert.ToByte(_dataEnumerator.Current);
                            ++j;
                        }
                        else
                        {
                            break;
                        }
                    }
                    return j;
                });
        }

        [Test]
        public void GetMessage_Nospecialbytes_CorrectNumber()
        {
            _serialData = "ANogetdsaffdsafdsaOKAFLFA";

            SetPhysicalResponse(7);

            var buffer = new byte[100];
            var length = _uut.GetMessage(buffer);

            for (var i = 0; i < length; i++)
            {
                Console.Write(Convert.ToChar(buffer[i]));
            }

            Assert.That(length, Is.EqualTo(19));
        }

        [Test]
        public void GetMessage_SpecialChars_CorrectNumber()
        {
            _serialData = "AKBCLLE er BDBCGUDAdsadsaA";

            SetPhysicalResponse(7);

            var buffer = new byte[100];
            var length = _uut.GetMessage(buffer);

            for (var i = 0; i < length; i++)
            {
                Console.Write(Convert.ToChar(buffer[i]));
            }

            Assert.That(length, Is.EqualTo(14));
        }

        [Test]
        public void SendMessage_SpecialChar_CorrectNumber()
        {
            const string message = "KALLE er BAGUD";
            var messageBytes = new byte[message.Length];
            var i = 0;
            foreach (var ch in message)
            {
                messageBytes[i] = Convert.ToByte(ch);
                ++i;
            }

            _uut.SendMessage(messageBytes, message.Length);

            _fakePhysical.Received(1).Write(Arg.Any<byte[]>(), 19);
        }
    }
}