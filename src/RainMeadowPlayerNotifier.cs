using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        
        foreach (MethodBase methodBase in RainMeadowHookCache.MatchmakingManagerSubTypes_LeaveLobby)
        {
            _ = new Hook(
                methodBase,
                On_RainMeadow_MatchmakingManager_LeaveLobby
            );
        }
    }
    
    private static void OnJoinedLobby(bool _, string __)
    {
        OnlinePlayersWithCtd.Add(OnlineManager.mePlayer);
        OnlinePlayersWithCtdc.Add(OnlineManager.mePlayer);
        
        foreach (OnlinePlayer oPlayer in OnlineManager.players.Where(oPlayer => !oPlayer.isMe))
            oPlayer.InvokeRPC(NotifyOfCrawlTurnDieCompatSoftRpc, Compat.CrawlTurnDieMod?.version!, true);
    }
    
    private static void On_RainMeadow_MatchmakingManager_HandleDisconnect(Action<MatchmakingManager, OnlinePlayer> orig, MatchmakingManager matchmakingManager, OnlinePlayer oPlayer)
    {
        OnlinePlayersWithCtd.Remove(oPlayer);
        OnlinePlayersWithCtdc.Remove(oPlayer);
        
        orig(matchmakingManager, oPlayer);
    }
    
    private static void On_RainMeadow_MatchmakingManager_LeaveLobby(Action<MatchmakingManager> orig, MatchmakingManager matchmakingManager)
    {
        if (OnlineManager.players is not null)
        {
            OnlinePlayersWithCtd = [];
            OnlinePlayersWithCtdc = [];
        }
        
        orig(matchmakingManager);
    }
    
    [SoftRPCMethod]
    private static void NotifyOfCrawlTurnDieCompatSoftRpc(RPCEvent rpcEvent, string? ctdVersion, bool shouldRespond)
    {
        bool hasCtd = ctdVersion is not null;
        
        Logger.Info($"{rpcEvent.from} is using Crawl Turn Die version {ctdVersion ?? "null"}");

        if (shouldRespond)
            rpcEvent.from.InvokeRPC(NotifyOfCrawlTurnDieCompatSoftRpc, Compat.CrawlTurnDieMod?.version!, false);
        
        if (OnlinePlayersWithCtdc.Contains(rpcEvent.from))
        {
            Logger.Warning($"Received notify from {rpcEvent.from} ({ctdVersion}) but they've already been added to {nameof(OnlinePlayersWithCtdc)}.");
            return;
        }
        
        if (hasCtd) OnlinePlayersWithCtd.Add(rpcEvent.from);
        OnlinePlayersWithCtdc.Add(rpcEvent.from);
    }
}