using System.Diagnostics;
using System.Runtime.CompilerServices;
using LogLevel = BepInEx.Logging.LogLevel;

namespace OneLetterShor.CrawlTurnDieCompat;

public static class Logger
{
    public static LogLevel EnabledLogLevels = LogLevel.All;
    public static bool
        CanLogCompatsChange = false,
        CanLogGameModeChange = false;
    private static string TrimPath(string path)
    {
        string[] elements = path.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        int startIndex = Array.IndexOf(elements, "src") + 1;
        return String.Join(Path.AltDirectorySeparatorChar.ToString(), elements.ToArray().Skip(startIndex).ToArray());
    }
    private static int _nextMarkIndex => field++;
    public static void Mark(LogLevel logLevel=LogLevel.Debug, [CallerMemberName] string callerMemberName="", [CallerLineNumber] int callerLineNumber=0, [CallerFilePath] string callerFilePath="") =>
        Log(logLevel, $"============================== MARK {_nextMarkIndex} ==============================", callerMemberName, callerLineNumber, callerFilePath);
    public static Exception Exception(Exception exception, string prefix="", string suffix="", LogLevel logLevel=LogLevel.Fatal, [CallerMemberName] string callerMemberName="", [CallerLineNumber] int callerLineNumber=0, [CallerFilePath] string callerFilePath="")
    {
        if (exception.StackTrace is null)
            StackTrace(logLevel, $"{prefix} {exception.Message} {suffix}", skipFrames:2, callerMemberName, callerLineNumber, callerFilePath);
        else
            Log(logLevel, $"{prefix} {exception.Message} {suffix}\n{exception.StackTrace}", callerMemberName, callerLineNumber, callerFilePath);

        return exception;
    }
    public static void StackTrace(LogLevel logLevel, string message, int skipFrames=1, [CallerMemberName] string callerMemberName="", [CallerLineNumber] int callerLineNumber=0, [CallerFilePath] string callerFilePath="")
    {
        StackTrace stackTrace = new StackTrace(skipFrames:skipFrames, fNeedFileInfo:true);
        Log(logLevel, $"{message}\n{stackTrace}", callerMemberName, callerLineNumber, callerFilePath);
    }
    public static void Fatal(object? data, [CallerMemberName] string callerMemberName="", [CallerLineNumber] int callerLineNumber=0, [CallerFilePath] string callerFilePath="") =>
        Log(LogLevel.Fatal, data, callerMemberName, callerLineNumber, callerFilePath);
    public static void Error(object? data, [CallerMemberName] string callerMemberName="", [CallerLineNumber] int callerLineNumber=0, [CallerFilePath] string callerFilePath="") =>
        Log(LogLevel.Error, data, callerMemberName, callerLineNumber, callerFilePath);
    public static void Warning(object? data, [CallerMemberName] string callerMemberName="", [CallerLineNumber] int callerLineNumber=0, [CallerFilePath] string callerFilePath="") =>
        Log(LogLevel.Warning, data, callerMemberName, callerLineNumber, callerFilePath);
    public static void Message(object? data, [CallerMemberName] string callerMemberName="", [CallerLineNumber] int callerLineNumber=0, [CallerFilePath] string callerFilePath="") =>
        Log(LogLevel.Message, data, callerMemberName, callerLineNumber, callerFilePath);
    public static void Info(object? data, [CallerMemberName] string callerMemberName="", [CallerLineNumber] int callerLineNumber=0, [CallerFilePath] string callerFilePath="") =>
        Log(LogLevel.Info, data, callerMemberName, callerLineNumber, callerFilePath);
    public static void Debug(object? data, [CallerMemberName] string callerMemberName="", [CallerLineNumber] int callerLineNumber=0, [CallerFilePath] string callerFilePath="") =>
        Log(LogLevel.Debug, data, callerMemberName, callerLineNumber, callerFilePath);
    public static void Log(LogLevel logLevel, object? data, [CallerMemberName] string callerMemberName="", [CallerLineNumber] int callerLineNumber=0, [CallerFilePath] string callerFilePath="")
    {
        if (!EnabledLogLevels.HasFlag(logLevel))
            return;
        
        string sourceInfo = $"{TrimPath(callerFilePath)}:{callerLineNumber}, {callerMemberName}()";
        
        Plugin.__Log(logLevel, $"{sourceInfo}: {data}");
    }
}