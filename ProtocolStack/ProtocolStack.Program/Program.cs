using System;
using System.IO.Ports;

namespace ProtocolStack.Program
{
    class Program
    {
        static void Main(string[] args)
        {
            var msg = new byte[10] { 75, 65, 76, 76, 69, 66, 97, 108, 108, 101 };

            var port = new SerialPort("COM5", 115200, Parity.None, 8 ,StopBits.One);
            
            port.Open();
            port.ReadTimeout = 10000;
            Console.Write("Pre");
            port.Read(msg, 0, 10);
            Console.WriteLine("Post");
            port.Read(msg, 0, 10);
            Console.WriteLine("Post");

            //var link = new Link(1000, new DecryptStm(), new EncryptStm(), new Serial("COM5", 15520, 8)  );
            //link.SendMessage(msg, 10);

            //var transport = new Transport("COM5", 115200, 8);
            //transport.SendMessage(msg, 10);

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
