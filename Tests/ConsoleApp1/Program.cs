using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CDBExecutor;

namespace ConsoleApp1 {
    class Program {
        static void Main(string[] args) {
            if (args.Length > 0) {
                Thread.Sleep(TimeSpan.FromSeconds(5));
                M1();
                return;
            }
            var w = CDBExecutor.Worker.Register();
            Console.WriteLine($"[e, exception] to throw {nameof(NotImplementedException)}");
            Console.WriteLine($"[s, stack] to throw {nameof(StackOverflowException)}");
            Console.WriteLine($"[q, quit] to exit");
            Console.WriteLine($"[p, process] to start process");
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
                    case "p":
                    case "process":
                        ExecuteProcess(typeof(Program).Assembly.Location, "abc", 300000000, null);
                        return;
                    default:
                        Console.WriteLine("huh?");
                        return;
                }
            }
        }
        public static int ExecuteProcess(string fileName, string arguments, int timeout, string workingDir) {
            int exitCode;

            using(var process = new Process()) {
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.FileName = fileName;
                process.StartInfo.Arguments = arguments;
                if(!string.IsNullOrEmpty(workingDir))
                    process.StartInfo.WorkingDirectory = workingDir;
                process.OutputDataReceived += (sender, args) => {
                    if(args.Data != null) {
                        Console.WriteLine(args.Data);
                    }
                };
                process.ErrorDataReceived += (sender, args) => {
                    if(args.Data != null) {
                        Console.WriteLine(args.Data);
                    }
                };

                process.Start();

                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                if(process.WaitForExit(timeout)) {
                    exitCode = process.ExitCode;
                } else {
                    process.Kill();
                    process.WaitForExit(30 * 1000);
                    if(!process.HasExited)
                        Console.WriteLine("[WARNING] process.HasExited = false after final WaitForExit(30s)");
                    throw new TimeoutException("Process wait timeout expired for " + fileName + " args: [" + arguments + "] in worker dir: " + workingDir);
                }
            }

            return exitCode;
        }
        static void M1() {
            M1();
        }
    }
}
