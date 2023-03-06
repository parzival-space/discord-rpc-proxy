using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace RPCProxy.Shared.Logging
{
  public class BasicLogger : ILogger
  {
    private readonly string name;
    private readonly int pid;
    private LogLevel level;

    // closed constructor
    private BasicLogger(string name, LogLevel level = LogLevel.Trace)
    {
      this.name = name;
      this.pid = Process.GetCurrentProcess().Id;
      this.level = level;
    }

    /// <summary>
    /// Creates a new basic color console logger.
    /// This logger does not handle any kind of i/o activity, 
    /// so you should use another logger for that.
    /// The main idea behind this was to test the app and have basic output capabilities.
    /// </summary>
    public static BasicLogger Create<T>(LogLevel level = LogLevel.Trace)
    {
      return new BasicLogger(typeof(T).Name, level);
    }

    /// <summary>
    /// Creates a new basic color console logger.
    /// This logger does not handle any kind of i/o activity, 
    /// so you should use another logger for that.
    /// The main idea behind this was to test the app and have basic output capabilities.
    /// </summary>
    /// <param name="name">Name of the class to log from.</param>
    public static BasicLogger Create(string name, LogLevel level = LogLevel.Trace)
    {
      return new BasicLogger(name, level);
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => default!;

    public bool IsEnabled(LogLevel logLevel) => logLevel > this.level;
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
      // if (logLevel > this.level == false) return;


      Console.ForegroundColor = this.LogLevelColorMap[logLevel][0];
      Console.BackgroundColor = this.LogLevelColorMap[logLevel][1];


      string threadName = "Thread-" + Thread.CurrentThread.ManagedThreadId;

      Console.WriteLine(
        "{0} {1} {2} --- [{3}] {4} : {5}",
        DateTime.Now.ToString("dd.MM.yyy HH:mm:ss.fff"),
        LogLevelNameMap[logLevel],
        pid,
        threadName.PadRight(10).Substring(0, 10),
        this.name.PadRight(10).Substring(0, 10),
        formatter(state, exception)
      );

      Console.ForegroundColor = this.originalColors[0];
      Console.BackgroundColor = this.originalColors[1];
    }


    private readonly ConsoleColor[] originalColors = new ConsoleColor[] { Console.ForegroundColor, Console.BackgroundColor };

    private readonly Dictionary<LogLevel, ConsoleColor[]> LogLevelColorMap = new()
    {
      [LogLevel.Trace] = new ConsoleColor[] { ConsoleColor.White, ConsoleColor.Cyan },
      [LogLevel.Debug] = new ConsoleColor[] { ConsoleColor.Gray, Console.BackgroundColor },
      [LogLevel.Information] = new ConsoleColor[] { ConsoleColor.Cyan, Console.BackgroundColor },
      [LogLevel.Warning] = new ConsoleColor[] { ConsoleColor.Yellow, Console.BackgroundColor },
      [LogLevel.Error] = new ConsoleColor[] { ConsoleColor.Red, Console.BackgroundColor },
      [LogLevel.Critical] = new ConsoleColor[] { ConsoleColor.Black, ConsoleColor.Red },
    };

    private readonly Dictionary<LogLevel, string> LogLevelNameMap = new()
    {
      [LogLevel.Trace] = "TRACE",
      [LogLevel.Debug] = "DEBUG",
      [LogLevel.Information] = "INFO ",
      [LogLevel.Warning] = "WARN ",
      [LogLevel.Error] = "ERROR",
      [LogLevel.Critical] = "CRIT ",
    };
  }
}