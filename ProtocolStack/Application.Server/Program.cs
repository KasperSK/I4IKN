using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApplicationLayerClient;
using ApplicationLayerServer;

namespace Application.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            FileServer server;
            FileClient client;
            if (args[0] == "server")
            {
                var arguments = new string[args.Length];
                if (args[1] != null)
                {
                    arguments[0] = args[1];
                }
                server = new FileServer(arguments);
                server.Run();
                Console.WriteLine("Server Startet");
            }
            else if (args[0] == "client")
            {
                var arguments = new string[args.Length];
                if (args[1] != null)
                {
                    arguments[0] = args[1];
                }
                if (args[2] != null)
                {
                    arguments[1] = args[2];
                }
                client = new FileClient(arguments);
                client.Run();
                Console.WriteLine("Client Startet");
            }
            else
            {
                Console.WriteLine("Nothing Startet :( :( :( :(");
            }
            Console.ReadKey();
        }
    }
}
