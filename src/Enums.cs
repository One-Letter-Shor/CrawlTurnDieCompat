namespace OneLetterShor.CrawlTurnDieCompat;

[Flags]
public enum Compats
{
    None = 0,
    CrawlTurnDie = 1 << 0,
    RainMeadow = 1 << 1,
    All = CrawlTurnDie | RainMeadow
}

public enum GameMode
{
    None,
    VanillaStory,
    VanillaArena,
    RainMeadowMeadow,
    RainMeadowStory,
    RainMeadowArena
}