// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using DevConsole.Backend.Infrastructure.Services.DevConsole.Interfaces;
using Code.Infrastructure.Services.DevConsole.Types;

using UnityEngine;

namespace DevConsole.Backend.Infrastructure.Services.DevConsole.Commands.Performance
{
  public class SetFPSCommand : IConsoleCommand
  {
    private readonly IDevConsole _console;

    public string CommandName => "set_fps";
    public string Description => "Set target FPS. Usage: set_fps <value>";

    public SetFPSCommand(IDevConsole console)
    {
      _console = console;
    }

    public void Execute(string[] args)
    {
      if (args.Length < 1)
      {
        _console.AddMessage(Description, ConsoleMessageType.Warning);
        return;
      }

      if (int.TryParse(args[0], out int targetFPS))
      {
        Application.targetFrameRate = targetFPS;
        _console.AddMessage($"Target FPS set to {targetFPS}", ConsoleMessageType.Success);
      }
      else
        _console.AddMessage($"Invalid FPS value: {args[0]}", ConsoleMessageType.Error);
    }
  }
}
