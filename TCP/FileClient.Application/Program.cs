using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileClient.Application
{
    class Program
    {
        static void Main(string[] args)
        {
            FileClient hat = new FileClient(args);
            hat.Run();
/*            hat.SendRequest(hat.ConstructRequest());
            var response = hat.ParseString(hat.GetResponce());
            foreach (var str in response)
            {
                Console.WriteLine(str.Replace("\r\n", ""));
            }
//            var file = hat.GetFile(41926);
//            var sha = hat.CalculateSha1(file);
            var getSize = 0;
            int.TryParse(response[6], out getSize);
            Console.WriteLine(getSize);
            hat.GetBigFile(getSize, "tophat.jpg");
            var sha = hat.CalculateSha1("tophat.jpg");
            Console.WriteLine(sha);
            if (sha == response[4])
            {
                Console.WriteLine("MatcH!");
            }
            else
            {
                Console.WriteLine("No match :/");
            }*/
            Console.ReadKey();
        }
    }
}
