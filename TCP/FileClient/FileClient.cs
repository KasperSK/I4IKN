using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text.RegularExpressions;

namespace FileClient
{
    public class FileClient
    {
        private const string Delimitter = "\r\n\r\n";
        private const string Message = "GET ";
        private const string Version = " FS/1.0";
        private const int Buffer = 1000;
        private const int Port = 9000;
        private string _filePath;
        private IPAddress _clientAddress;
        private bool running;

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
            try
            {
                _tcpClient.Connect(_clientAddress, Port);
            }
            catch (Exception e)
            {
                Console.WriteLine("The connection could not be established: " + e.Message);
                Environment.Exit(1);
            }
            running = false;
        }

        public void SendRequest(string request)
        {
            byte[] toServer = ConvertToBytes(request);
            var stream = _tcpClient.GetStream();
            stream.Write(toServer, 0, toServer.Length);
        }

        public string GetResponce()
        {
            var stream = _tcpClient.GetStream();
            byte[] fromServer = new byte[1000];
            bool gotRespone = false;
            int limitCount = 0;
            string responce = "";

            while (!gotRespone)
            {
                var ch = (char)stream.ReadByte();
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
            }
            return responce;
        }

        public byte[] GetFile(int lenght)
        {
            var stream = GetStream();
            var fileBytes = new byte[lenght];
            int position = 0;
            int bytesToRead = Buffer;
            while (lenght > 0)
            {
                var noBytesRead = stream.Read(fileBytes, position, bytesToRead);
                lenght -= noBytesRead;
                position += noBytesRead;
                noBytesRead = 0;
                if (lenght < Buffer)
                {
                    bytesToRead = lenght;
                }
            }   
            return fileBytes;
        }

        public void GetBigFile(int lenght, string path)
        {
            var stream = GetStream();
            var fileBytes = new byte[Buffer];
            int bytesToRead = Buffer;
            var file = System.IO.File.OpenWrite(path);
            while (lenght > 0)
            {
                var noBytesRead = stream.Read(fileBytes, 0, bytesToRead);
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

        public string CalculateSha1(byte[] toCalc)
        {
            using (var cryptoProvider = new SHA1CryptoServiceProvider())
            {
                string hash = BitConverter
                        .ToString(cryptoProvider.ComputeHash(toCalc));
                return hash.Replace("-", "").ToLower();
            }
        }

        public void SaveFile(byte[] toSave, string path)
        {
            var file = System.IO.File.OpenWrite(path);
            file.Write(toSave, 0, toSave.Length);
            file.Close();
        }

        public string[] ParseString(string str)
        {
            char[] delimitter = {' ', '\r'};
            string[] parts = str.Split(delimitter, StringSplitOptions.RemoveEmptyEntries);
            return parts;
        }

        public NetworkStream GetStream()
        {
            return _tcpClient.GetStream();
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
            return (Message + _filePath + Version + Delimitter);
        }

        public void DrawDisplay()
        {
            Console.Clear();
            Console.WriteLine("Welcome to the file client" + Version);
            Console.WriteLine("Enter a file name:");
        }

        public void GetFilePath()
        {
            var path = Console.ReadLine();
            _filePath = path;
        }

        public void Run()
        {
            running = true;
            while (running)
            {
                DrawDisplay();
                GetFilePath();
                SendRequest(ConstructRequest());
                var response = ParseString(GetResponce());
            /*    foreach (var str in response)
                {
                    Console.WriteLine(str);
                }
                Console.ReadKey();*/
                switch (response[1])
                {
                    case "200":
                        var getSize = 0;
                        int.TryParse(response[6], out getSize);
                        GetBigFile(getSize, "tophat.jpg");
                        break;
                    case "404":
                        Console.WriteLine(response[0]+ " " + response[1] + " " + response[3]);
                        break;

                }
                var sha = CalculateSha1("tophat.jpg");
                if (sha == response[4])
                {
                    Console.WriteLine("File recieved");
                    running = false;
                }
                char quit;
                var input = Console.ReadKey(false);
                if (input.KeyChar == 'q' || input.KeyChar == 'Q')
                    running = false;
            }
        }
    }
}
