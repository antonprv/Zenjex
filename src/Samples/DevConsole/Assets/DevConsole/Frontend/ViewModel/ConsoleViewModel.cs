// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using DevConsole.Backend.Infrastructure.Services.DevConsole.Interfaces;
using DevConsole.Frontend.Model;
using DevConsole.Frontend.Services;

using System.Collections.Generic;

namespace DevConsole.Frontend.ViewModel
{
  public class ConsoleViewModel
  {
    private readonly IDevConsole _console;
    private readonly ConsoleState _state;
    private readonly CommandHistory _history;
    private readonly MobileKeyboard _keyboard;
    private readonly InputService _inputService;
    private readonly PlatformService _platform;

    public ConsoleViewModel(
      IDevConsole console,
      ConsoleState state,
      CommandHistory history,
      MobileKeyboard keyboard,
      InputService inputService,
      PlatformService platform)
    {
      _console = console;
      _state = state;
      _history = history;
      _keyboard = keyboard;
      _inputService = inputService;
      _platform = platform;
    }

    public bool IsVisible => _state.IsVisible;

    public string InputText
    {
      get => _state.InputText;
      set => _state.InputText = value;
    }

    public IReadOnlyList<string> Messages => _console.GetMessages();

    public bool IsTogglePressed() => _inputService.IsTogglePressed();

    public void ToggleConsole()
    {
      _console.Toggle();
      _state.IsVisible = _console.IsEnabled;

      if (_state.IsVisible)
        OnConsoleOpened();
      else
        OnConsoleClosed();
    }

    public void HandleInput()
    {
      HandleHistoryNavigation();
      HandleSubmit();
      HandleMobileKeyboard();
    }

    public void SubmitCommand()
    {
      if (string.IsNullOrWhiteSpace(_state.InputText))
        return;

      ExecuteCommand(_state.InputText);
      _state.ClearInput();

      ReopenKeyboardIfNeeded();
    }

    public float GetViewHeight() =>
      _platform.IsMobile
        ? UnityEngine.Screen.height * 0.4f - 120
        : UnityEngine.Screen.height * 0.4f - 100;

    private void OnConsoleOpened()
    {
      _state.ClearInput();
      _keyboard.Open(_state.InputText);
    }

    private void OnConsoleClosed()
    {
      _keyboard.Close();
    }

    private void HandleHistoryNavigation()
    {
      int direction = _inputService.GetHistoryNavigationDirection();

      if (direction == 0)
        return;

      _state.InputText = direction > 0
        ? _history.NavigateUp(_state.InputText)
        : _history.NavigateDown();
    }

    private void HandleSubmit()
    {
      if (_inputService.IsSubmitPressed())
        SubmitCommand();
    }

    private void HandleMobileKeyboard()
    {
      _state.InputText = _keyboard.GetCurrentText(_state.InputText);
    }

    private void ExecuteCommand(string command)
    {
      _console.ExecuteCommand(command);
      _history.Add(command);
    }

    private void ReopenKeyboardIfNeeded()
    {
      if (_platform.IsMobile && _state.IsVisible)
        _keyboard.Open(_state.InputText);
    }
  }
}
