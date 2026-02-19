// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using DevConsole.Backend.Infrastructure.Services.Input.Interfaces;
using UnityEngine;

namespace DevConsole.Frontend.Services
{
  public class InputService
  {
    private readonly IInputService _inputService;
    private readonly bool _isMobile;
    private readonly float _navigationDeadzone;
    private readonly float _navigationCooldown;

    private float _lastNavigationTime;
    private bool _navigationAxisReleased = true;

    public InputService(
      IInputService inputService,
      bool isMobile,
      float navigationDeadzone,
      float navigationCooldown)
    {
      _inputService = inputService;
      _isMobile = isMobile;
      _navigationDeadzone = navigationDeadzone;
      _navigationCooldown = navigationCooldown;
    }

    public bool IsTogglePressed() =>
      _inputService.IsConsoleButtonPressed();

    public bool IsSubmitPressed() =>
      _inputService.IsConsoleSubmitPressed();

    public int GetHistoryNavigationDirection()
    {
      if (_isMobile)
        return 0;

      float axis = _inputService.GetConsoleHistoryAxis();

      if (IsAxisInDeadzone(axis))
      {
        _navigationAxisReleased = true;
        return 0;
      }

      if (!CanNavigate())
        return 0;

      ResetNavigationCooldown();

      return axis > 0 ? 1 : -1;
    }

    private bool IsAxisInDeadzone(float axis) =>
      Mathf.Abs(axis) < _navigationDeadzone;

    private bool CanNavigate() =>
      _navigationAxisReleased
      && Time.time - _lastNavigationTime >= _navigationCooldown;

    private void ResetNavigationCooldown()
    {
      _navigationAxisReleased = false;
      _lastNavigationTime = Time.time;
    }
  }
}
