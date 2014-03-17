from SCons.Script.SConscript import SConsEnvironment

#-- ------------------
#-- jDataClass holds data for the helper functions
class jDataClass:
    mHelpText = {}
    mHelpTextHead = []
    mHelpTextTail = []
SConsEnvironment.jData = jDataClass()
#-- ------------------
#-- wraps Alias to put the alias name in the help text
def jAlias(self, aliasname, tgt, helptext=None):
   thealias = self.Alias(aliasname, tgt)
   if helptext is None:
       if not self.jData.mHelpText.has_key(aliasname):
          self.jData.mHelpText[aliasname] = '???'
   else:
       self.jData.mHelpText[aliasname] = helptext
   return thealias
SConsEnvironment.jAlias = jAlias
#-- ------------------
#-- adds a line of text to the help heading
def jHelpHead(self, msg):
   self.jData.mHelpTextHead.append(msg);
SConsEnvironment.jHelpHead = jHelpHead
#-- ------------------
#-- adds a line of text to the help footing
def jHelpFoot(self, msg):
   self.jData.mHelpTextTail.append(msg);
SConsEnvironment.jHelpFoot = jHelpFoot
#-- ------------------
#-- generates the help
def jGenHelp(self):
   tmp = []
   tmp.extend(self.jData.mHelpTextHead)
   keys = self.jData.mHelpText.keys()
   keys.sort()
   maxlen = 0
   for a in keys:
      if len(a) > maxlen: maxlen = len(a)
   for a in keys:
      s = ' %-*s : %s' % (maxlen, a, self.jData.mHelpText[a])
      tmp.append(s)
   tmp.extend(self.jData.mHelpTextTail)
   self.Help("\n".join(tmp))
SConsEnvironment.jGenHelp = jGenHelp
#-- ------------------
#-- adds upper/lower case of alias names for a target
def jSetAliases(self, projname, tgt):
   self.jAlias(projname, tgt)
   self.jAlias(projname.lower(), tgt, "compile " + projname)
SConsEnvironment.jSetAliases = jSetAliases

#SConsEnvironment.dev = Dev()

