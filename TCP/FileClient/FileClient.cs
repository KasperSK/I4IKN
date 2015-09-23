using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace FileClient
{
    public class FileClient
    {
        private const string Delimitter = "\r\n";
        private const string Message = "GET ";
        private const string Version = " FS/1.0";
        private const int Buffer = 1000;
        private const int Port = 9000;
        private string _filePath;
        private IPAddress _clientAddress;

        TcpClient _tcpClient = new TcpClient();

        public FileClient(string[] args)
        {
            try
            {
                _clientAddress = IPAddress.Parse(args[0]);
            }
            catch(Exception e)
            {
                Console.WriteLine("You did not specify a valid IP adress: Exeption " + e.Message + " was thrown");
            }
            _filePath = args[1];
            _tcpClient.Connect(_clientAddress, Port);
        }

        public void SendRequest(string request)
        {
            byte[] toServer = ASCIIEncoding.Convert(request);
            var stream = _tcpClient.GetStream();
  
            stream.WriteAsync(toServer, 0, 1000);
            stream.ReadAsync(toServer,0,1000);
        }

        public string ConstructRequest(string path)
        {
            return (Message + _filePath + Version + Delimitter + Delimitter);
        }
    }
}
