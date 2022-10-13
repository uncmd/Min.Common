using Microsoft.Extensions.Logging;
using Min.Common.Logging;
using System.Runtime.ExceptionServices;

namespace System;

public static class MinExceptionExtensions
{
    public static Exception ReThrow(this Exception exception)
    {
        ExceptionDispatchInfo.Capture(exception).Throw();

        return exception;
    }

    public static LogLevel GetLogLevel(this Exception exception, LogLevel defaultLevel = LogLevel.Error)
    {
        return (exception as IHasLogLevel)?.LogLevel ?? defaultLevel;
    }
}
