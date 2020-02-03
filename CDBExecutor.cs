using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyTitle("CDBExecutor")]

namespace CDBExecutor {
    public sealed class Worker {
        static Worker instance;
        static readonly object synchronized = new object();
        public static Worker Register() {
            if (instance == null)
                lock (synchronized) {
                    if (instance == null)
                        instance = new Worker();
                }
            return instance;
        }
        readonly string architecture;
        readonly string tempDirectory;
        readonly string scriptDirectory;
        readonly string cdbDirectory;
        public String Architecture {
            get { return this.architecture; }
        }
        public String TempDirectory {
            get { return this.tempDirectory; }
        }
        public String ScriptDirectory {
            get { return this.scriptDirectory; }
        }
        public String DebuggerDirectory {
            get { return this.cdbDirectory; }
        }
        Worker() {
            architecture = IntPtr.Size == 4 ? "x86" : "x64";
            tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());        
            scriptDirectory = Path.Combine(tempDirectory, "CDBExecutor", "Scripts");
            cdbDirectory = Path.Combine(tempDirectory, "CDBExecutor", architecture);
            
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
        public void SetPS1Postscript(Func<String> getPowershellScriptBody) {
            lock (synchronized) {
                var body = getPowershellScriptBody();
                using (var sw = new StreamWriter(Path.Combine(ScriptDirectory, "publish.ps1"), false)) {
                    sw.Write(body);
                }
            }
        }

        void PatchScripts() {
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
        string FixBackslash(string str, string ext) {
            if (ext == ".s")
                return str.Replace(@"\", @"\\");
            return str;
        }

        void ExtractEmbeddedResources() {
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

        string GetTemporaryDirectory()
        {
            string tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            return tempDirectory;
        }
    }
}