// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using DevConsole.Backend.Infrastructure.Services.DevConsole.Interfaces;
using Code.Infrastructure.Services.DevConsole.Types;
using System.Collections.Generic;

namespace DevConsole.Backend.Infrastructure.Services.DevConsole.Commands
{
  public class HelpCommand : IConsoleCommand
  {
    private readonly IDevConsole _console;
    private readonly Dictionary<string, IConsoleCommand> _allCommands;

    public string CommandName => "help";
    public string Description => "Show all available commands. Usage: help";

    public HelpCommand(IDevConsole console, Dictionary<string, IConsoleCommand> allCommands)
    {
      _console = console;
      _allCommands = allCommands;
    }

    public void Execute(string[] args)
    {
      _console.AddMessage("Available commands:", ConsoleMessageType.Log);
      foreach (var command in _allCommands.Values)
      {
        _console.AddMessage($"  {command.CommandName} - {command.Description}", ConsoleMessageType.Log);
      }
    }
  }
}
