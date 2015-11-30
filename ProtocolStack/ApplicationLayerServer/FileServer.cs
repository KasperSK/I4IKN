using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using TransportLayer;

namespace ApplicationLayerServer
{
    public class FileServer
    {
        private ITransport _homeBrew = new Transport();

        private const string Delimitter = "\r\n\r\n";
        private byte[] fileBuffer;
        private byte[] msgBuffer;
        private string _comPort;
        public bool Running = false;

        public FileServer(string[] args)
        {
            if (args[0] != null)
            {
                _comPort = args[0];
            }
            fileBuffer = new byte[1000];
            msgBuffer = new byte[1000];
        }
        private void Connect()
        {
            _homeBrew.Connect(_comPort, 115200, 8);
        }


        public void FsProtocol(int lenght)
        {
            var response = GetResponce(lenght);
            Console.WriteLine("Got request " + response);
            var parts = ParseString(response);
            foreach (var part in parts)
            {
                Console.WriteLine(part);
            }
            if (ValidateStringParts(parts))
            {
                if (parts[0] == "GET")
                {
                    if (File.Exists(parts[1]))
                    {
                        Console.WriteLine("Sending the file");

                        var sha = CalculateSha1(parts[1]);
                        var file = File.OpenRead(parts[1]);
                        var len = file.Length;
                        SendRequest("FS/1.0 200 OK\r\n" + "Sha1: " + sha + "\r\n" + "Content-Length: " + file.Length + "\r\n\r\n");
                        SendFile(parts[1], len);

                    }
                    else
                    {
                        SendRequest("FS/1.0 404 File Not Found\r\n\r\n");
                    }
                }
            }
            else
            {
                SendRequest("FS/1.0 400 BadRequest\r\n\r\n");
            }
        }

        public void SendFile(string path, long length)
        {
            var file = File.OpenRead(path);
            int offset = 0;
            int bytestosend = 1000;
            while (length > 0)
            {
                var read = file.Read(fileBuffer, 0, bytestosend);
                _homeBrew.SendMessage(fileBuffer, read);
                length -= read;
                offset += read;
                if (length < bytestosend)
                {
                    bytestosend = (int)length;
                }
            }
        }

        public void SendRequest(string request)
        {
            byte[] toServer = ConvertToBytes(request);
            _homeBrew.SendMessage(toServer, toServer.Length);
        }

        public byte[] ConvertToBytes(string toConvert)
        {
            UTF8Encoding en = new UTF8Encoding();
            return en.GetBytes(toConvert);
        }

        public bool ValidateStringParts(string[] parts)
        {
            return parts[2] == "FS/1.0";
        }

        public string[] ParseString(string str)
        {
            char[] delimitter = { ' ', '\r' };
            string[] parts = str.Split(delimitter, StringSplitOptions.RemoveEmptyEntries);
            return parts;
        }

        public string CalculateSha1(string path)
        {
            var file = System.IO.File.OpenRead(path);
            using (var cryptoProvider = new SHA1CryptoServiceProvider())
            {
                string hash = BitConverter
                        .ToString(cryptoProvider.ComputeHash(file));
                file.Close();
                return hash.Replace("-", "").ToLower();
            }
        }

        public string GetResponce(int length)
        { 
            bool gotRespone = false;
            int limitCount = 0;
            string responce = "";
            var i = 0;

            while (!gotRespone)
            {
                var ch = (char)msgBuffer[i];
                switch (ch)
                {
                    case '\r':
                        responce += ch;
                        limitCount++;
                        break;
                    case '\n':
                        if (limitCount >= 3)
                            gotRespone = true;
                        responce += ch;
                        limitCount++;
                        break;
                    default:
                        limitCount = 0;
                        responce += ch;
                        break;
                }
                if (i == length)
                {
                    responce = ":/";
                    break;
                }
                ++i;
            }
            return responce;
        }

        public void Run()
        {
            Running = true;
            Console.WriteLine("Starting server");
            Connect();
            while (Running)
            {
                var clientRequest = _homeBrew.ReceiveMessage(msgBuffer, 1000);
                FsProtocol(clientRequest);
                _homeBrew.Synced = false;
            }
        }
    }
}
