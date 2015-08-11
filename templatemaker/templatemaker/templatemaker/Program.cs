using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace templatemaker
{
    class Program
    {
        static void Main(string[] args)
        {
            new DolphinConfig(File.ReadAllLines(args[0]));
        }
    }
}
