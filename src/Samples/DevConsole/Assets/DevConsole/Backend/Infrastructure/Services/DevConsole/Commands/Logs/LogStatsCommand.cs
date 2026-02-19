// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using DevConsole.Backend.Infrastructure.Services.DevConsole.Interfaces;
using Code.Infrastructure.Services.DevConsole.Types;

using UnityEngine;

namespace DevConsole.Backend.Infrastructure.Services.DevConsole.Commands.Logs
{
  public class LogStatsCommand : IConsoleCommand
  {
    private readonly IDevConsole _console;
    private int _logCount = 0;
    private int _warningCount = 0;
    private int _errorCount = 0;

    public string CommandName => "log_stats";
    public string Description => "Show log statistics. Usage: log_stats";

    public LogStatsCommand(IDevConsole console)
    {
      _console = console;
      Application.logMessageReceived += CountLogs;
    }

    private void CountLogs(string log, string trace, LogType type)
    {
      switch (type)
      {
        case LogType.Log:
          _logCount++;
          break;
        case LogType.Warning:
          _warningCount++;
          break;
        case LogType.Error:
        case LogType.Exception:
          _errorCount++;
          break;
      }
    }

    public void Execute(string[] args)
    {
      _console.AddMessage("=== Log Statistics ===", ConsoleMessageType.Log);
      _console.AddMessage($"Logs: {_logCount}", ConsoleMessageType.Log);
      _console.AddMessage($"Warnings: {_warningCount}", ConsoleMessageType.Warning);
      _console.AddMessage($"Errors: {_errorCount}", ConsoleMessageType.Error);
    }
  }

}
