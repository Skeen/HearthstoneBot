using System;
using System.IO;

namespace HearthstoneBot
{
	public class Log
	{
        // Should be set, when bot is started
        public static string log_directory = "logs/";

        // Default log name
        public const string default_log_name = "Default.log";
        // Default UI log name
        public const string default_ui_log_name = "DefaultUI.log";
        // Default error log name
        public const string default_error_log_name = "DefaultError.log";
        // Default debug log name
        public const string default_debug_log_name = "DefaultDebug.log";

        // Change the log directory
		public static void new_log_directory(string new_log_dir)
		{
            // Save the old log, in case changing fails
            string old_log_dir = log_directory;
            // Try to change log directory
            try
            {
                // Write that we're changing log directory
                Log.log("Changing log_dir to; " + new_log_dir);
                // Change the log directory
                Log.log_directory = new_log_dir;
                // Write to the new default log
                Log.log("New log initialized");
            }
            catch(Exception e) // If an exception happens, we couldn't change the log
            {
                // Revert to the old log
                Log.log_directory = old_log_dir;
                // Write to the old log, that we were unable to change
                Log.log("Unable to change log; " + e.ToString());
            }
		}

        // Write a debug message
		public static void debug(string msg, string log = default_debug_log_name)
		{
			Log.log("DEBUG ::: " + msg, log);
		}

        // Write an error message
		public static void error(string msg, string log = default_error_log_name)
		{
            Log.log("ERROR ::: " + msg, log);
		}

        // Write a message to the UI Screen
		public static void say(string msg, bool is_error = false, string log = default_ui_log_name)
        {
            // Determine if we're to print an error message, or an ordinary message
            if (is_error)
            {
                // Add Error, and print via error printer
			    UIStatus.Get().AddError(msg);
                Log.error("UI ::: " + msg, log);
            }
            else
            {
                // Add Info, and print via logger
			    UIStatus.Get().AddInfo(msg);
                Log.log("UI ::: " + msg, log);
            }
		}

        // Log a message
		public static void log(string msg, string log = default_log_name)
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
	}
}
