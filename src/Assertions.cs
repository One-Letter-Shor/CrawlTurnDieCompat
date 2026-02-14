using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using BepInEx.Logging;
using UnityEngine.Assertions;

namespace OneLetterShor.CrawlTurnDieCompat;

internal static class Assertions
{
    [DoesNotReturn]
    private static void FailAssertion(string? message)
    {
        string additionalText = message is null
            ? "."
            : $": {message}";
        
        throw new AssertionException($"Assertion Failure{additionalText}", null);
    }
    
    private static void FailSoftAssertion(
        string? message,
        LogLevel logLevel,
        bool canGenerateStackTrace,
        int skipFrames,
        string assertingCallerMemberName,
        int assertingCallerLineNumber,
        string assertingCallerFilePath)
    {
        string additionalText = "";
        
        additionalText += message is null
            ? "."
            : $": {message}";
        
        additionalText += canGenerateStackTrace
            ? new StackTrace(skipFrames: skipFrames, fNeedFileInfo: true).ToString()
            : "";
        
        Logger.Log(
            logLevel,
            $"Assertion Failure{additionalText}",
            assertingCallerMemberName,
            assertingCallerLineNumber,
            assertingCallerFilePath
        );
    }

    internal static void Assert(
        [DoesNotReturnIf(false)] bool condition,
        string? message = null)
    {
        if (condition) return;
        
        FailAssertion(message);
    }
    
    [Conditional("DEBUG")]
    internal static void DebugAssert(
        [DoesNotReturnIf(false)] bool condition,
        string? message = null)
    {
        if (condition) return;
        
        FailAssertion(message);
    }
    
    internal static void SoftAssert(
        bool condition,
        string? message = null,
        LogLevel logLevel = LogLevel.Error,
        bool canGenerateStackTrace = false,
        int skipFrames = 1,
        [CallerMemberName] string callerMemberName = "",
        [CallerLineNumber] int callerLineNumber = 0,
        [CallerFilePath] string callerFilePath = "")
    {
        if (condition) return;
        
        FailSoftAssertion(
            message,
            logLevel,
            canGenerateStackTrace,
            skipFrames + 1,
            callerMemberName,
            callerLineNumber,
            callerFilePath
        );
    }
    
    [Conditional("DEBUG")]
    internal static void DebugSoftAssert(
        bool condition,
        string? message = null,
        LogLevel logLevel = LogLevel.Error,
        bool canGenerateStackTrace = false,
        int skipFrames = 1,
        [CallerMemberName] string callerMemberName = "",
        [CallerLineNumber] int callerLineNumber = 0,
        [CallerFilePath] string callerFilePath = "")
    {
        if (condition) return;
        
        FailSoftAssertion(
            message,
            logLevel,
            canGenerateStackTrace,
            skipFrames + 1,
            callerMemberName,
            callerLineNumber,
            callerFilePath
        );
    }
    
    internal static void AssertIsType<T>(object? obj, out T t, string? message=null)
    {
        if (obj is T objT)
        {
            t = objT;
            return;
        }
        
        string additionalText = "";
        
        additionalText += message is null
            ? "."
            : $": {message}";
        
        FailAssertion($"{obj} must be {nameof(T)}{additionalText}");
        
        throw new UnreachableException();
    }
}

file sealed class UnreachableException : Exception;