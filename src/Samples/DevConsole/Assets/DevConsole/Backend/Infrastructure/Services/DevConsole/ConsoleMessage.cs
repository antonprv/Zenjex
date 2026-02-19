// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using DevConsole.Backend.Infrastructure.Services.DevConsole.Interfaces;
using Code.Infrastructure.Services.DevConsole.Types;

namespace DevConsole.Backend.Infrastructure.Services.DevConsole
{
  public class ConsoleMessage
  {
    private IDevConsole _console;

    public string Message { get; }
    public ConsoleMessageType Type { get; }
    public string FormattedMessage { get; }

    public ConsoleMessage(string message, ConsoleMessageType type, IDevConsole devConsole)
    {
      _console = devConsole;

      Message = message;
      Type = type;
      FormattedMessage = FormatMessage(message, type);
    }

    private string FormatMessage(string message, ConsoleMessageType type)
    {
      string prefix = type switch
      {
        ConsoleMessageType.Warning => "<color=yellow>[WARNING]</color> ",
        ConsoleMessageType.Error => "<color=red>[ERROR]</color> ",
        ConsoleMessageType.Command => "<color=cyan>",
        ConsoleMessageType.Success => "<color=green>[OK]</color> ",
        ConsoleMessageType.UnityLog => "<color=white>[Unity]</color> ",
        _ => ""
      };

      string suffix = type == ConsoleMessageType.Command ? "</color>" : "";

      if (type == ConsoleMessageType.UnityLog)
        return prefix + message + suffix;
      else
        return _console.ConsoleMarker + prefix + message + suffix;
    }
  }
}
