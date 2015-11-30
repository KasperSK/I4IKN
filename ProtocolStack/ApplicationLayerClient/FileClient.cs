using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Remoting.Channels;
using System.Security.Cryptography;
using System.Text;
using TransportLayer;

namespace ApplicationLayerClient
{
    public class FileClient
    {
        private const string Delimitter = "\r\n\r\n";
        private const string Message = "GET ";
        private const string Version = " FS/1.0";
        private const int Buffer = 1000;
        private string _comPort;
        private string _filePathServer;
        private string _filePathClient;
        private bool running;

        private ITransport _homeBrew = new Transport();

        public FileClient(string[] args)
        {
            if (args[0] != null)
                _comPort = args[0];
            if (args[1] != null)
            {
                _filePathServer = args[1];
                _filePathClient = args[1];
            }

            running = false;
        }

        private void Connect()
        {
            _homeBrew.Connect(_comPort, 115200, 8);
        }

        public void SendRequest(string request)
        {
            byte[] toServer = ConvertToBytes(request);
            _homeBrew.SendMessage(toServer, toServer.Length);
        }

        public string GetResponce()
        {
            byte[] fromServer = new byte[1000];
            var length = _homeBrew.ReceiveMessage(fromServer, 1000);
            bool gotRespone = false;
            int limitCount = 0;
            string responce = "";
            var i = 0;

            while (!gotRespone)
            {
                var ch = (char)fromServer[i];
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

        public void GetBigFile(int lenght, string path)
        {
            var fileBytes = new byte[Buffer];
            int bytesToRead = Buffer;
            if(System.IO.File.Exists(path))
                System.IO.File.Delete(path);
            var file = System.IO.File.OpenWrite(path);
            while (lenght > 0)
            {
                var noBytesRead = _homeBrew.ReceiveMessage(fileBytes, bytesToRead);
                file.Write(fileBytes, 0, noBytesRead);
                lenght -= noBytesRead;
                noBytesRead = 0;
                if (lenght < Buffer)
                {
                    bytesToRead = lenght;
                }
            }
            file.Close();      
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

        public string[] ParseString(string str)
        {
            char[] delimitter = {' ', '\r'};
            string[] parts = str.Split(delimitter, StringSplitOptions.RemoveEmptyEntries);
            return parts;
        }

        public char[] ConvetToChar(byte[] toConvert)
        {
            UTF8Encoding en = new UTF8Encoding();
            return en.GetChars(toConvert);
        }

        public byte[] ConvertToBytes(string toConvert)
        {
            UTF8Encoding en = new UTF8Encoding();
            return en.GetBytes(toConvert);
        }

        public string ConstructRequest()
        {
            return (Message + _filePathServer + Version + Delimitter);
        }

        public void DrawDisplay()
        {
            Console.Clear();
            Console.WriteLine("Welcome to the file client" + Version);
            Console.WriteLine("Using com port " + _comPort);
            Console.WriteLine("File: " + _filePathServer);
            Console.WriteLine("\r\n");
            Console.WriteLine("What would you like to do?");
            Console.WriteLine("1. Request file");
            Console.WriteLine("2. Change file to request");
            Console.WriteLine("3. Change server IP");
            Console.WriteLine("Press Q to quit");
            Console.WriteLine("\r\n");
        }

        public void GetFilePath()
        {
            Console.WriteLine("Please input new file path");
            var path = Console.ReadLine();
            _filePathServer = path;
        }

        public string ParseResponse(string responseString)
        {
            var response = ParseString(responseString);
            switch (response[1])
            {
                case "200":
                    var getSize = 0;
                    int.TryParse(response[6], out getSize);
                    GetBigFile(getSize, "tophat.jpg");
                    return response[4];
                case "404":
                    Console.WriteLine(response[0] + " " + response[1] + " " + response[3]);
                    return null;
                default:
                    return null;
            }
            
        }

        public void CheckSha1(string serverSha)
        {
            var sha = CalculateSha1("tophat.jpg");
            if (sha == serverSha)
            {
                Console.WriteLine("File recieved");
                Console.WriteLine("Recived sha: \t" + serverSha);
                Console.WriteLine("Calculated sha: \t" + sha);
                running = false;
            }
        }

        public void Run()
        {
            Connect();
            running = true;
            while (running)
            {
                DrawDisplay();
                var input = Console.ReadKey(false);
                switch (input.KeyChar)
                {
                    case '1':
                        SendRequest(ConstructRequest());
                        var shaFromServer = ParseResponse(GetResponce());
                        if (shaFromServer != null)
                        {
                            CheckSha1(shaFromServer);
                        }
                        break;
                    case '2':
                        GetFilePath();
                        break;
                    default:
                        Console.WriteLine("Default");
                        DrawDisplay();
                        break;
                }
                if (input.KeyChar == 'q' || input.KeyChar == 'Q')
                    running = false;
            }
        }
    }
}
