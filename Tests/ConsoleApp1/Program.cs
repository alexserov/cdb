using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1 {
    class Program {
        static void Main(string[] args) {
            CDBExecutor.Worker.Start();
            Console.WriteLine($"[e, exception] to throw {nameof(NotImplementedException)}");
            Console.WriteLine($"[s, stack] to throw {nameof(StackOverflowException)}");
            Console.WriteLine($"[q, quit] to exit");
            while (true) {
                var l = Console.ReadLine();
                switch (l) {
                    case "e":
                    case "exception":
                        throw new NotImplementedException();
                    case "s":
                    case "stack":
                        M1();
                        return;
                    case "q":
                    case "quit":
                        return;
                    default:
                        Console.WriteLine("huh?");
                        return;
                }
            }
        }
        static void M1() {
            M1();
        }
    }
}
