using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace CDBExecutor {
    public class Worker {
        static Worker() {
            var tempfilename = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            foreach (var name in typeof(Worker).Assembly.GetManifestResourceNames()) {
                using (var stream = typeof(Worker).Assembly.GetManifestResourceStream(name)) {
                    
                    var sname = name.Split('.');
                    var writeTo = Path.Combine(tempfilename, String.Concat(sname.Take(sname.Length - 2).Select(x => x + "\\")), $"{sname[sname.Length - 2]}.{sname[sname.Length - 1]}");
                    var writeToDir = Path.GetDirectoryName(writeTo);
                    if (!Directory.Exists(writeToDir))
                        Directory.CreateDirectory(writeToDir);

                    using (var fs = new FileStream(writeTo, FileMode.CreateNew, FileAccess.Write)) {
                        stream.CopyTo(fs);                        
                    }
                }
            }

            var bitness = IntPtr.Size == 4 ? "x86" : "x64";
            var cmd = Process.Start(new ProcessStartInfo("cmd.exe", $"/c \"{Path.GetFileName(Assembly.GetEntryAssembly().Location)}\" {bitness}") {
                CreateNoWindow = true,
                UseShellExecute =  false,
                RedirectStandardOutput = true
            });
            cmd.Start();
            cmd.WaitForExit();
            // cmd.OutputDataReceived += (sender, args) => Console.WriteLine(args.Data);
            // cmd.BeginOutputReadLine();
            // proc
        }
        public string GetTemporaryDirectory()
        {
            string tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            return tempDirectory;
        }
        [MethodImpl(MethodImplOptions.NoInlining|MethodImplOptions.NoOptimization)]
        public static void Start(){ }
    }
}