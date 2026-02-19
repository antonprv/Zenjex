// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using UnityEngine;

namespace DevConsole.Frontend.Services
{
  public class PlatformService
  {
    public bool IsMobile { get; }

    private readonly float _safeAreaLeftOffset;
    private readonly float _safeAreaRightOffset;
    private readonly bool _useSafeAreas;

    public PlatformService(
      float safeAreaLeftOffset,
      float safeAreaRightOffset,
      bool useSafeAreas)
    {
      _safeAreaLeftOffset = safeAreaLeftOffset;
      _safeAreaRightOffset = safeAreaRightOffset;
      _useSafeAreas = useSafeAreas;

      IsMobile = DetectMobilePlatform();
    }

    public Rect GetConsoleRootRect()
    {
      if (!IsMobile || !_useSafeAreas)
        return new Rect(0, 0, Screen.width, Screen.height);

      return CalculateSafeAreaRect();
    }

    private bool DetectMobilePlatform()
    {
      return Application.platform == RuntimePlatform.Android
        || Application.platform == RuntimePlatform.IPhonePlayer;
    }

    private Rect CalculateSafeAreaRect()
    {
      Rect safeArea = Screen.safeArea;
      float guiY = Screen.height - safeArea.yMax;

      return new Rect(
        safeArea.x + _safeAreaLeftOffset,
        guiY,
        safeArea.width - _safeAreaRightOffset,
        safeArea.height);
    }
  }
}
