using System.Runtime.CompilerServices;
using Menu.Remix;

namespace OneLetterShor.CrawlTurnDieCompat.Utils;

public static class ModGetter
{
    internal static ModManager.Mod GetRequiredMod(string guid, bool isSilent=false, [CallerMemberName] string callerMemberName="", [CallerLineNumber] int callerLineNumber=0, [CallerFilePath] string callerFilePath="")
    {
        ModManager.Mod? mod = ModManager.ActiveMods.Find(mod => mod.id == guid);
        
        if (mod is null) 
            throw Logger.Exception(new NullModException(guid), callerMemberName:callerMemberName, callerLineNumber:callerLineNumber, callerFilePath:callerFilePath); 
        else if (!isSilent) 
            Logger.Info($"Mod with id \"{guid}\" was found.", callerMemberName, callerLineNumber, callerFilePath);

        return mod;
    }
    
    internal static ModManager.Mod? GetOptionalMod(string guid, bool isSilent=false, [CallerMemberName] string callerMemberName="", [CallerLineNumber] int callerLineNumber=0, [CallerFilePath] string callerFilePath="")
    {
        ModManager.Mod? mod = ModManager.ActiveMods.Find(mod => mod.id == guid);

        if (!isSilent)
            if (mod is null)
                Logger.Info($"Mod with id \"{guid}\" was not found.", callerMemberName, callerLineNumber, callerFilePath);
            else 
                Logger.Info($"Mod with id \"{guid}\" was found.", callerMemberName, callerLineNumber, callerFilePath);

        return mod;
    }
}