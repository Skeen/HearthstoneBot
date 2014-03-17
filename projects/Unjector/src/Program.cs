using System;
using System.IO;
using System.Diagnostics;
using System.Threading;

namespace HearthstoneBot
{
	public class Program
	{
		public static void Main(string[] args)
		{
            Process[] pname = Process.GetProcessesByName("Hearthstone");
            if (pname.Length == 0)
            {
                Console.WriteLine("Error: Hearthstone not running!");
                Environment.Exit(-1);
            }
            // At this point, Hearthstone will be running

            while(true)
            {
                pname = Process.GetProcessesByName("Hearthstone");
                if (pname.Length == 0)
                {
                    Console.WriteLine("Heartstone just exited!");
                    uninject();
                    return;
                }
                Thread.Sleep(200);
            }
		}

        private static void uninject()
        {
            // Check that the file exists
            if (!File.Exists("injector/path"))
            {
                Console.WriteLine("Unable to find: " + "injector/path");
                Environment.Exit(-1);
            }
            // If it does, read the path and return it
            string hearthstone_path = File.ReadAllText("injector/path");

            // Name of the backup
            string original_assembly = "injector/Assembly-CSharp.original.dll";
            // If the modified assembly charp file, was not found, we'll create it
            if(!File.Exists(original_assembly))
            {
                Console.WriteLine("Unable to find: " + original_assembly);
                Environment.Exit(-1);
            }
            // Check that we can find the file we're going to replace
            string hearthstone_assembly_csharp = hearthstone_path + "/Hearthstone_Data/Managed/Assembly-CSharp.dll";
            // If we're unable to, throw an error
            if(!File.Exists(hearthstone_assembly_csharp))
            {
                // Write error code to user
                Console.WriteLine("Error: Unable to detect Hearthstone file to replace (Assembly-CSharp.dll)!");
                Console.WriteLine("\tCheck the Hearthstone installation (possibly repair)");
                Console.WriteLine("\tCheck the updates to this program");
                Environment.Exit(-1);
            }
            try
            {
                File.Copy(original_assembly, hearthstone_assembly_csharp, true);
            }
            catch(IOException e) // If we were unable to copy the file
            {
                Console.WriteLine("Error: Unable uninject file!");
                Console.WriteLine(e.ToString());
                Environment.Exit(-1);
            }
        }
	}
}
