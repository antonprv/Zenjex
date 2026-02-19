// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using DevConsole.Backend.Infrastructure.Services.DevConsole.Interfaces;
using Code.Infrastructure.Services.DevConsole.Types;

namespace DevConsole.Backend.Infrastructure.Services.DevConsole.Commands.Logs
{
  public class ToggleUnityLogsCommand : IConsoleCommand
  {
    private readonly IDevConsole _console;
    private bool _isEnabled = true;

    public string CommandName => "toggle_unity_logs";
    public string Description => "Toggle Unity Debug.Log capture. Usage: toggle_unity_logs";

    public ToggleUnityLogsCommand(IDevConsole console)
    {
      _console = console;
    }

    public void Execute(string[] args)
    {
      _isEnabled = !_isEnabled;
      _console.SetCaptureUnityLogs(_isEnabled);
      _console.AddMessage(
        $"Unity log capture {(_isEnabled ? "enabled" : "disabled")}",
        ConsoleMessageType.Success
      );
    }
  }
}
