using System.Runtime.CompilerServices;
using BepInEx;
using LogLevel = BepInEx.Logging.LogLevel;

namespace OneLetterShor.CrawlTurnDieCompat;

public static class Logger
{
    public static LogLevel EnabledLogLevels = LogLevel.All;
    public static bool
        CanLogApiTypeChange = false,
        CanLogGuiTypeChange = false,
        CanLogCompatsChange = false,
        CanLogGameModeChange = false;
    private static int __nextMarkIndex => ++field;
    
    private static string TrimPath(string path)
    {
        string[] elements = path.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        int startIndex = Array.IndexOf(elements, "src") + 1;
        return String.Join(Path.AltDirectorySeparatorChar.ToString(), elements.ToArray().Skip(startIndex).ToArray());
    }
    
    public static void Mark(
        LogLevel logLevel=LogLevel.Debug,
        [CallerMemberName] string callerMemberName="",
        [CallerLineNumber] int callerLineNumber=0,
        [CallerFilePath] string callerFilePath="")
    {
        Log(logLevel, $"============================== MARK {__nextMarkIndex} ==============================", callerMemberName, callerLineNumber, callerFilePath);
    }
    
    public static void Exception(
        Exception exception,
        LogLevel logLevel=LogLevel.Fatal,
        int skipFrames=1,
        [CallerMemberName] string callerMemberName="",
        [CallerLineNumber] int callerLineNumber=0,
        [CallerFilePath] string callerFilePath="")
    {
        Assert(!exception.StackTrace.IsNullOrWhiteSpace(), "Exception must be caught before attempting to log it.");
        
        Log(logLevel, $"{exception.Message}\n{exception.StackTrace}", callerMemberName, callerLineNumber, callerFilePath);
    }
    
    public static void Fatal(
        object? data,
        [CallerMemberName] string callerMemberName="",
        [CallerLineNumber] int callerLineNumber=0,
        [CallerFilePath] string callerFilePath="")
    {
        Log(LogLevel.Fatal, data, callerMemberName, callerLineNumber, callerFilePath);
    }
    
    public static void Error(
        object? data,
        [CallerMemberName] string callerMemberName="",
        [CallerLineNumber] int callerLineNumber=0,
        [CallerFilePath] string callerFilePath="")
    {
        Log(LogLevel.Error, data, callerMemberName, callerLineNumber, callerFilePath);
    }
    
    public static void Warning(
        object? data,
        [CallerMemberName] string callerMemberName="",
        [CallerLineNumber] int callerLineNumber=0,
        [CallerFilePath] string callerFilePath="")
    {
        Log(LogLevel.Warning, data, callerMemberName, callerLineNumber, callerFilePath);
    }
    
    public static void Message(
        object? data,
        [CallerMemberName] string callerMemberName="",
        [CallerLineNumber] int callerLineNumber=0,
        [CallerFilePath] string callerFilePath="")
    {
        Log(LogLevel.Message, data, callerMemberName, callerLineNumber, callerFilePath);
    }
    
    public static void Info(
        object? data,
        [CallerMemberName] string callerMemberName="",
        [CallerLineNumber] int callerLineNumber=0,
        [CallerFilePath] string callerFilePath="")
    {
        Log(LogLevel.Info, data, callerMemberName, callerLineNumber, callerFilePath);
    }
    
    public static void Debug(
        object? data,
        [CallerMemberName] string callerMemberName="",
        [CallerLineNumber] int callerLineNumber=0,
        [CallerFilePath] string callerFilePath="")
    {
        Log(LogLevel.Debug, data, callerMemberName, callerLineNumber, callerFilePath);
    }
    
    public static void Log(
        LogLevel logLevel,
        object? data,
        [CallerMemberName] string callerMemberName="",
        [CallerLineNumber] int callerLineNumber=0,
        [CallerFilePath] string callerFilePath="")
    {
        if (!EnabledLogLevels.HasFlag(logLevel))
            return;
        
        string sourceInfo = $"{TrimPath(callerFilePath)}:{callerLineNumber}, {callerMemberName}()";
        
        Plugin.__Log(logLevel, $"{sourceInfo}: {data}");
    }
}