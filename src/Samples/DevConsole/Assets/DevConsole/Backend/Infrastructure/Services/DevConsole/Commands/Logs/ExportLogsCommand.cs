// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using DevConsole.Backend.Infrastructure.Services.DevConsole.Interfaces;
using Code.Infrastructure.Services.DevConsole.Types;

using UnityEngine;

namespace DevConsole.Backend.Infrastructure.Services.DevConsole.Commands.Logs
{
  public class ExportLogsCommand : IConsoleCommand
  {
    private readonly IDevConsole _console;

    public string CommandName => "export_logs";
    public string Description => "Export logs to file. Usage: export_logs";

    public ExportLogsCommand(IDevConsole console)
    {
      _console = console;
    }

    public void Execute(string[] args)
    {
      string[] messages = _console.GetMessages();
      string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
      string path = $"{Application.persistentDataPath}/console_log_{timestamp}.txt";

      System.IO.File.WriteAllLines(path, messages);

      _console.AddMessage($"Logs exported to: {path}", ConsoleMessageType.Success);
      Debug.Log($"Console logs exported to: {path}");
    }
  }
}

