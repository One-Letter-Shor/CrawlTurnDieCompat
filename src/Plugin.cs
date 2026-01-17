using System.Security.Permissions;
using BepInEx;
using OneLetterShor.CrawlTurnDieCompat;
using Logger_ = OneLetterShor.CrawlTurnDieCompat.Logger;
using LogLevel = BepInEx.Logging.LogLevel;
using SecurityAction = System.Security.Permissions.SecurityAction;

#pragma warning disable CS0618
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618
[assembly: AssemblyVersion(Plugin.Version)]

namespace OneLetterShor.CrawlTurnDieCompat;

// [BepInDependency(Compat.CrawlTurnDieGuid, BepInDependency.DependencyFlags.SoftDependency)] // This causes a deadlock. (ctd marks this mod as a hard dependency)
[BepInDependency(Compat.RainMeadowGuid, BepInDependency.DependencyFlags.SoftDependency)]
[BepInPlugin(Guid, Name, Version)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string
        Guid = "OneLetterShor.CrawlTurnDieCompat",
        Name = "Crawl Turn Die Compat",
        Version = "1.0.0";
    public static ModManager.Mod Mod { get; private set; } = null!;
    public static Plugin Instance { get; private set; } = null!;
    private static bool _isEnabled = false;
    public static bool IsFullyInitialized = false;
    
    private static void ApplyHooksAndEvents()
    {
        Compat.ApplyHooksAndEvents();
        
        if (Compat.IsRainMeadowEnabled)
            ApplyRainMeadowHooksAndEvents();
        
        return;
        
        void ApplyRainMeadowHooksAndEvents()
        {
            RainMeadowHookCache.__Initialize();
            RainMeadowPlayerNotifier.ApplyHooksAndEvents();
            RainMeadowPlayerActionRpcs.ApplyHooksAndEvents();
        }
    }
    
    private Plugin()
    {
        Instance = this;
    }
    
    private void OnEnable()
    {
        if (_isEnabled) throw Logger_.Exception(new InvalidOperationException("Cannot enable mod again."));
        _isEnabled = true;
        
        On.RainWorld.OnModsInit += On_RainWorld_OnModsInit;
    }
    
    private void On_RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld rainWorld)
    {
        if (IsFullyInitialized) { orig(rainWorld); return; }
        IsFullyInitialized = true;
        
        try
        {
            Mod = Utils.ModGetter.GetRequiredMod(Guid);
            Compat.CheckMods();
            ApplyHooksAndEvents();
        }
        catch(Exception exception)
        {
            Logger_.Exception(exception);
        }
        
        orig(rainWorld);
    }
    
    internal static void __Log(LogLevel level, object data) => Instance.Logger.Log(level, data);
}
