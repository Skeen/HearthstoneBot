using System;
using System.IO;
using System.Diagnostics;
using System.Threading;

namespace HearthstoneBot
{
	public class Program
	{
        private const string log_directory = "logs/";
        // Default log name
        private const string default_log_name = "AutoBotter.log";
        // Log a message
		private static void log(string msg, string log = default_log_name)
		{
            // Generate log string
            string log_message = DateTime.Now + " : " + msg;
            // Write it to the console (may be lost)
            Console.WriteLine(log_message);
            // Write it to the requested file
            string log_path = log_directory + log;
            using (TextWriter tw = (StreamWriter) File.AppendText(log_path))
            {
                tw.WriteLine(log_message);
            }
		}

        private static void preconditions()
        {
            // Check that the file exists
            if (!File.Exists("injector/path"))
            {
                log("Unable to find: " + "injector/path");
                Environment.Exit(-1);
            }

            Process[] pname = Process.GetProcessesByName("Battle.net");
            if (pname.Length == 0)
            {
                log("Error: Battle.net not running!");
                Environment.Exit(-1);
            }
            // At this point, Hearthstone will be running
            
            Process[] pname2 = Process.GetProcessesByName("Hearthstone");
            if (pname2.Length != 0)
            {
                log("Error: Hearthstone is running!");
                Environment.Exit(-1);
            }
            // At this point, Hearthstone will not be running
        }

        private static bool loader_commandline(string argument)
        {
            Process proc = new Process();
            proc.StartInfo.FileName ="LoaderCommandline.exe";
            proc.StartInfo.Arguments = argument;

            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.CreateNoWindow = true;
            proc.StartInfo.RedirectStandardOutput = true;
            
            log("Starting: LoaderCommandline.exe " + argument);

            proc.Start();
            proc.WaitForExit();

            string result = proc.StandardOutput.ReadToEnd();
            log("Output: " + result);

            return (proc.ExitCode != 0);
        }

        public static bool start_hearthstone(string hearthstone_path)
        {
            string hearthstone_executable = hearthstone_path + "\\Hearthstone.exe";
            // If the executable was found, then assume this is the hearthstone folder
            bool executable_found = File.Exists(hearthstone_executable);
            if(executable_found == false)
            {
                log("Error: Hearthstone.exe not found!");
                Environment.Exit(-1);
            }

            Process proc = new Process();
            proc.StartInfo.FileName = "Hearthstone.exe";
            proc.StartInfo.WorkingDirectory = hearthstone_path;
            
            log("Starting: " + hearthstone_executable);

            proc.Start();

            Thread.Sleep(20000);

            Process[] pname = Process.GetProcessesByName("Hearthstone");
            if (pname.Length == 0)
            {
                return false;
            }
            log("Hearthstone Started!");
            return true;
        }

        private static bool start_unjector()
        {
            Process proc = new Process();
            proc.StartInfo.FileName ="Unjector.exe";

            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.CreateNoWindow = true;
            proc.StartInfo.RedirectStandardOutput = true;
            
            log("Starting: Unjector.exe");

            proc.Start();
            proc.WaitForExit();

            string result = proc.StandardOutput.ReadToEnd();
            log("Output: " + result);

            return (proc.ExitCode != 0);
        }

		public static void Main(string[] args)
		{
            preconditions();
            // If it does, read the path and return it
            string hearthstone_path = File.ReadAllText("injector/path");
            
            // Regen the injection file
            if(loader_commandline("regen_inject"))
                Environment.Exit(-1);

            while(true)
            {
                // Regen the injection file
                if(loader_commandline("inject"))
                    Environment.Exit(-1);

                // Start Hearthstone
                while(start_hearthstone(hearthstone_path) == false)

                // Start the bot
                if(loader_commandline("startbot"))
                    Environment.Exit(-1);

                if(start_unjector())
                {
                    Thread.Sleep(20000);
                }
                Thread.Sleep(10000);
            }
        }
	}
}
