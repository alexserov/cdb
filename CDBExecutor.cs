using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

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
        bool managedOnly = true;
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
        public bool ManagedOnly {
            get { return this.managedOnly; }
            set {
                if (this.managedOnly == value)
                    return;
                this.managedOnly = value;
                UpdateDebuggerScript((i, s) => {
                    if (!s.StartsWith("!EEStack"))
                        return s;
                    return this.managedOnly ? "!EEStack -EE" : "!EEStack";
                });
            }
        }
        Worker() {
            architecture = IntPtr.Size == 4 ? "x86" : "x64";
            tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());        
            scriptDirectory = Path.Combine(tempDirectory, "CDBExecutor", "Scripts");
            cdbDirectory = Path.Combine(tempDirectory, "CDBExecutor", architecture);
            
            SetErrorMode(ErrorModes.SYSTEM_DEFAULT);
            
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
        [Flags]
        public enum ErrorModes : uint {
            SYSTEM_DEFAULT = 0x0,
            SEM_FAILCRITICALERRORS = 0x0001,
            SEM_NOALIGNMENTFAULTEXCEPT = 0x0004,
            SEM_NOGPFAULTERRORBOX = 0x0002,
            SEM_NOOPENFILEERRORBOX = 0x8000
        }
        [DllImport("kernel32.dll")]
        static extern ErrorModes SetErrorMode(ErrorModes uMode);
        public void SetPS1Postscript(Func<String> getPowershellScriptBody) {
            lock (synchronized) {
                var body = getPowershellScriptBody();
                using (var sw = new StreamWriter(Path.Combine(ScriptDirectory, "publish.ps1"), false)) {
                    sw.Write(body);
                }
            }
        }
        void UpdateDebuggerScript(Func<int, string, string> updateLine) {
            lock (synchronized) {
                StringBuilder dsBuilder = new StringBuilder();
                var filename = Path.Combine(ScriptDirectory, "d.s");
                using (var sr = new StreamReader(filename)) {
                    for (int i = 0; sr.Peek() != -1; i++) {
                        var line = sr.ReadLine();
                        dsBuilder.AppendLine(updateLine(i, line));
                    }
                }

                using (var sw = new StreamWriter(filename, false)) {
                    sw.Write(dsBuilder.ToString());
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