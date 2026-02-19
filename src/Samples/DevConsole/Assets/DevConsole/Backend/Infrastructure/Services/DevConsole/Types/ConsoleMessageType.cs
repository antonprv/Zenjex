// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

namespace Code.Infrastructure.Services.DevConsole.Types
{
  public enum ConsoleMessageType
  {
    Log,
    Warning,
    Error,
    Command,
    Success,
    UnityLog,  // For captured Unity Debug.Log messages
    All        // Special filter value to show all messages
  }
}
