import os
import shutil

Import(['env'])

from SCons.Builder import Builder

env['CSC'] = "gmcs"
env['CSCWIN'] = False
env['CSCFLAGS'] = ""
env['CSC_LIBS'] = []
env['CSC_SEARCH'] = []
env['CSC_RUN'] = []

def MOVE_LIBS(target_string, env):

    assembly_search_paths = env['CSC_SEARCH']

    # move libraries
    for assembly_search_path in assembly_search_paths:
        runlibs = env['CSC_RUN']
        for runlib in runlibs:
            lib_path = assembly_search_path + '/' + runlib
            lib_target = os.path.dirname(target_string) + '/' + runlib
            if os.path.isfile(lib_path):
                shutil.copy2(lib_path, lib_target)

def CLI_COMPILE_WIN(target, sources, env, library):
    compiler = env['CSC'] + " "
    compiler_flags = env['CSCFLAGS'] + " "
    if library:
        compiler_flags += " /t:library "
    else:
        compiler_flags += " /t:exe "

    compiler_references = " "
    assembly_references = env['CSC_LIBS']
    for assembly_reference in assembly_references:
        compiler_references += " /reference:"
        compiler_references += assembly_reference.replace('/', '\\\\')

    compiler_references += " /reference:System.Data.dll "    
    compiler_references += " /reference:System.dll "        
    compiler_references += " /reference:System.Core.dll "    
    compiler_references += " "

    search_path = " "
    assembly_search_paths = env['CSC_SEARCH']
    for assembly_search_path in assembly_search_paths:
        if os.path.exists(assembly_search_path):
            search_path += " /lib:"
            search_path += assembly_search_path.replace('/', '\\\\')
    search_path += " "

    target_string = str(target[0])
    output_target = " /out:" + target_string.replace('/', '\\\\')

    source_files = " "
    for source in sources:
        source_files += str(source).replace('/', '\\\\') + " "
    source_files += " "

    cmd = compiler + compiler_flags + search_path + compiler_references + output_target + source_files
    print(cmd)

    os.system(cmd)

    MOVE_LIBS(target_string, env);

def CLI_COMPILE_MONO(target, sources, env, library):
    compiler = env['CSC'] + " "
    compiler_flags = env['CSCFLAGS'] + " "
    if library:
        compiler_flags += " -t:library "

    compiler_references = " "
    assembly_references = env['CSC_LIBS']
    for assembly_reference in assembly_references:
        compiler_references += " /reference:"
        compiler_references += assembly_reference
    compiler_references += " "

    search_path = " "
    assembly_search_paths = env['CSC_SEARCH']
    for assembly_search_path in assembly_search_paths:
        search_path += " /lib:"
        search_path += assembly_search_path
    search_path += " "

    target_string = str(target[0])
    output_target = " -out:" + target_string

    source_files = " "
    for source in sources:
        source_files += str(source) + " "
    source_files += " "

    cmd = compiler + compiler_flags + search_path + compiler_references + output_target + source_files
    print(cmd)

    os.system(cmd)

    MOVE_LIBS(target_string, env);

def CLI_COMPILE(target, sources, env, library):
    if(len(target) > 1):
        print("ERROR: CLI_COMPILE takes a single string target argument")
        return 1

    is_win = env['CSCWIN']
    if is_win:
        CLI_COMPILE_WIN(target, sources, env, library)
    else:
        CLI_COMPILE_MONO(target, sources, env, library)

def CLIProgram(target, source, env):
    CLI_COMPILE(target, source, env, False)

def CLILibrary(target, source, env):
    CLI_COMPILE(target, source, env, True)

env['BUILDERS']['CLIProgram'] = Builder(
    action=CLIProgram,
    suffix='.exe',
    src_suffix='.cs')

env['BUILDERS']['CLILibrary'] = Builder(
    action=CLILibrary,
    suffix='.dll',
    src_suffix='.cs')

