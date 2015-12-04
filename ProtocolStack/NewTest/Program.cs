using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using log4net.Config;
using log4net.Repository.Hierarchy;
using LinkLayer;
using Transport;

namespace NewTest
{
    class Program
    {
        static void Main(string[] args)
        {
            ushort a = UInt16.MaxValue - 1;
            ++a;
            ++a;
            ushort b = 4;

            Console.WriteLine("Calc " + (ushort)(b - a));

            BasicConfigurator.Configure();
            


            IPortController portController = new PortController();

            IPort port = portController.GetPort(5);

            ISocket socket1 = new Socket(1000, portController);
            socket1.SourcePort = 0x72;
            socket1.Connect(5,0x73);

            ISocket socket2 = new Socket(1000, portController);
            socket2.SourcePort = 0x65;
            socket2.Connect(5, 0x64);

            ISocketListener listener = new SocketListener(5, 0x64, portController);
            listener.Start();
            var sock = listener.AcceptSocket();
            Console.WriteLine("Flaf");
            

            while (true)
            {
                Thread.Sleep(1000);
            }
        }
    }
}
