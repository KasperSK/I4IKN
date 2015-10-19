using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UDPSrv.Application
{
    class Program
    {
        static void Main(string[] args)
        {
            UdpServer hat = new UdpServer();
            hat.Run();
            Console.ReadKey();
        }
    }
}
