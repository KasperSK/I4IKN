using System;
using System.Threading;
using log4net.Appender;
using log4net.Config;
using log4net.Core;
using log4net.Filter;
using log4net.Layout;
using ProtocolStack.Program.Properties;
using TransportLayer;

namespace ProtocolStack.Program
{

    public class MyServer
    {
        private readonly string _port;

        public MyServer(string port)
        {
            _port = port;
        }
        public void Start()
        {

            ITransport transport = new Transport();
            transport.Connect(_port, 115200, 8);

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
        private static string SetupCom(string name, string value, bool force)
        {
            if (force || value == "COM#")
            {
                Console.Write("Enter port for " + name + " ["+value+"]: ");
                var input = Console.ReadLine() ?? value;

                if (input == "")
                    input = value;

                value = input;

                if (!value.StartsWith("COM"))
                    value = "COM" + value;
            }
            Console.WriteLine(name + " = " + value);
            return value;
        }
        private static void SetupComs(bool force = false)
        {
            Settings.Default.PhyCOM1 = SetupCom("PhyCom1", Settings.Default.PhyCOM1, force);
            Settings.Default.PhyCOM2 = SetupCom("PhyCom2", Settings.Default.PhyCOM2, force);
            Settings.Default.VirCOM1 = SetupCom("VirCom1", Settings.Default.VirCOM1, force);
            Settings.Default.VirCOM2 = SetupCom("VirCom2", Settings.Default.VirCOM2, force);
            Settings.Default.Save();
        }

        private static IAppender MakeLog(string name, Level minLevel, Level maxLevel)
        {
            var layout = new PatternLayout("[%thread] %-5level %logger - %message %newline");
            layout.ActivateOptions();

            var nameFilter = new LoggerMatchFilter
            {

                AcceptOnMatch = true,
                LoggerToMatch = name,
                Next = new DenyAllFilter()
            };

            var levelFilter = new LevelRangeFilter
            {
                AcceptOnMatch = false,
                LevelMin = minLevel,
                LevelMax = maxLevel,
                Next = nameFilter
            };

            var appender = new ConsoleAppender
            {
                Layout = layout
            };
            appender.AddFilter(levelFilter);



            return appender;

        }
        static void Main(string[] args)
        {
            
            var appenders = new []
            {
                MakeLog("LinkLayer.Serial", Level.Debug, Level.Fatal),
                MakeLog("TransportLayer.ReceiverStmContext", Level.Debug, Level.Fatal),
                MakeLog("LinkLayer.Link", Level.Debug, Level.Fatal)
            };


            BasicConfigurator.Configure(appenders);

            var t = new byte[] {200};

            DataType g = (DataType) t[0];

            Console.WriteLine(Enum.IsDefined(typeof (DataType),g));
            
        

            string com1;
            string com2;

            SetupComs();

            // Set true for Physical
            if (false)
            {
                com1 = Settings.Default.PhyCOM1;
                com2 = Settings.Default.PhyCOM2;
            }
            else
            {
                com1 = Settings.Default.VirCOM1;
                com2 = Settings.Default.VirCOM2;
            }

            var msgSmall = new byte[10] { 75, 65, 76, 76, 69, 66, 97, 108, 108, 101 };

            var msgLong = new byte[3000];

            for (int i = 0; i < 3000; i++)
            {
                msgLong[i] = (byte) ((i / 1000) + 97);
            }
            
            var server = new MyServer(com1);
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
            client.Connect(com2, 115200, 8);
            //var link = Factory.GetLink(new SerialPort(com2,115200,Parity.None,8,StopBits.One), 1004, 1);
            
            //IPhysical phys;
            //phys = new Serial(new SerialPort(com2, 115200, Parity.None, 8, StopBits.One),2010,1);

            while (true)
            {
                var key = Console.ReadKey().Key;
                switch (key)
                {
                    //case ConsoleKey.A:
                    //    phys.Write(new byte[]{65, 75, 75, 75, 65},5);
                     //   break;
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
