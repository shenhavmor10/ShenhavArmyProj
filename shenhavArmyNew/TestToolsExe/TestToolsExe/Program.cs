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
        static string toolExeFolder = @"..\..\..\..\ToolsExe";
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
                StartInfo = { FileName = fileName,Arguments= @"C:\Users\student\Desktop\Test\test.c C:\Users\student\Desktop\Test\test2.c" },
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
                    Directory.GetFiles(toolExeFolder, "*.EXE", SearchOption.AllDirectories)
                    .Select(fileName => Path.GetFileNameWithoutExtension(fileName))
                    .AsEnumerable()
                    .ToArray();
            for(int i=0;i<exes.Length;i++)
            {
                exes[i] = toolExeFolder+"\\"+exes[i]+".exe";
            }
            RunAllTasks(exes);
        }
    }
}
