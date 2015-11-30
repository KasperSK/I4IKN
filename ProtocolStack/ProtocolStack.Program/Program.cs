using System;
using System.Threading;
using TransportLayer;

namespace ProtocolStack.Program
{

    public class MyServer
    {
        public void Start()
        {

            ITransport transport = new Transport();
            transport.Connect("COM7", 115200, 8);

            var data = new byte[Transport.MaxMessageDataSize];
            Console.WriteLine("Server up and running");
            while (true)
            {
                var len = transport.ReceiveMessage(data, data.Length);
                Console.Write("Message: ");
                for (var i = 0; i < len; i++)
                {
                    Console.Write((char)data[i]);
                }
                Console.WriteLine("");
                if (len == 1)
                {
                    return;
                }
            }
        }
    }

    static class Program
    {
        

        static void Main(string[] args)
        {
            
            var msgSmall = new byte[10] { 75, 65, 76, 76, 69, 66, 97, 108, 108, 101 };

            var msgLong = new byte[3000];

            for (int i = 0; i < 3000; i++)
            {
                msgLong[i] = (byte) ((i / 1000) + 97);
            }
            
            var server = new MyServer();
            var serverThread = new Thread(server.Start);
            serverThread.Start();

            /*
            var fakeChecksum = Substitute.For<IChecksum>();
            fakeChecksum.VerifyChecksum(Arg.Any<Message>()).Returns(x => ((Message)x[0]).Checksum == 24929);
            fakeChecksum.When(x => x.GenerateChecksum(Arg.Any<Message>())).Do(x => { x.Arg<Message>().Checksum = 24929; });
            

            var seq = Substitute.ForPartsOf<SequenceGenerator>();
            seq.When(x => x.Reset()).DoNotCallBase();
            seq.Sequence = 97;
            */

            ITransport client = new Transport();
            client.Connect("COM8", 115200, 8);

            while (true)
            {
                var key = Console.ReadKey().Key;
                switch (key)
                {
                    case ConsoleKey.W:
                        client.SendMessage(msgSmall, msgSmall.Length);
                        break;

                    case ConsoleKey.E:
                        client.SendMessage(msgLong, msgLong.Length);
                        break;
                }
            }
            /*
            Sync AaanaA

Data1     AaaobaaaaA
	 
Data2	  AaapbbfdbfdA
	 
Data3	 AaaqbfjdsklafjdsaA
	 
Data4	 AaarbfjdsklafjdsaA

            */


            /*
                    var fakeChecksum = Substitute.For<IChecksum>();
                        fakeChecksum.VerifyChecksum(Arg.Any<Message>()).Returns(true);
                        fakeChecksum.When(x => x.GenerateChecksum(Arg.Any<Message>())).Do(x => { x.Arg<Message>().Checksum = 24929; });

                        var fakeSequence = Substitute.For<ISequenceGenerator>();
                        fakeSequence.Sequence.Returns((byte) 'n');

                        //ISender sender = new SenderStmContext(Factory.GetLink("COM5",1004,10000), fakeChecksum, fakeSequence, 1000, 5000);

                        IReceiver  receiver = new ReceiverStmContext(Factory.GetLink("COM5", 1004, 10000), fakeChecksum, new SequenceGenerator(), 1000);
                        receiver.ReceiveData(msg);
                        receiver.ReceiveData(msg);
                        //sender.SyncUp();
                        //sender.SendData(msg, 10);
                        */
            //var port = new SerialPort("COM20", 115200, Parity.None, 8 ,StopBits.One);

            //var msg2 = new byte[] {0, 0, 75, 65, 76, 76, 69, 66, 97, 108, 108, 101};
            //var checksum = new Checksum();

            //checksum.CalculateCheckSum(msg2);

            //if(checksum.VerifyCheckSum(msg2))
            //    Console.WriteLine("True");

            //port.Open();
            //port.ReadTimeout = 10000;
            //Console.Write("Pre");
            //port.Read(msg, 0, 10);
            //Console.WriteLine("Post");
            //port.Read(msg, 0, 10);
            //Console.WriteLine("Post");

            //var link = new Link(1000, new DecryptStm(), new EncryptStm(), new Serial("COM5", 15520, 8)  );
            //link.SendMessage(msg, 10);


            /*
            var toMe = new byte[1000];

            var transport = new Transport(args[0], 115200, 8);
            if(args[1] == "sender")
                transport.SendMessage(msg, 10);
            if (args[1] == "receiver")
            {
               var whatIgot = transport.ReceiveMessage(toMe);
                for (int i = 0; i < whatIgot; i++)
                {
                    Console.Write(Convert.ToChar(toMe[i]));
                }
                Console.WriteLine(Environment.NewLine);
            }
            */


            //var Link = new Link(1000, new DecryptStm(), new EncryptStm(), new Serial("/dev/ttyS1", 115200, 8));
            //Link.SendMessage(msg, 10);
            //var read = new byte[20];
            //while (Link.GetMessage(read) == 0)
            //{
            //    Console.WriteLine("Trying to read!");
            //    Thread.Sleep(1000);
            //}
        }
    }
}
