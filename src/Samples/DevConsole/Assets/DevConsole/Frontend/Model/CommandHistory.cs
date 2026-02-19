// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using System.Collections.Generic;

namespace DevConsole.Frontend.Model
{
  public class CommandHistory
  {
    public IReadOnlyList<string> Commands => _commands.AsReadOnly();

    private readonly int _maxCapacity;
    private readonly List<string> _commands = new();

    private int _navigationIndex = -1;
    private string _cachedInput = string.Empty;

    public CommandHistory(int maxCapacity) => _maxCapacity = maxCapacity;

    public void Add(string command)
    {
      _commands.Add(command);

      if (_commands.Count > _maxCapacity)
        _commands.RemoveAt(0);

      ResetNavigation();
    }

    public string NavigateUp(string currentInput)
    {
      if (_commands.Count == 0)
        return currentInput;

      if (IsAtBeginning())
      {
        _cachedInput = currentInput;
        _navigationIndex = _commands.Count - 1;
      }
      else if (CanNavigateUp())
      {
        _navigationIndex--;
      }

      return _commands[_navigationIndex];
    }

    public string NavigateDown()
    {
      if (IsAtBeginning())
        return _cachedInput;

      _navigationIndex++;

      if (IsAtEnd())
      {
        ResetNavigation();
        return _cachedInput;
      }

      return _commands[_navigationIndex];
    }

    private void ResetNavigation() => _navigationIndex = -1;

    private bool IsAtBeginning() => _navigationIndex == -1;
    private bool CanNavigateUp() => _navigationIndex > 0;
    private bool IsAtEnd() => _navigationIndex >= _commands.Count;
  }
}
