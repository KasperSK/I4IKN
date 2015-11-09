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
        private byte[] defaultIP = {192, 168, 135, 133};
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
                _clientAddress = new IPAddress(defaultIP);
            }
            if(args[1] != null)
                _filePath = args[1];
            running = false;
        }

        public void Close()
        {
            _tcpClient.Close();
        }

        public void Connect()
        {
            try
            {
                _tcpClient.Connect(_clientAddress, Port);
            }
            catch (Exception e)
            {
                Console.WriteLine("The connection could not be established: " + e.Message);
                Environment.Exit(1);
            }
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
            if(System.IO.File.Exists(path))
                System.IO.File.Delete(path);
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
            Console.WriteLine("Server IP: " + _clientAddress.ToString());
            Console.WriteLine("File: " + _filePath);
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
            _filePath = path;
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

        public void ChangeIP()
        {
            Close();
            Console.WriteLine("Please input a new IP adresse in IPv4 format");
            var ip = Console.ReadLine();
            try
            {
                _clientAddress = IPAddress.Parse(ip);
            }
            catch (Exception e)
            {
                Console.WriteLine("You did not specify a valid IP adress: Exeption " + e.Message + " was thrown" + " Using dafault IP");
                _clientAddress = new IPAddress(defaultIP); 
            }
            Connect();
        }

        public void Run()
        {
            running = true;
            Connect();
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
                    case '3':
                        ChangeIP();
                        break;
                    default:
                        Console.WriteLine("Default");
                        DrawDisplay();
                        break;
                }


                if (input.KeyChar == 'q' || input.KeyChar == 'Q')
                    running = false;
            }
            Close();
        }
    }
}
