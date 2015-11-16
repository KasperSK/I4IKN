using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LinkLayer;

namespace ProtocolStack.Program
{
    class Program
    {
        static void Main(string[] args)
        {
            var Link = new Link(1000, new DecryptStm(), new EncryptStm(), new Serial("/dev/ttyS1", 115200, 8));
            var msg = new byte[10] {75, 65, 76, 76, 69, 66, 97, 108, 108, 101};
            Link.SendMessage(msg, 10);
        }
    }
}
