using MonoMod.RuntimeDetour;
using RainMeadow;

namespace OneLetterShor.CrawlTurnDieCompat;

public static class RainMeadowPlayerNotifier
{
    // ReSharper disable once CollectionNeverQueried.Global
    public static List<OnlinePlayer> OnlinePlayersWithCtd { get; private set; } = [];
    public static List<OnlinePlayer> OnlinePlayersWithCtdc { get; private set; } = [];

    internal static void ApplyHooksAndEvents()
    {
        MatchmakingManager.OnLobbyJoined += OnJoinedLobby; // TODO: check if OnPlayerListReceived_t works
        
        _ = new Hook(
            RainMeadowHookCache.MatchmakingManager_HandleDisconnect,
            On_RainMeadow_MatchmakingManager_HandleDisconnect
        );
        
        foreach (MethodInfo method in RainMeadowHookCache.MatchmakingManagerSubTypes_LeaveLobby)
        {
            _ = new Hook(
                method,
                On_RainMeadow_MatchmakingManager_LeaveLobby
            );
        }
    }
    
    private static void OnJoinedLobby(bool __, string ___)
    {
        if (Compat.IsCrawlTurnDieEnabled)
            OnlinePlayersWithCtd.Add(OnlineManager.mePlayer);
        OnlinePlayersWithCtdc.Add(OnlineManager.mePlayer);
        
        foreach (OnlinePlayer oPlayer in OnlineManager.players.Where(oPlayer => !oPlayer.isMe))
            oPlayer.InvokeRPC(NotifyOfCtdcSoftRpc, Compat.CrawlTurnDieMod?.version!, true);
    }
    
    private static void On_RainMeadow_MatchmakingManager_HandleDisconnect(Action<MatchmakingManager, OnlinePlayer> orig, MatchmakingManager matchmakingManager, OnlinePlayer oPlayer)
    {
        OnlinePlayersWithCtd.Remove(oPlayer);
        OnlinePlayersWithCtdc.Remove(oPlayer);
        
        orig(matchmakingManager, oPlayer);
    }
    
    private static void On_RainMeadow_MatchmakingManager_LeaveLobby(Action<MatchmakingManager> orig, MatchmakingManager matchmakingManager)
    {
        OnlinePlayersWithCtd = [];
        OnlinePlayersWithCtdc = [];
        
        orig(matchmakingManager);
    }
    
    [SoftRPCMethod]
    private static void NotifyOfCtdcSoftRpc(RPCEvent rpcEvent, string? ctdVersion, bool shouldRespond)
    {
        bool hasCtd = ctdVersion is not null;
        
        if (shouldRespond)
            rpcEvent.from.InvokeRPC(NotifyOfCtdcSoftRpc, Compat.CrawlTurnDieMod?.version!, false);
        
        
        if (!OnlinePlayersWithCtd.Contains(rpcEvent.from) && hasCtd) OnlinePlayersWithCtd.Add(rpcEvent.from);
        if (!OnlinePlayersWithCtdc.Contains(rpcEvent.from)) OnlinePlayersWithCtdc.Add(rpcEvent.from);
    }
}