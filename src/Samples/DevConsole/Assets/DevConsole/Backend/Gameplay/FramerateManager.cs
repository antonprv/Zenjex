// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using DevConsole.Backend.Infrastructure.Services.StaticDataService.Interfaces;
using DevConsole.Backend.Infrastructure.Services.StaticDataService.Interfaces.Subservice;
using UnityEngine;
using Zenjex.Extensions.Attribute;
using Zenjex.Extensions.Injector;
using DevConsole.Backend.Infrastructure.Services.Time;


// In this example, we're injecting trhough the ZenjexBehaviour
// Explicit injection in Awake().
namespace DevConsole.Backend.Gameplay
{
  public class FramerateManager : ZenjexBehaviour
  {
    public bool showFPS = true;

    private float _deltaTime = 0.0f;

    [Zenjex] private ITimeService _timeService;
    [Zenjex] private IStaticDataService _staticData;

    private IBuildConfigSubservice _build;
    private bool _isInitialized;

    protected override void OnAwake()
    {
      _build = _staticData.BuildConfig;
      _isInitialized = true;

      Debug.Log($"{nameof(FramerateManager)} initialized!");
    }

    void Start() => Application.targetFrameRate = 120;

    void Update()
    {
      if (!_isInitialized) return;

      if (_build.IsDevelopment())
      {
        _deltaTime += (_timeService.UnscaledDeltaTime - _deltaTime) * 0.1f;
      }
    }

    void OnGUI()
    {
      if (!_isInitialized) return;

      if (_build.IsDevelopment())
      {
        if (!showFPS) return;

        int w = Screen.width, h = Screen.height;

        GUIStyle style = new GUIStyle();

        Rect rect = new Rect(w - 120, 10, 100, 30); // Right upper corner

        style.alignment = TextAnchor.MiddleRight;
        style.fontSize = 40;
        style.normal.textColor = new Color(0.0f, 0.0f, 0.5f, 1.0f);

        float msec = _deltaTime * 1000.0f;
        float fps = 1.0f / _deltaTime;
        string text = $"{msec:F1} ms ({fps:F0}) FPS";

        GUI.Label(rect, text, style);
      }
    }
  }
}
