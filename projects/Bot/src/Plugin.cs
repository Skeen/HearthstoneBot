using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Net;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace HearthstoneBot
{
	public class Plugin : MonoBehaviour
	{
        private static volatile string bot_path = null;
		private static volatile bool running = false;

        private static ServerSocket server_socket = null;
        private static AIBot ai_bot = null;

		public static void init()
		{
            // Get the SceneManager object
			GameObject sceneObject = SceneMgr.Get().gameObject;
            // Add ourselves to the Scene Manager
			sceneObject.AddComponent<Plugin>();
		}

        public Plugin()
        {
            // Setup a server socket
            server_socket = new ServerSocket();
            server_socket.start();

            // Announce that loading is completed
			Log.log("Bot loaded, accepting commands!");
        }

        public static void destroy()
        {
            // Announce that Hearthstone is closing
			Log.log("Hearthstone closing");
            // Shutdown the server socket
            server_socket.stop();
        }

		private void OnDestroy()
		{
            destroy();
		}

		public void Start()
		{
            // Announce that the Bot is activated
			Log.say("Bot activated");
		}

		public void Update()
		{
            // Try to handle pending events
            try
            {
                server_socket.handle_events();
            }
            catch(Exception e)
            {
                Log.error("Exception: Handling pending events");
                Log.error(e.ToString());
            }

            // If bot is ready, run it
            if (ai_bot != null)
            {
                // Run a tick of the AI
                ai_bot.tick();
            }
		}

        // Set whether the bot is running
        public static void setRunning(bool run)
        {
            running = run;

            if(running)
            {
                Log.say("Bot started");
            }
            else
            {
                Log.say("Bot stopped");
            }
        }

        public static bool isRunning()
        {
            return running;
        }

        public static void loadAIBot()
        {
            // Setup the AI Bot
            ai_bot = new AIBot();
        }

        // Reload AI scripts
        public static void ReloadScripts()
        {
            Log.say("Bot scripts reloaded");
            ai_bot.ReloadScripts();
        }

        public static void setBotPath(string str)
        {
            Log.log("Bot path set to : " + str);
            bot_path = str;
        }

        public static string getBothPath()
        {
            if(bot_path == null)
            {
                Log.error("Bot Path not set, when requested!");
            }
            return bot_path;
        }
    }
}
