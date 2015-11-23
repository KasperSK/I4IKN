using System;
using System.IO.Ports;
using TransportLayer;

namespace ProtocolStack.Program
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(args[0]);
            var msg = new byte[10] { 75, 65, 76, 76, 69, 66, 97, 108, 108, 101 };

            //var port = new SerialPort("COM20", 115200, Parity.None, 8 ,StopBits.One);

            var msg2 = new byte[] {0, 0, 75, 65, 76, 76, 69, 66, 97, 108, 108, 101};
            var checksum = new Checksum();

            checksum.CalculateCheckSum(msg2);

            if(checksum.VerifyCheckSum(msg2))
                Console.WriteLine("True");
            
            //port.Open();
            //port.ReadTimeout = 10000;
            //Console.Write("Pre");
            //port.Read(msg, 0, 10);
            //Console.WriteLine("Post");
            //port.Read(msg, 0, 10);
            //Console.WriteLine("Post");

            //var link = new Link(1000, new DecryptStm(), new EncryptStm(), new Serial("COM5", 15520, 8)  );
            //link.SendMessage(msg, 10);
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
