using RainMeadow;

namespace OneLetterShor.CrawlTurnDieCompat;

public static class Compat
{
    public const string 
        CrawlTurnDieGuid = "OneLetterShor.CrawlTurnDie",
        RainMeadowGuid = "henpemaz_rainmeadow";
    public static Compats ActiveCompats { get; internal set; } = Compats.None;
    public static GameMode ActiveGameMode { get; internal set; } = GameMode.None;
    public static bool IsCrawlTurnDieEnabled => ActiveCompats.HasFlag(Compats.CrawlTurnDie);
    public static bool IsRainMeadowEnabled => ActiveCompats.HasFlag(Compats.RainMeadow);
    internal static ModManager.Mod? CrawlTurnDieMod;
    internal static ModManager.Mod? RainMeadowMod;
    public static event Action<RainWorldGame, ProcessManager?>? ActiveGameModeChanged;
    
    internal static void ApplyHooksAndEvents()
    {
        On.RainWorldGame.ctor += On_RainWorldGame_ctor; 
        On.RainWorldGame.ShutDownProcess += On_RainWorldGame_ShutDownProcess;
    }
    
    public static void CheckMods()
    {
        if ((CrawlTurnDieMod = Utils.ModGetter.GetOptionalMod(CrawlTurnDieGuid)) is not null) ActiveCompats |= Compats.CrawlTurnDie;
        if ((RainMeadowMod = Utils.ModGetter.GetOptionalMod(RainMeadowGuid)) is not null) ActiveCompats |= Compats.RainMeadow;
        
        if (Logger.CanLogCompatsChange) Logger.Info($"Using {ActiveCompats} as the {nameof(Compats)}.");
    }
    
    private static void OnActiveGameModeChanged(RainWorldGame game, ProcessManager? processManager)
    {
        if (Logger.CanLogGameModeChange) Logger.Info($"Using {ActiveGameMode} as the {nameof(GameMode)}.");
        ActiveGameModeChanged?.Invoke(game, processManager);
    }
    
    private static void On_RainWorldGame_ctor(On.RainWorldGame.orig_ctor orig, RainWorldGame game, ProcessManager processManager)
    {
        orig(game, processManager);
        
        if (IsRainMeadowEnabled)                                           ActiveGameMode = RainMeadowGetActiveGameMode();
        else if (IsCrawlTurnDieEnabled || ActiveCompats == Compats.None)   ActiveGameMode = VanillaGetActiveGameMode();
        else throw Logger.Exception(new InvalidOperationException($"Unrecognized or invalid {nameof(Compats)} value. value: {ActiveCompats}"));
        
        OnActiveGameModeChanged(game, processManager);
        
        return;
        
        GameMode VanillaGetActiveGameMode()
        {
            return game.session switch
            {
                StoryGameSession => GameMode.VanillaStory,
                ArenaGameSession => GameMode.VanillaArena,
                _ => throw Logger.Exception(new InvalidOperationException($"Unrecognized or invalid {nameof(Compats)} value. value: {ActiveCompats}"))
            };
        }
        
        GameMode RainMeadowGetActiveGameMode()
        {
            if (OnlineManager.lobby is null) return VanillaGetActiveGameMode();
        
            return OnlineManager.lobby.gameMode switch
            {
                StoryGameMode => GameMode.RainMeadowStory,
                ArenaOnlineGameMode => GameMode.RainMeadowArena,
                MeadowGameMode => GameMode.RainMeadowMeadow,
                _ => throw Logger.Exception(new InvalidOperationException($"Unrecognized or invalid {nameof(OnlineGameMode)} type. type: {OnlineManager.lobby.gameMode.GetType()}"))
            };
        }
    }
    
    private static void On_RainWorldGame_ShutDownProcess(On.RainWorldGame.orig_ShutDownProcess orig, RainWorldGame game)
    {
        ActiveGameMode = GameMode.None;
        OnActiveGameModeChanged(game, null);
        
        orig(game);
    }
}