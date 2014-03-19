using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.IO;
using System.Linq;

namespace HearthstoneBot
{
	public class Program
	{
        // Find the MethodDefinition of the Method to hook in
        private static MethodDefinition getHookInMethod()
        {
            // Find the function to hook in, from 'Bot.dll'
            string bot_dll_file = "Bot.dll";
            if (File.Exists(bot_dll_file) == false)
            {
                Console.WriteLine("ERROR: " + bot_dll_file + " not found!");
                Environment.Exit(-1);
            }

            // Load the assembly
            AssemblyDefinition bot_file_def = AssemblyDefinition.ReadAssembly(bot_dll_file);

            // Find the main class
            string injection_type_name = "Main";
            TypeDefinition bot_main_type_def = bot_file_def.MainModule.Types.Single((TypeDefinition t) => t.Name == injection_type_name);

            // Find the method Start
            string injection_method_name = "Start";
            MethodDefinition bot_start_method_def = bot_main_type_def.Methods.Single((MethodDefinition t) => t.Name == injection_method_name);

            // Return the hook'in method
            return bot_start_method_def;
        }

        private static AssemblyDefinition getCSharpDefintion()
        {
            // Find hook spot in 'Assembly-CSharp.original.dll'
            string assembly_csharp_original = "Assembly-CSharp.original.dll";
            if (File.Exists(assembly_csharp_original) == false)
            {
                Console.WriteLine("ERROR: " + assembly_csharp_original + " not found!");
                Environment.Exit(-1);
            }
            AssemblyDefinition assembly_csharp_original_def = AssemblyDefinition.ReadAssembly(assembly_csharp_original);

            return assembly_csharp_original_def;
        }

        private static ILProcessor getInstructionProcessor(AssemblyDefinition assembly_csharp_original_def) 
        {
            // In the class SceneMgr
            string application_type_name = "SceneMgr";
            TypeDefinition scenemgr_type_def = assembly_csharp_original_def.MainModule.Types.Single((TypeDefinition t) => t.Name == application_type_name);
            // The method Start
            string application_method_name = "Start";
            MethodDefinition scenemgr_start_method_def = scenemgr_type_def.Methods.Single((MethodDefinition t) => t.Name == application_method_name);

            // Find the method IL processor
            ILProcessor processor = scenemgr_start_method_def.Body.GetILProcessor();
            // Return it
            return processor;
        }

		public static void Main(string[] args)
		{
			try
			{
                // Get the method we want to call, on load up
                MethodDefinition bot_start_method_def = getHookInMethod();
                // Load the CSharp Definition
                AssemblyDefinition assembly_csharp_original_def = getCSharpDefintion();
                // Get the instruction processor
                ILProcessor processor = getInstructionProcessor(assembly_csharp_original_def);
                // Get the instruction we want to inject before
                Instruction first_instruction = processor.Body.Instructions[0];
                // Create a call instruction, to the Bot.dll load up method;
				Instruction instruction = processor.Create(OpCodes.Call, assembly_csharp_original_def.MainModule.Import(bot_start_method_def.Resolve()));
                // Insert the generate instruction, before the rest of the code
				processor.InsertBefore(first_instruction, instruction);
                // Write the result to a new dll file
				string assembly_csharp = "Assembly-CSharp.dll";
				assembly_csharp_original_def.Write(assembly_csharp);
                // Let the user know
                Console.WriteLine("Success!");
                // Exit gracefully
				Environment.Exit(0);
			}
			catch(Exception e)
			{
                Console.WriteLine("Exception: When generating Injection file");
                Console.WriteLine(e.ToString());

				File.WriteAllText("../logs/injector.log", e.ToString());
				Environment.Exit(1);
			}
		}
	}
}
