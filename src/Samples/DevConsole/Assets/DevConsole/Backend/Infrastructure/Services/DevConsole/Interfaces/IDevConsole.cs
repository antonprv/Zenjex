// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using Code.Infrastructure.Services.DevConsole.Types;

namespace DevConsole.Backend.Infrastructure.Services.DevConsole.Interfaces
{
  public interface IDevConsole
  {
    public void Initialize();
    public string ConsoleMarker { get; }
    public bool IsEnabled { get; }
    public void Toggle();
    public void ExecuteCommand(string command);
    public void RegisterCommand(IConsoleCommand command);
    public void AddMessage(string message, ConsoleMessageType type = ConsoleMessageType.Log);
    public string[] GetMessages();
    public void ClearMessages();
    public void SetCaptureUnityLogs(bool capture);
    public void SetLogFilter(ConsoleMessageType filter);
    public ConsoleMessageType GetLogFilter();
  }
}
