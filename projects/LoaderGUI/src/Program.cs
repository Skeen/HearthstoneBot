using System.Windows.Forms;
using System.Drawing;
using System;
using System.Diagnostics;
using System.IO;

namespace HearthstoneBot
{
    public class GUI : Form
    {
        private const int WIDTH = 340;
        private const int HEIGHT = 170;

        private const string log_directory = "logs/";
        // Default log name
        private const string default_log_name = "GUI.log";
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

        private void OnExit(object sender, EventArgs e)
        {
            log("OnExit called, closing GUI");
            Close();
        }

        private void OnLogs(object sender, EventArgs e)
        {
            log("OnLogs called, opening logs dir");

            string windir = Environment.GetEnvironmentVariable("WINDIR");

            Process proc = new Process();
            proc.StartInfo.FileName = windir + @"\explorer.exe";
            proc.StartInfo.Arguments = AppDomain.CurrentDomain.BaseDirectory + log_directory;
            proc.Start();
        }

        private void OnAbout(object sender, EventArgs e)
        {
            log("OnAbout called, showing dialog");

            const string message = "HearthstoneBot by Skeen\n" +
                                   "Modified by HearthstoneBot\n" +
                                   "hearthstonebot@yahoo.com";
            const string caption = "About";
            MessageBox.Show(message, caption,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Asterisk);
        }

        private void OnGithub(object sender, EventArgs e)
        {
            log("OnGithub called, opening default browser");

            Process.Start("https://github.com/HearthstoneBot/HearthstoneBot");
        }

        private MainMenu setupMainMenu()
        {
            log("setupMainMenu called, creating menu bar");

            MainMenu mainMenu = new MainMenu();

            MenuItem file = mainMenu.MenuItems.Add("File");
            file.MenuItems.Add(new MenuItem("Exit", new EventHandler(OnExit)));
            file.MenuItems.Add(new MenuItem("Logs", new EventHandler(OnLogs)));

            MenuItem info = mainMenu.MenuItems.Add("Help");
            info.MenuItems.Add(new MenuItem("About", new EventHandler(OnAbout)));
            info.MenuItems.Add(new MenuItem("Github", new EventHandler(OnGithub)));

            return mainMenu;
        }

        private string getHSPath()
        {
            // Check that the file exists
            if (File.Exists("injector/path"))
            {
                // If it does, read the path and return it
                return File.ReadAllText("injector/path");
            }
            else
            {
                return "Hearthstone path not set...";
            }
        }

        private StatusBar sb;
        private void setStatus(string str)
        {
            sb.Text = str;
        }

        private bool check_hs_path(string hearthstone_path)
        {
            string hearthstone_executable = hearthstone_path + "\\Hearthstone.exe";
            // If the executable was found, then assume this is the hearthstone folder
            bool executable_found = File.Exists(hearthstone_executable);
            return executable_found;
        }

        private void onKeyUpEvent(object sender, EventArgs e)
        {
            string hearthstone_path = ((TextBox) sender).Text;
            Console.WriteLine(hearthstone_path);
                
            // If the executable was not found, report this as an error
            if(check_hs_path(hearthstone_path) == true)
            {
                // Check that the folder, to place the path file in exists
                if (Directory.Exists("injector"))
                {
                    log("Wrote hs path file");

                    // Write the file, and return the string
                    File.WriteAllText("injector/path", hearthstone_path);
                    // Open an info box
                    const string message = "Valid Hearthstone path entered, wrote path file!";
                    const string caption = "Info";
                    MessageBox.Show(message, caption,
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Asterisk);

                    setStatus("Ready");
                }
                else // If the folder didn't exist, the installation is broke
                {
                    // Inform the user, to check their installation
                    log("ERROR: The directory 'injector' does not exists!");
                    log("\tCheck your installation!");
                    setStatus("Error");
                }
            }
        }

        private bool loader_commandline(string argument)
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

        private void inject(object sender, EventArgs e)
        {
            setStatus("Injecting...");
            bool b = loader_commandline("inject");
            if(b)
            {
                setStatus("Error during injection");
            }
            else
            {
                setStatus("Injected");
            }
        }

        private void regen_inject(object sender, EventArgs e)
        {
            setStatus("Regenerating injection file");
            bool b = loader_commandline("regen_inject");
            if(b)
            {
                setStatus("Error during regen_inject");
            }
            else
            {
                setStatus("Injection Regenerated");
            }
        }

        private void reload_scripts(object sender, EventArgs e)
        {
            setStatus("Reloading scripts");
            bool b = loader_commandline("reload");
            if(b)
            {
                setStatus("Error during reload");
            }
            else
            {
                setStatus("Scripts reloaded");
            }
        }
        
        private void startbot(object sender, EventArgs e)
        {
            setStatus("Starting bot");
            bool b = false;

            string selectedItem = (string) cmb.SelectedItem;
            if(selectedItem == null)
            {
                b = loader_commandline("startbot");
            }
            else
            {
                b = loader_commandline("--set_mode=" + selectedItem + " startbot");
            }

            if(b)
            {
                setStatus("Error during startbot");
            }
            else
            {
                setStatus("Bot started");
            }
        }
        
        private void stopbot(object sender, EventArgs e)
        {
            setStatus("Stopping bot");
            bool b = loader_commandline("stopbot");
            if(b)
            {
                setStatus("Error during stopbot");
            }
            else
            {
                setStatus("Bot stopped");
            }
        }
        
        private void HearthstonePathSetter(Panel parent)
        {
            FlowLayoutPanel panel = new FlowLayoutPanel();
            panel.AutoSize = true;
            panel.Parent = parent;

            Label label1 = new Label();
            label1.Text = "Hearthstone Path: ";
            label1.AutoSize = true;
            label1.Parent = panel;

            TextBox tb = new TextBox();
            tb.Parent = panel;
            tb.Text = getHSPath();
            tb.Size = new Size(Width-label1.Width, label1.Height);
            tb.Multiline = false;
            tb.TextChanged += new EventHandler(onKeyUpEvent);
        }

        private ComboBox cmb = null;

        public GUI()
        {
            Text = "LoaderGUI";
            Size = new Size(WIDTH, HEIGHT);
            //AutoSize = true;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            TopMost = true;

            Menu = setupMainMenu();

            FlowLayoutPanel panel = new FlowLayoutPanel();
            panel.AutoSize = true;
            panel.Parent = this;
            panel.FlowDirection = FlowDirection.TopDown;

            HearthstonePathSetter(panel);

            FlowLayoutPanel buttons1 = new FlowLayoutPanel();
            buttons1.AutoSize = true;
            buttons1.Parent = panel;

            Button btn1 = new Button();
            btn1.Text = "Inject";
            btn1.Parent = buttons1;
            btn1.Anchor = AnchorStyles.Right;
            btn1.Click += new EventHandler(inject);
            
            Button btn2 = new Button();
            btn2.Text = "Regen";
            btn2.Parent = buttons1;
            btn2.Anchor = AnchorStyles.Right;
            btn2.Click += new EventHandler(regen_inject);
            
            Button btn3 = new Button();
            btn3.Text = "Reload";
            btn3.Parent = buttons1;
            btn3.Anchor = AnchorStyles.Right;
            btn3.Click += new EventHandler(reload_scripts);

            FlowLayoutPanel buttons2 = new FlowLayoutPanel();
            buttons2.AutoSize = true;
            buttons2.Parent = panel;
            
            Button btn4 = new Button();
            btn4.Text = "Start bot";
            btn4.Parent = buttons2;
            btn4.Anchor = AnchorStyles.Right;
            btn4.Click += new EventHandler(startbot);
            
            Button btn5 = new Button();
            btn5.Text = "Stop bot";
            btn5.Parent = buttons2;
            btn5.Anchor = AnchorStyles.Right;
            btn5.Click += new EventHandler(stopbot);
            
            cmb = new ComboBox();
            cmb.DropDownStyle = ComboBoxStyle.DropDownList;
            cmb.Parent = buttons2;
            cmb.Anchor = AnchorStyles.Right;

            string[] game_modes =
                new string[]{"TOURNAMENT_RANKED", "TOURNAMENT_UNRANKED",
                             "PRATICE_NORMAL",    "PRATICE_EXPERT"};

            cmb.Items.AddRange(game_modes);
            cmb.Name = "GameMode";
            cmb.Size = new Size(btn5.Width*2, btn5.Height);

            // Create status bar
            sb = new StatusBar();
            sb.Parent = this;
            sb.Text = "Ready";

            if(check_hs_path(getHSPath()) == false)
            {
                sb.Text = "HS PATH NOT SET";
            }

            CenterToScreen();
        }

        static public void Main()
        {
            Application.Run(new GUI());
        }
    }
}
