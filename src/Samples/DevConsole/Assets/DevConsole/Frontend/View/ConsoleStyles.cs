// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using UnityEngine;

namespace DevConsole.Frontend.View
{
  public class ConsoleStyles
  {
    public GUIStyle BoxStyle { get; private set; }
    public GUIStyle InputStyle { get; private set; }
    public GUIStyle OutputStyle { get; private set; }
    public GUIStyle LabelStyle { get; private set; }
    public GUIStyle ButtonStyle { get; private set; }

    private readonly int _outputFontSize;
    private readonly int _inputFontSize;
    private bool _initialized;

    public ConsoleStyles(int outputFontSize, int inputFontSize)
    {
      _outputFontSize = outputFontSize;
      _inputFontSize = inputFontSize;
    }

    public void EnsureInitialized()
    {
      if (_initialized)
        return;

      InitializeStyles();
      _initialized = true;
    }

    private void InitializeStyles()
    {
      BoxStyle = CreateBoxStyle();
      InputStyle = CreateInputStyle();
      OutputStyle = CreateOutputStyle();
      LabelStyle = CreateLabelStyle();
      ButtonStyle = CreateButtonStyle();
    }

    private GUIStyle CreateBoxStyle() =>
      new GUIStyle(GUI.skin.box)
      {
        normal = { background = CreateSolidTexture(new Color(0, 0, 0, 0.85f)) }
      };

    private GUIStyle CreateInputStyle() =>
      new GUIStyle(GUI.skin.textField)
      {
        fontSize = _inputFontSize,
        normal = { textColor = Color.white },
        focused = { textColor = Color.white }
      };

    private GUIStyle CreateOutputStyle() =>
      new GUIStyle(GUI.skin.label)
      {
        fontSize = _outputFontSize,
        normal = { textColor = Color.white },
        wordWrap = true,
        richText = true
      };

    private GUIStyle CreateLabelStyle() =>
      new GUIStyle(GUI.skin.label)
      {
        fontSize = 14,
        normal = { textColor = Color.white },
        wordWrap = true
      };

    private GUIStyle CreateButtonStyle() =>
      new GUIStyle(GUI.skin.button)
      {
        fontSize = 32,
        fontStyle = FontStyle.Bold,
        normal = CreateButtonStateStyle(new Color(0.2f, 0.2f, 0.2f, 0.8f), Color.white),
        hover = CreateButtonStateStyle(new Color(0.3f, 0.3f, 0.3f, 0.9f), Color.cyan),
        active = CreateButtonStateStyle(new Color(0.4f, 0.4f, 0.4f, 1.0f), Color.yellow)
      };

    private GUIStyleState CreateButtonStateStyle(Color backgroundColor, Color textColor) =>
      new GUIStyleState
      {
        background = CreateSolidTexture(backgroundColor),
        textColor = textColor
      };

    private Texture2D CreateSolidTexture(Color color)
    {
      const int size = 2;
      Color[] pixels = new Color[size * size];

      for (int i = 0; i < pixels.Length; i++)
        pixels[i] = color;

      Texture2D texture = new Texture2D(size, size);
      texture.SetPixels(pixels);
      texture.Apply();

      return texture;
    }
  }
}
