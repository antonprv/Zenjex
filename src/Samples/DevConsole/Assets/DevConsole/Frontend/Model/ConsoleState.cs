// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

namespace DevConsole.Frontend.Model
{
  public class ConsoleState
  {
    public bool IsVisible { get; set; }
    public string InputText { get; set; } = string.Empty;

    public void Show()
    {
      IsVisible = true;
      InputText = string.Empty;
    }

    public void Hide() => IsVisible = false;

    public void Toggle()
    {
      if (IsVisible)
        Hide();
      else
        Show();
    }

    public void ClearInput() => InputText = string.Empty;
  }
}
