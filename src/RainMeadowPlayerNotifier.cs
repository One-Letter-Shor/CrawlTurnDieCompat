using MonoMod.RuntimeDetour;
using OneLetterShor.CrawlTurnDie;
using RainMeadow;

namespace OneLetterShor.CrawlTurnDieCompat;

public static class RainMeadowPlayerNotifier
{
    public static OnlinePlayer? CtdMarkOwner => OnlineManager.lobby?.owner;
    public static List<OnlinePlayer> CtdOPlayers { get; private set; } = [];
    public static List<OnlinePlayer> CtdcOPlayers { get; private set; } = [];
	
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
            CtdOPlayers.Add(OnlineManager.mePlayer);
        CtdcOPlayers.Add(OnlineManager.mePlayer);
        
        BroadcastNotify(OnlineManager.players, NotifyOfCtdcSoftRpc, [ Compat.CrawlTurnDieMod?.version!, true ]);
    }
    
    private static void On_RainMeadow_MatchmakingManager_HandleDisconnect(Action<MatchmakingManager, OnlinePlayer> orig, MatchmakingManager matchmakingManager, OnlinePlayer oPlayer)
    {
        CtdOPlayers.Remove(oPlayer);
        CtdcOPlayers.Remove(oPlayer);
        
        orig(matchmakingManager, oPlayer);
    }
    
    private static void On_RainMeadow_MatchmakingManager_LeaveLobby(Action<MatchmakingManager> orig, MatchmakingManager matchmakingManager)
    {
        CtdOPlayers = [];
        CtdcOPlayers = [];
        
        orig(matchmakingManager);
    }
    
    public static void BroadcastNotify(List<OnlinePlayer> targets, Delegate softRpc, object?[]? softRpcArgs=null)
    {
        if (softRpc != (Delegate)NotifyOfCtdcSoftRpc && targets.Any(target => !CtdcOPlayers.Contains(target))) Logger.Error($"At least one target being broadcasted to is not in {nameof(CtdcOPlayers)}. targets: {String.Join(", ", targets)} ctdc players: {String.Join(", ", CtdcOPlayers)}");
        
        foreach (OnlinePlayer oPlayer in targets.Where(oPlayer => !oPlayer.isMe))
        {
            if (softRpcArgs is null)
                oPlayer.InvokeRPC(softRpc);
            else
                oPlayer.InvokeRPC(softRpc, softRpcArgs!);
        }
    }
    
    [SoftRPCMethod]
    private static void NotifyOfCtdcSoftRpc(RPCEvent rpcEvent, string? ctdVersion, bool shouldRespond)
    {
        bool hasCtd = ctdVersion is not null;
        
        Logger.Info($"{rpcEvent.from.id.name} has {(hasCtd ? $"CTD {ctdVersion}" : "CTDC")}.");
        
        if (shouldRespond)
            rpcEvent.from.InvokeRPC(NotifyOfCtdcSoftRpc, Compat.CrawlTurnDieMod?.version!, false);
        
        if (!CtdOPlayers.Contains(rpcEvent.from) && hasCtd)   CtdOPlayers.Add(rpcEvent.from);
        if (!CtdcOPlayers.Contains(rpcEvent.from))            CtdcOPlayers.Add(rpcEvent.from);
    }
    
    [SoftRPCMethod]
    public static void NotifyOfMarkSoftRpc(RPCEvent rpcEvent, OnlinePlayer oPlayer, byte index, byte playerActionsValue)
    {
        var playerActions = (PlayerActions)playerActionsValue;
        if (CtdMarkOwner is null ||
            PlayerActionHandler.Instance is not RainMeadowPlayerActionHandler playerActionHandler) 
            return;

        var player = RainMeadowPlayerHandle.GetOrCreateInstance(oPlayer, index);
        if (player.Index == 0 || CrawlTurnDie.Logger.CanLogAllPlayerHandles) Logger.Info($"Received Mark {player.Name} from {rpcEvent.from.id.name}");

        playerActionHandler.MarkLocally(player, playerActions);
        if (CtdMarkOwner.isMe)
        {
            if (player.Index == 0 || CrawlTurnDie.Logger.CanLogAllPlayerHandles) Logger.Info("Broadcasting Mark.");
            BroadcastNotify(CtdOPlayers, NotifyOfMarkSoftRpc, [ player.OPlayer, player.Index, playerActionsValue ]);
        }
    }
    
    [SoftRPCMethod]
    public static void NotifyOfUnmarkSoftRpc(RPCEvent rpcEvent, OnlinePlayer oPlayer, byte index, byte playerActionsValue)
    {
        var playerActions = (PlayerActions)playerActionsValue;
        if (CtdMarkOwner is null ||
            PlayerActionHandler.Instance is not RainMeadowPlayerActionHandler playerActionHandler) 
            return;

        var player = RainMeadowPlayerHandle.GetOrCreateInstance(oPlayer, index);

        if (player.Index == 0 || CrawlTurnDie.Logger.CanLogAllPlayerHandles) Logger.Info($"Received Unmark {player.Name} from {rpcEvent.from.id.name}");

        playerActionHandler.UnmarkLocally(player, playerActions);
        if (CtdMarkOwner.isMe)
        {
            if (player.Index == 0 || CrawlTurnDie.Logger.CanLogAllPlayerHandles) Logger.Info("Broadcasting Unmark.");
            BroadcastNotify(CtdOPlayers, NotifyOfUnmarkSoftRpc, [ oPlayer, index, playerActionsValue ]);
        }
    }
    
    [SoftRPCMethod]
    public static void NotifyOfClearMarksSoftRpc(RPCEvent rpcEvent)
    {
        if (CtdMarkOwner is null ||
            PlayerActionHandler.Instance is not RainMeadowPlayerActionHandler playerActionHandler) 
            return;
        
        Logger.Info($"Received ClearMarks from {rpcEvent.from.id.name}");
        
        playerActionHandler.ClearMarksLocally();
        if (CtdMarkOwner.isMe)
        {
            Logger.Info("Broadcasting ClearMarks.");
            BroadcastNotify(CtdOPlayers, NotifyOfClearMarksSoftRpc, []);
        }
    }
}