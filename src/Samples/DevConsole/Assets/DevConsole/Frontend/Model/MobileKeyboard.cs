// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using UnityEngine;

namespace DevConsole.Frontend.Model
{
  public class MobileKeyboard
  {
    private TouchScreenKeyboard _keyboard;
    private readonly bool _isEnabled;

    public MobileKeyboard(bool isEnabled) => _isEnabled = isEnabled;

    public void Open(string initialText)
    {
      if (!_isEnabled)
        return;

      _keyboard = TouchScreenKeyboard.Open(
        initialText,
        TouchScreenKeyboardType.Default,
        autocorrection: false,
        multiline: false,
        secure: false,
        alert: false);
    }

    public void Close()
    {
      if (!_isEnabled || !IsActive())
        return;

      _keyboard.active = false;
      _keyboard = null;
    }

    public string GetCurrentText(string fallbackText)
    {
      if (!_isEnabled || _keyboard == null)
        return fallbackText;

      try
      {
        if (_keyboard.status == TouchScreenKeyboard.Status.Visible)
          return _keyboard.text;

        if (_keyboard.status == TouchScreenKeyboard.Status.Canceled)
          _keyboard = null;
      }
      catch
      {
        _keyboard = null;
      }

      return fallbackText;
    }

    public bool IsActive()
    {
      if (_keyboard == null)
        return false;

      try
      {
        return TouchScreenKeyboard.visible;
      }
      catch
      {
        _keyboard = null;
        return false;
      }
    }

    private bool IsVisible()
    {
      if (_keyboard == null)
        return false;

      try
      {
        return _keyboard.status == TouchScreenKeyboard.Status.Visible;
      }
      catch
      {
        return false;
      }
    }

    private bool IsCanceled()
    {
      if (_keyboard == null)
        return false;

      try
      {
        return _keyboard.status == TouchScreenKeyboard.Status.Canceled;
      }
      catch
      {
        return false;
      }
    }
  }
}
