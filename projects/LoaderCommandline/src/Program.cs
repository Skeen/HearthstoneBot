using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;

using Mono.Options;

namespace HearthstoneBot
{
	internal static class Program
	{
        private static string set_path(string hearthstone_path)
        {
            // If no path were given, try to read the path file
            if (hearthstone_path == null)
            {
                // Check that the file exists
                if (File.Exists("injector/path"))
			    {
                    // If it does, read the path and return it
                    return File.ReadAllText("injector/path");
                }
                else // Nothing we can do, inform the user, to set it
                {
                    string path = "C:\\Program Files (x86)\\Hearthstone";
                    string hearthstone_executable = path + "\\Hearthstone.exe";
                    // If the executable was found, then assume this is the hearthstone folder
                    bool executable_found = File.Exists(hearthstone_executable);
                    if(executable_found)
                    {
                        // Write the file, and return the string
                        File.WriteAllText("injector/path", path);
                        return path;
                    }
                    else
                    {
                        Console.WriteLine("ERROR: Hearthstone path not set!");
                        Console.WriteLine("\tSet it using --set_hs_path=PATH");
                        Environment.Exit(-1);
                    }
                }
            }
            else // If a path was indeed given
            {
                // Check that the folder, to place the path file in exists
                if (Directory.Exists("injector"))
                {
                    // Write the file, and return the string
				    File.WriteAllText("injector/path", hearthstone_path);
                    return hearthstone_path;
                }
                else // If the folder didn't exist, the installation is broke
                {
                    // Inform the user, to check their installation
                    Console.WriteLine("ERROR: The directory 'injector' does not exists!");
                    Console.WriteLine("\tCheck your installation!");
                    Environment.Exit(-1);
                }
            }
            // Will never happen!
            return null;
        }

        private static void send_bot_path()
        {
            Console.WriteLine(AppDomain.CurrentDomain.BaseDirectory);
            send_bot_command("path=" + AppDomain.CurrentDomain.BaseDirectory);
        }

        private static void regen_inject()
        {
            string modified_assembly_csharp = "injector/Assembly-CSharp.dll";
            // If the modified assembly charp file, was found, we'll remove it
            if(File.Exists(modified_assembly_csharp))
            {
                File.Delete(modified_assembly_csharp);
            }

            string original_assembly_csharp = "injector/Assembly-CSharp.original.dll";
            // If the modified assembly charp file, was found, we'll remove it
            if(File.Exists(original_assembly_csharp))
            {
                File.Delete(original_assembly_csharp);
            }
        }

		private static void Main(string[] args)
		{
            // Parsing output variables
            bool print_help = false;
            string set_path_string = null;

            // Parsing options
            OptionSet option_set = new OptionSet()  
                .Add("?|help|h", "Prints out the options.", v => print_help = v != null)  
                .Add("set_hs_path=", "Set the path of the hearthstone folder", (string v) => set_path_string = v)  
                ;  

            List<string> extra = null;
            // Try to parse the input
            try  
            {  
                extra = option_set.Parse(args);  
            }  
            catch (OptionException /*e*/)  
            {  
                // If we were unable to do so, throw an error
                Console.WriteLine("Unable to parse commands!");
                show_help(option_set);  
                return;
            } 

            if(set_path_string != null)
            {
                // This will never be null, as the program will terminate in that case
                string hearthstone_path = set_path(set_path_string);
                // Validate that the hearthstone folder is indeed legit
                validate_hearthstone_folder(hearthstone_path);
            }
            // If no commands where specified, or if we were asked to print the
            // help, do so
            else if (print_help || extra.Count == 0)
            {
                show_help(option_set);
                return;
            }
            else // We've got a command argument
            {
                // This will never be null, as the program will terminate in that case
                string hearthstone_path = set_path(set_path_string);
                // Validate that the hearthstone folder is indeed legit
                validate_hearthstone_folder(hearthstone_path);

                switch(extra[0])
                {
                    case "inject":
                        inject(hearthstone_path, extra);
                        break;

                    case "regen_inject":
                        create_injection_file(hearthstone_path, true);
                        break;
                    
                    case "status":
                        injection_status(hearthstone_path, extra);
                        break;

                    case "about":
                        print_about();
                        break;

                    case "startbot":
                        startbot(extra);
                        break;

                    case "stopbot":
                        stopbot(extra);
                        break;

                    case "reload":
                        reload_scripts(extra);
                        break;
                }
            }
		}

        static void send_bot_command(string cmd)
		{
            // Setup a TCP client
		    TcpClient client = new TcpClient();
            try
		    {
                // Connect the TCP client
				client.Connect("127.0.0.1", 8111);
            }
            catch(Exception /*e*/)
            {
                Console.WriteLine("ERROR: Unable to open connection to bot!");
                Console.WriteLine("\tCheck that Hearthstone is running, and injected");
                Environment.Exit(-1);
            }
            // Connect to the network stream
		    NetworkStream network_stream = client.GetStream();
			if (client.Connected == false)
            {
                Console.WriteLine("ERROR: Client not connected!");
                Console.WriteLine("\tCheck that Hearthstone is running, and injected");
                Environment.Exit(-1);
            }
            if(network_stream.CanWrite == false)
            {
                Console.WriteLine("ERROR: Unable to write on network stream!");
                Console.WriteLine("\tCheck that Hearthstone is running, and injected");
                Environment.Exit(-1);
            }
            // At this point, we're able to write our message
            StreamWriter sw = new StreamWriter(network_stream);
            sw.WriteLine(cmd);
            sw.Flush();
            // Clean up the resources
            client.Close();
            network_stream.Close();
        }

        static void startbot(List<string> args)
        {
            send_bot_path();
            send_bot_command("start_bot");
        }
        static void stopbot(List<string> args)
        {
            send_bot_command("stop_bot");
        }
        static void reload_scripts(List<string> args)
        {
            send_bot_command("reload_scripts");
        }

        static void print_about()
        {
            Console.WriteLine("LoaderCommandline by Skeen");
            Console.WriteLine("\tSovende@gmail.com");
        }

        static void injection_status(string hearthstone_path, List<string> args)
        {
            string modified_assembly_csharp = "injector/Assembly-CSharp.dll";
            // If the modified assembly charp file, was not found, we'll create it
            if(!File.Exists(modified_assembly_csharp))
            {
                // Let the user know the file is missing!
                Console.WriteLine("Injection file not found, generating");
                // Create the injection file
                create_injection_file(hearthstone_path);
            }
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
            // At this point both files are good
            // Get information about the files
			FileInfo modified_file_info = new FileInfo(modified_assembly_csharp);
			FileInfo hearthstone_file_info = new FileInfo(hearthstone_assembly_csharp);
            // TODO: LowPrio: Check by checksum, rather than by length
            if (modified_file_info.Length == hearthstone_file_info.Length)
			{
                Console.WriteLine("The Hearthstone files are injected!");
                Environment.Exit(0);
            }
            else
            {
                Console.WriteLine("The Hearthstone files are NOT injected!");
                Environment.Exit(-1);
            }
        }

        static void validate_hearthstone_folder(string hearthstone_path)
        {
            string hearthstone_executable = hearthstone_path + "\\Hearthstone.exe";
            // If the executable was found, then assume this is the hearthstone folder
            bool executable_found = File.Exists(hearthstone_executable);
            // If the executable was not found, report this as an error
            if(executable_found == false)
            {
                Console.WriteLine("Error: Invalid hearthstone folder (Heartstone.exe not found)!");
                Console.WriteLine("\tRe-set it using --set_hs_path=PATH");
                Environment.Exit(-1);
            }
        }

        static void create_injection_file(string hearthstone_path, bool renew=false)
        {
            if(renew)
            {
                regen_inject();
            }

            // Check that the injector can be found
            string injector_executable = "Injector.exe";
            string injector_path = "injector/" + injector_executable;
            if(File.Exists(injector_path) == false)
            {
                Console.WriteLine("Error: Injector not found (injector/Injector.exe)!");
                Console.WriteLine("\tCheck your installation!");
                Environment.Exit(-1);
            }
            // Check if the Assembly-CSharp.orig.dll file can be found
            string original_assembly_csharp = "injector/Assembly-CSharp.original.dll";
            if(File.Exists(original_assembly_csharp) == false)
            {
                try
                {
                    // If it cannot be found, let's pull it in, from the Hearthstone folder
                    string hearthstone_assembly_csharp = hearthstone_path + "/Hearthstone_Data/Managed/Assembly-CSharp.dll";
                    File.Copy(hearthstone_assembly_csharp, original_assembly_csharp, true);
                    // Let the user know
                    Console.WriteLine("Pulled in Assembly-CSharp.dll from the Heartstone Folder!");
                }
                catch(IOException e) // If we were unable to copy the file
                {
                    Console.WriteLine("Error: Unable to Assembly-CSharp.dll from Hearthstone!");
                    Console.WriteLine("\tCheck that the Hearthstone folder is correctly set");
                    Console.WriteLine();
                    Console.WriteLine(e.ToString());
                    Environment.Exit(-1);
                }
            }

            // Setup the injector process
            Process process = new Process();
            process.StartInfo.FileName = injector_path;
            //process.StartInfo.Arguments = injector_executable;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.WorkingDirectory = "injector";
            // Inform the user
            Console.WriteLine("Creating injection file...");
            // Start the injector process
            process.Start();
            process.WaitForExit();
            // If non succesfull exit
            if (process.ExitCode != 0)
            {
                // Write error code to user
                Console.WriteLine("Error: Unsuccessfull injection file creation (Error code = " + process.ExitCode + ")!");
                Console.WriteLine("\tCheck the injector log file (logs/injector.log)");
                Environment.Exit(-1);
            }
            else
            {
                Console.WriteLine("Success!");
            }
        }

        static void inject(string hearthstone_path, List<string> args)
        {
            string modified_assembly_csharp = "injector/Assembly-CSharp.dll";
            // If the modified assembly charp file, was not found, we'll create it
            if(!File.Exists(modified_assembly_csharp))
            {
                // Create the injection file
                create_injection_file(hearthstone_path);
            }
            // At this point modified_assembly_csharp exists
            
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
            // At this point we know that we'll be able to inject the file
            Console.WriteLine("Injecting files, into the Heartstone directory");
            IEnumerable<string> files = Directory.EnumerateFiles("injector", "*.dll");
            foreach(string file_name in files)
            {
                // Create the destination name
                string destionation_file_name = hearthstone_path + "/Hearthstone_Data/Managed/" + Path.GetFileName(file_name);
                // And try to copy the file
                try
                {
                    File.Copy(file_name, destionation_file_name, true);
                }
                catch(IOException e) // If we were unable to copy the file
                {
                    // TODO: Check if game is running, to provide better error message
                    Console.WriteLine("Error: Unable to copy/inject files into Hearthstone!");
                    Console.WriteLine("\tMaybe the game is running?");
                    Console.WriteLine("\tFile: " + file_name);
                    Console.WriteLine();
                    Console.WriteLine(e.ToString());
                    Environment.Exit(-1);
                }
            }
            Console.WriteLine("Injected " + files.Count() + " files");
            Console.WriteLine("Ready to launch Hearthstone");
        }

        static void show_help(OptionSet p)
        {
            Console.WriteLine("Usage: LoaderCommandline.exe [OPTIONS]+ command");
            Console.WriteLine();
            Console.WriteLine("Options:");
            p.WriteOptionDescriptions(Console.Out);
            Console.WriteLine();
            Console.WriteLine("Commands:");

            string[][] commands = new string[][]
                {new string[]{"inject",       "Inject the bot into Hearthstone"},
                 new string[]{"regen_inject", "Regenerate the injection file"},
                 new string[]{"status",       "Check the status for injection"},
                 new string[]{"about",        "Write information about this program"},
                 new string[]{"startbot",     "Start the bot"},
                 new string[]{"stopbot",      "Stop the bot"},
                 new string[]{"reload",       "Reload the AI lua scripts"}};

            foreach(string[] command in commands)
            {
                string command_name = command[0];
                string command_help = command[1];

                int spacing_size = 28 - command_name.Count();
                string spacing = new String(' ', spacing_size);

                Console.WriteLine(" " + command_name + spacing + command_help);
            }
        }
	}
}
