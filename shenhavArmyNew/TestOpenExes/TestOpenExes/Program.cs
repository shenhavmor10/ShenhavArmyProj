using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TestOpenExes
{
    class Program
    {
        public static void RunAllTasks(string[] exe)
        {
            for (int i = 0; i < exe.Length; i++)
            {
                RunProcessAsync(exe[i]);
            }
        }
        public static Task<int> RunProcessAsync(string fileName)
        {
            var tcs = new TaskCompletionSource<int>();

            var process = new Process
            {
                StartInfo = { FileName = fileName },
                EnableRaisingEvents = true
            };

            process.Exited += (sender, args) =>
            {
                tcs.SetResult(process.ExitCode);
                process.Dispose();
            };

            process.Start();

            return tcs.Task;
        }
        static void Main(string[] args)
        {
            String[] exes =
                    Directory.GetFiles(@"..\..\..\ToolsExe", "*.EXE", SearchOption.AllDirectories)
                    .Select(fileName => Path.GetFileNameWithoutExtension(fileName))
                    .AsEnumerable()
                    .ToArray();
            Thread threadOpenTools = new Thread(() => RunAllTasks(exes));
            threadOpenTools.Start();
        }
    }
}
