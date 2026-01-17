using RainMeadow;

namespace OneLetterShor.CrawlTurnDieCompat;

internal static class RainMeadowHookCache
{
    internal static void __Initialize() { }
    
    
    internal static MethodBase MatchmakingManager_HandleDisconnect = typeof(MatchmakingManager).GetMethod(nameof(MatchmakingManager.HandleDisconnect), BindingFlags.Instance | BindingFlags.Public)!;
    
    internal static MethodBase SteamMatchmakingManager_LeaveLobby = typeof(SteamMatchmakingManager).GetMethod(nameof(MatchmakingManager.LeaveLobby), BindingFlags.Instance | BindingFlags.Public)!;
    
    internal static MethodBase LANMatchmakingManager_LeaveLobby = typeof(LANMatchmakingManager).GetMethod(nameof(MatchmakingManager.LeaveLobby), BindingFlags.Instance | BindingFlags.Public)!;
    
    internal static MethodBase[] MatchmakingManagerSubTypes_LeaveLobby =  [ SteamMatchmakingManager_LeaveLobby, LANMatchmakingManager_LeaveLobby ];
    
    
    internal static MethodBase RainMeadow_PlayerHooks = typeof(RainMeadow.RainMeadow).GetMethod(nameof(RainMeadow.RainMeadow.PlayerHooks), BindingFlags.Instance | BindingFlags.Public)!;
    
    
    internal static MethodBase OnlinePhysicalObject_Explode = typeof(OnlinePhysicalObject).GetMethod(nameof(OnlinePhysicalObject.Explode), BindingFlags.Instance | BindingFlags.Public)!;
}