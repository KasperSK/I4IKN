using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Threading;

namespace UDPSrv
{
    public class UdpServer
    {
        Socket JustSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        private int port = 9999;
        private int received = 0;
        private byte[] data = new byte[1024];
        public bool Running { get; set; }


        public UdpServer()
        {
            try
            {
                IPEndPoint ep = new IPEndPoint(IPAddress.Any, port);
                JustSocket.Bind(ep);
            }
            catch (SocketException e)
            {
                Console.WriteLine("Source: " + e.Source);
                Console.WriteLine("Message: " + e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine("Source: " + e.Source);
                Console.WriteLine("Message: " + e.Message);
            }
            Running = true;
            IPHostEntry host;

            host = Dns.GetHostEntry(Dns.GetHostName());

            Console.WriteLine("GetHostEntry({0}) returns:", "Localhost");

            foreach (IPAddress ip in host.AddressList)
            {
                Console.WriteLine("    {0}", ip);
            }
        }

        public void Run()
        {

            while (Running)
            {
                IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
                EndPoint Remote = (EndPoint)(sender);
                received = JustSocket.ReceiveFrom(data, ref Remote);
                UdpWorker work = new UdpWorker(Remote, Encoding.UTF8.GetChars(data));
                Thread WorkerThread = new Thread(work.Run);
                WorkerThread.Start();
                Console.WriteLine("Got: " + Encoding.UTF8.GetString(data, 0, received));
            }
        }
    }

    public class UdpWorker
    {
        private EndPoint _remote;
        private char _request;
        Socket responseSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        private string badRequest = "You suck at requesting stuff";
        private string uRequest = "/proc/uptime";
        private string lRequest = "/proc/loadavg";

        public UdpWorker(EndPoint remote, char[] request)
        {
            _remote = remote;
            _request = request[0];
        }

        public void Run()
        {
            Console.WriteLine("Switching on: " + _request);
            switch (_request)
            {
                case 'u':
                case 'U':
                    responseSocket.SendTo(Encoding.UTF8.GetBytes(uRequest), uRequest.Length, SocketFlags.None, _remote);
                    var U = HandleFile(uRequest);
                    break;
                case 'l':
                case 'L':
                    responseSocket.SendTo(Encoding.UTF8.GetBytes(lRequest), lRequest.Length, SocketFlags.None, _remote);
                    var L = HandleFile(lRequest);
                    break;
                default:
                    responseSocket.SendTo(Encoding.UTF8.GetBytes(badRequest), badRequest.Length, SocketFlags.None, _remote);
                    break;
            }
        }

        private byte[] HandleFile(string filePath)
        {
            var answer = File.OpenRead(filePath);
            var Content = new BinaryReader(answer).ReadBytes((int)answer.Length);
            foreach (var Byte in Content)
            {
                Console.Write($"{Byte}");
            }
            return Content;
        }
    }
}
