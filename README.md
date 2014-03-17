This project is divided into 4 parts.

* The bot plugin (Bot.dll), found at projects/Bot
* The injector module (Injector.exe), found at projects/Injector
* The commandline tool (LoaderCommandline.exe), found at projects/LoaderCommandline
* The AI scripts, found at projects/Release/LuaScripts

The injector module, modifies the Blizzard dll file, Assembly-CSharp.dll, by injecting a reference to the Bot.dll file. This effectivly means that whenever Heartstone startup, it loads Bot.dll, which will be our hook into Heartstone.exe

The bot plugin, serves multiple purposes. 1. It allows us to send commands to Heartstone, from the commandline tool (via. a tcp server socket it opens). 2. It loads the lua enviroment, and fires off the ai lua script, also it triggers events in the ai scripts, whenever it makes sense to do so.

The commandline tool, is the interface to the entire system, it allows for easy inject into Heartstone, which involves invoking the injector module, copying the injected dll files, and any dependencies, that Bot.dll depends on. Also it's the tool for sending commands to Bot.dll, for instance starting the bot, stopping the bot, reloading the AI scripts, ect.

For a compresensive list of commands, please run the commandline tool, with --help.

The AI scripts are plain lua scripts, and an interface to interact with the game (implemented by hooks to C#).

Developer info:

* error CS0006: Metadata file `Assembly-CSharp.dll' could not be found;
    This means you have not 'Assembly-CSharp.dll' into projects/Bot/lib, from the Hearthstone folder
