using RainMeadow;

namespace OneLetterShor.CrawlTurnDieCompat;

internal static class RainMeadowHookCache
{
    internal static void __Initialize() { }
    
    
    internal static MethodInfo MatchmakingManager_HandleDisconnect = typeof(MatchmakingManager).GetMethod(nameof(MatchmakingManager.HandleDisconnect), BindingFlags.Instance | BindingFlags.Public)!;
    
    internal static MethodInfo SteamMatchmakingManager_LeaveLobby = typeof(SteamMatchmakingManager).GetMethod(nameof(MatchmakingManager.LeaveLobby), BindingFlags.Instance | BindingFlags.Public)!;
    
    internal static MethodInfo LANMatchmakingManager_LeaveLobby = typeof(LANMatchmakingManager).GetMethod(nameof(MatchmakingManager.LeaveLobby), BindingFlags.Instance | BindingFlags.Public)!;
    
    internal static MethodInfo[] MatchmakingManagerSubTypes_LeaveLobby =  [ SteamMatchmakingManager_LeaveLobby, LANMatchmakingManager_LeaveLobby ];
    
    
    internal static MethodInfo RainMeadow_PlayerHooks = typeof(RainMeadow.RainMeadow).GetMethod(nameof(RainMeadow.RainMeadow.PlayerHooks), BindingFlags.Instance | BindingFlags.Public)!;
    
    
    internal static MethodInfo OnlinePhysicalObject_Explode = typeof(OnlinePhysicalObject).GetMethod(nameof(OnlinePhysicalObject.Explode), BindingFlags.Instance | BindingFlags.Public)!;
}