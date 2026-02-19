// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using DevConsole.Backend.Gameplay;
using DevConsole.Backend.Infrastructure.Services.DevConsole.Interfaces;
using Code.Infrastructure.Services.DevConsole.Types;

namespace DevConsole.Backend.Infrastructure.Services.DevConsole.Commands.Performance
{
  public class ToggleFPSCounterCommand : IConsoleCommand
  {
    private readonly FramerateManager _framerateManager;
    private readonly IDevConsole _console;

    public string CommandName => "stat_fps";
    public string Description => "Toggle FPS counter visibility. Usage: stat_fps";

    public ToggleFPSCounterCommand(IDevConsole console, FramerateManager framerateManager)
    {
      _console = console;
      _framerateManager = framerateManager;
    }

    public void Execute(string[] args)
    {
      if (_framerateManager == null)
      {
        _console.AddMessage("FramerateManager not found!", ConsoleMessageType.Error);
        return;
      }

      _framerateManager.showFPS = !_framerateManager.showFPS;
      _console.AddMessage($"FPS counter {(_framerateManager.showFPS ? "enabled" : "disabled")}", ConsoleMessageType.Success);
    }
  }
}
