// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using DevConsole.Backend.Infrastructure.Services.DevConsole.Interfaces;

namespace DevConsole.Backend.Infrastructure.Services.DevConsole.Commands
{
  public class ClearCommand : IConsoleCommand
  {
    private readonly IDevConsole _console;

    public string CommandName => "clear";
    public string Description => "Clear console history. Usage: clear";

    public ClearCommand(IDevConsole console) => _console = console;

    public void Execute(string[] args) => _console.ClearMessages();
  }
}
