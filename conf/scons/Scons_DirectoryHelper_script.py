import os
import sys
import string

Import(['env'])

def GetDirectoryName(env, sources):
    current_dir = os.path.basename(env.GetDirectoryPathAbsolute([]))
    return current_dir

env.AddMethod(GetDirectoryName, "GetDirectoryName")

def GetDirectoryPathAbsolute(env, sources):
    current_dir = Dir('.').srcnode().abspath
    return current_dir

env.AddMethod(GetDirectoryPathAbsolute, "GetDirectoryPathAbsolute")

def GetDirectoryPathRelative(env, sources):
    current_dir = Dir('.').srcnode().path
    return current_dir

env.AddMethod(GetDirectoryPathRelative, "GetDirectoryPathRelative")

root_dir = Dir('#').srcnode().abspath
def GetRootDirectoryPath(env, sources):
    return root_dir

env.AddMethod(GetRootDirectoryPath, "GetRootDirectoryPath")

def get_immediate_subdirectories(dir):
    return [name for name in os.listdir(dir)
            if os.path.isdir(os.path.join(dir, name))]

def GetSubDirectories(env, sources):
    subdirs = get_immediate_subdirectories(sources)
    return subdirs

env.AddMethod(GetSubDirectories, "GetSubDirectories")
