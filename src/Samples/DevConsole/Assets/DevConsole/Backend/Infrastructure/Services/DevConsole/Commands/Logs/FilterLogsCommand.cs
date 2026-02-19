// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using DevConsole.Backend.Infrastructure.Services.DevConsole.Interfaces;
using Code.Infrastructure.Services.DevConsole.Types;

namespace DevConsole.Backend.Infrastructure.Services.DevConsole.Commands.Logs
{
  public class FilterLogsCommand : IConsoleCommand
  {
    private readonly IDevConsole _console;

    public string CommandName => "filter";
    public string Description => "Filter logs by type. Usage: filter <log|warning|error|unity|success|all>";

    public FilterLogsCommand(IDevConsole console)
    {
      _console = console;
    }

    public void Execute(string[] args)
    {
      if (args.Length < 1)
      {
        _console.AddMessage(Description, ConsoleMessageType.Warning);
        _console.AddMessage($"Current filter: {_console.GetLogFilter()}", ConsoleMessageType.Log);
        return;
      }

      string filterType = args[0].ToLower();
      ConsoleMessageType newFilter;

      switch (filterType)
      {
        case "log":
          newFilter = ConsoleMessageType.Log;
          break;
        case "warning":
          newFilter = ConsoleMessageType.Warning;
          break;
        case "error":
          newFilter = ConsoleMessageType.Error;
          break;
        case "unity":
          newFilter = ConsoleMessageType.UnityLog;
          break;
        case "success":
          newFilter = ConsoleMessageType.Success;
          break;
        case "all":
          newFilter = ConsoleMessageType.All;
          break;
        default:
          _console.AddMessage($"Unknown filter type: {filterType}", ConsoleMessageType.Error);
          _console.AddMessage(Description, ConsoleMessageType.Warning);
          return;
      }

      _console.SetLogFilter(newFilter);
    }
  }
}
