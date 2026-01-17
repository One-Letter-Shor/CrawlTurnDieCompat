using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using RainMeadow;
using UnityEngine;

namespace OneLetterShor.CrawlTurnDieCompat;

public static class RainMeadowPlayerActionRpcs
{
    public static event Action<OnlinePhysicalObject>? ReceivedKill;
    public static event Action<OnlinePhysicalObject, Vector2>? ReceivedExplode;
    public static event Action<OnlinePhysicalObject>? ReceivedRevive;
    private static RainWorldGame? Game;
    public static bool _shouldSkipRainMeadowPyroDeathIL = false; // Allows Crawl Turn Die to handle Explode the same as Kill (broadcast in room for example)
    public static Dictionary<OnlinePlayer, int> _explodesToIgnoreByOPlayer = new(); // Used to prevent a secondary explosion from a non-ctd using room owner 'reflecting' the rpc back. Should use opo, but online player is an easier implementation.\
    
    internal static void ApplyHooksAndEvents()
    {
        Compat.ActiveGameModeChanged += OnActiveGameModeChanged;
        
        _ = new Hook(
            RainMeadowHookCache.RainMeadow_PlayerHooks,
            On_RainMeadow_RainMeadow_PlayerHooks
        );
        
        _ = new Hook(
            RainMeadowHookCache.OnlinePhysicalObject_Explode,
            On_RainMeadow_OnlinePhysicalObject_Explode
        );
        
        _ = new Hook(
            RainMeadowHookCache.MatchmakingManager_HandleDisconnect,
            On_RainMeadow_MatchmakingManager_HandleDisconnect
        );
    }
    
    private static void OnActiveGameModeChanged(RainWorldGame game, ProcessManager? _)
    {
        if (Compat.ActiveGameMode == GameMode.None)
        {
            Game = null;
            _shouldSkipRainMeadowPyroDeathIL = false;
            _explodesToIgnoreByOPlayer = new Dictionary<OnlinePlayer, int>();
        }
        else
            Game = game;
    }
    
    public static void BroadcastInRoom(OnlinePhysicalObject target, Delegate softRpc, Delegate? rpc, object?[] softRpcArgs, object?[]? rpcArgs)
    {
        if (softRpc == (Delegate)ExplodeSoftRpc && !RainMeadowPlayerNotifier.OnlinePlayersWithCtdc.Contains(target.roomSession.owner))
        {
            if (rpc is null) throw Logger.Exception(new ArgumentNullException(nameof(rpc), "If sending explode rpcs, hard rpc related data must not be null."));
            if (rpcArgs is null) throw Logger.Exception(new ArgumentNullException(nameof(rpcArgs), "If sending explode rpcs, hard rpc related data must not be null."));
            
            _ = _explodesToIgnoreByOPlayer.TryGetValue(target.roomSession.owner, out int count);
            _explodesToIgnoreByOPlayer[target.roomSession.owner] = count + 1;

            target.owner.InvokeRPC(rpc, rpcArgs!);
            
            Logger.Debug($"Incrementing {target.roomSession.owner.id.name}. Value: {_explodesToIgnoreByOPlayer[target.roomSession.owner]}");
            return;
        }
        
        foreach (OnlinePlayer oPlayer in target.roomSession.participants.Where(oPlayer => !oPlayer.isMe)) {
            if (RainMeadowPlayerNotifier.OnlinePlayersWithCtdc.Contains(oPlayer))
                oPlayer.InvokeRPC(softRpc, softRpcArgs!);
            else
            {
                if (rpc is null) continue;
                oPlayer.InvokeRPC(rpc, rpcArgs!);
            }
        }
    }
    
    [SoftRPCMethod]
    public static void KillSoftRpc(RPCEvent __, OnlinePhysicalObject opo)
    {
        ReceivedKill?.Invoke(opo);
        
        if (!Compat.IsCrawlTurnDieEnabled)
            TryKill(opo);
    }
    
    [SoftRPCMethod]
    public static void ExplodeSoftRpc(RPCEvent __, OnlinePhysicalObject opo, Vector2 pos)
    {
        ReceivedExplode?.Invoke(opo, pos);

        if (!Compat.IsCrawlTurnDieEnabled)
            TryExplode(opo);
    }
    
    [SoftRPCMethod]
    public static void ReviveSoftRpc(RPCEvent __, OnlinePhysicalObject opo)
    {
        ReceivedRevive?.Invoke(opo);
        
        if (!Compat.IsCrawlTurnDieEnabled)
            TryRevive(opo);
    }
    
    private static void TryKill(OnlinePhysicalObject opo)
    {
        AbstractCreature? ac = opo.apo as AbstractCreature;
        Creature? creature = ac?.realizedCreature;
        
        if (
            Game is null ||
            ac?.Room is null ||
            opo.roomSession?.owner is null)
            return;
        
        if (creature is null)   ac.Die();
        else                    creature.Die();
    }
    
    private static void TryExplode(OnlinePhysicalObject opo)
    {
        AbstractCreature? ac = opo.apo as AbstractCreature;
        Player? player = ac?.realizedCreature as Player;
        
        if (
            Game is null ||
            ac?.Room is null ||
            opo.roomSession?.owner is null)
            return;
        
        if (
            !ModManager.MSC ||
            player?.room is null)
            return;
        
        _shouldSkipRainMeadowPyroDeathIL = true;
        player.PyroDeath();
        _shouldSkipRainMeadowPyroDeathIL = false;
    }
    
    private static void TryRevive(OnlinePhysicalObject opo)
    {
        AbstractCreature? ac = opo.apo as AbstractCreature;
        Player? player = ac?.realizedCreature as Player;
        
        if (
            Game is null ||
            ac?.Room is null ||
            opo.roomSession?.owner is null)
            return;
        
        if (
            player is null ||
            !opo.owner.isMe && !RainMeadowPlayerNotifier.OnlinePlayersWithCtdc.Contains(opo.owner))
            return;
        
        player.playerState.alive = true;
        player.playerState.permaDead = false;
        player.playerState.permanentDamageTracking = 0;
        player.playerState.isGhost = false;
        
        player.dead = false;
        player.stun = 1;
        player.exhausted = false;
        player.aerobicLevel = 0;
        player.airInLungs = 1f;
        player.killTag = null;
        player.killTagCounter = 0;

        if (ModManager.MSC)
        {
            player.Hypothermia = 0f;
            player.pyroJumpCooldown = 0f;
            player.pyroParryCooldown = 0f;
            player.pyroJumpCounter = 0;
        }

        Game.cameras[0].hud.textPrompt.gameOverMode = false;
    }
    
    private static void On_RainMeadow_RainMeadow_PlayerHooks(Action<RainMeadow.RainMeadow> orig, RainMeadow.RainMeadow rainMeadow)
    {
        orig(rainMeadow);
        IL.Player.PyroDeath += IL_Player_PyroDeath;
    }
    
    private static void IL_Player_PyroDeath(ILContext il) // TODO: A better solution is to il hook the delegate rain meadow emits, rather than skipping over the emitted il from rain meadow.
    {
        try
        {
            ILCursor cursor = new ILCursor(il);
            ILLabel skipRainMeadowsIL = il.DefineLabel();
            
            cursor.EmitDelegate(() => _shouldSkipRainMeadowPyroDeathIL);
            cursor.Emit(OpCodes.Brtrue, skipRainMeadowsIL);
            
            cursor.GotoNext(MoveType.After,
                c => c.MatchRet()
            );
            
            cursor.MarkLabel(skipRainMeadowsIL);
        }
        catch (Exception exception)
        {
            throw Logger.Exception(exception);
        }
    }
    
    private static void On_RainMeadow_OnlinePhysicalObject_Explode(Action<OnlinePhysicalObject, Vector2> orig, OnlinePhysicalObject opo, Vector2 pos)
    {
        if (
            opo.apo?.realizedObject is not Player ||
            opo.roomSession?.owner is null) 
        { orig(opo, pos); return; }
        
        if (_explodesToIgnoreByOPlayer.TryGetValue(opo.roomSession.owner, out int count) && count > 0)
        {
            _explodesToIgnoreByOPlayer[opo.roomSession.owner] = count - 1;
            Logger.Debug($"Decrementing {opo.roomSession.owner.id.name}. Value: {_explodesToIgnoreByOPlayer[opo.roomSession.owner]}");
            return;
        }
        
        orig(opo, pos);
    }
    
    private static void On_RainMeadow_MatchmakingManager_HandleDisconnect(Action<MatchmakingManager, OnlinePlayer> orig, MatchmakingManager matchmakingManager, OnlinePlayer oPlayer)
    {
        _explodesToIgnoreByOPlayer.Remove(oPlayer);
        
        orig(matchmakingManager, oPlayer);
    }
}