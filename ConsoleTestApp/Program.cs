using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleTestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            int elementsCount = 10;
            Console.WriteLine($"{DateTime.Now} - Start for {elementsCount} elements");


            Console.WriteLine("\nEnd - press any key to exit...");
            Console.Read();
        }
    }
}
