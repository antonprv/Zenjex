// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using DevConsole.Backend.Generated;
using DevConsole.Backend.Infrastructure.Services.Input.Interfaces;

using System;

namespace DevConsole.Backend.Infrastructure.Services.Input
{
  public class InputService : IInputService, IDisposable
  {
    public bool GameInputEnabled { get; set; }

    private InputActions _inputActions;

    public InputService()
    {
      GameInputEnabled = true;

      _inputActions = new InputActions();
      _inputActions.Enable();
      _inputActions.DevConsole.Enable();
    }

    // Console input methods
    public bool IsConsoleButtonPressed() => !GetTouchConsoleButtonUp() ?
      GetPCConsoleButtonUp() : GetTouchConsoleButtonUp();

    public bool IsConsoleSubmitPressed() => !GetTouchConsoleSubmitButtonUp() ?
      GetPCConsoleSubmitButtonUp() : GetTouchConsoleSubmitButtonUp();

    public float GetConsoleHistoryAxis() => GetHistoryAxis();


    #region private methods

    private bool GetPCConsoleButtonUp() =>
      _inputActions.DevConsole.ToggleConsole.WasPressedThisFrame();
    private bool GetTouchConsoleButtonUp() =>
      SimpleInput.GetButtonUp(TouchButtonNames.ToggleConsoleButton);

    private bool GetPCConsoleSubmitButtonUp() =>
      _inputActions.DevConsole.ConsoleSubmit.WasPressedThisFrame();
    private bool GetTouchConsoleSubmitButtonUp() =>
      SimpleInput.GetButtonUp(TouchButtonNames.ConsoleSubmitButton);

    private float GetHistoryAxis() =>
      _inputActions.DevConsole.ConsoleHistoryAxis.ReadValue<float>();

    public void Dispose()
    {
      _inputActions.DevConsole.Disable();
      _inputActions.Disable();
      _inputActions.Dispose();
    }

  }
  #endregion
}
