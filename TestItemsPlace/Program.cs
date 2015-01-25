using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestItemsPlace
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("Enter value for \"x\": ");
            double x = double.Parse(Console.ReadLine());
            Console.Write("Enter value for \"y\": ");
            double y = double.Parse(Console.ReadLine());

            bool rectangle = ((-1 < x) && (x < 5)) && ((-1 < y) && (y < 1));

            Console.WriteLine(rectangle);
        }
    }
}
