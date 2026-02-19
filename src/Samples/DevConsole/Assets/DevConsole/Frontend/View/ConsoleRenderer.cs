// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using DevConsole.Frontend.Services;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace DevConsole.Frontend.View
{
  public class ConsoleRenderer
  {
    private readonly ConsoleStyles _styles;
    private readonly PlatformService _platform;
    private readonly ScrollDragHandler _scrollDrag;

    private Vector2 _scrollPosition;
    private bool _pendingScrollToBottom;

    public ConsoleRenderer(
      ConsoleStyles styles,
      PlatformService platform)
    {
      _styles = styles;
      _platform = platform;
      _scrollDrag = new ScrollDragHandler(platform.IsMobile);
    }

    public void Render(
      IReadOnlyList<string> messages,
      ref string inputText,
      bool isVisible,
      Action onSubmit)
    {
      _styles.EnsureInitialized();

      Rect consoleRect = CalculateConsoleRect();

      GUILayout.BeginArea(consoleRect, _styles.BoxStyle);

      RenderHeader();
      RenderScrollableOutput(messages, consoleRect.height);
      GUILayout.Space(5);
      RenderInputField(ref inputText, isVisible, onSubmit);

      GUILayout.EndArea();

      if (_pendingScrollToBottom)
      {
        PerformScrollToBottom(messages, CalculateScrollHeight(consoleRect.height));
        _pendingScrollToBottom = false;
      }
    }

    public void ScrollToBottom(IReadOnlyList<string> messages, float viewHeight) =>
      _pendingScrollToBottom = true;

    private void PerformScrollToBottom(IReadOnlyList<string> messages, float viewHeight)
    {
      float contentHeight = CalculateContentHeight(messages);
      float maxScroll = Mathf.Max(0f, contentHeight - viewHeight);

      _scrollPosition = new Vector2(0, maxScroll);
      _scrollDrag.Reset();
    }

    private Rect CalculateConsoleRect()
    {
      Rect rootRect = _platform.GetConsoleRootRect();
      float consoleHeight = rootRect.height * 0.4f;

      return new Rect(
        rootRect.x,
        rootRect.y,
        rootRect.width,
        consoleHeight);
    }

    private void RenderHeader()
    {
      string headerText = _platform.IsMobile
        ? "Developer Console (Tap button to close)"
        : "Developer Console (Press Console key to close)";

      GUILayout.Label(headerText, _styles.LabelStyle);
    }

    private void RenderScrollableOutput(IReadOnlyList<string> messages, float consoleHeight)
    {
      float scrollHeight = CalculateScrollHeight(consoleHeight);
      Rect scrollViewRect = CalculateScrollViewRect(scrollHeight);

      _scrollPosition = _scrollDrag.HandleDrag(_scrollPosition, scrollViewRect);

      _scrollPosition = GUILayout.BeginScrollView(
        _scrollPosition,
        alwaysShowHorizontal: false,
        alwaysShowVertical: false,
        horizontalScrollbar: GUIStyle.none,
        verticalScrollbar: GUI.skin.verticalScrollbar,
        background: GUI.skin.scrollView,
        GUILayout.Height(scrollHeight));

      RenderMessages(messages);

      GUILayout.EndScrollView();
    }

    private void RenderMessages(IReadOnlyList<string> messages)
    {
      foreach (string message in messages)
        GUILayout.Label(message, _styles.OutputStyle);
    }

    private void RenderInputField(ref string inputText, bool isVisible, Action onSubmit)
    {
      if (_platform.IsMobile)
        RenderMobileInput(inputText, onSubmit);
      else
        RenderDesktopInput(ref inputText, isVisible, onSubmit);
    }

    private void RenderMobileInput(string inputText, Action onSubmit)
    {
      GUILayout.Label($"Input: {inputText}", _styles.InputStyle, GUILayout.Height(30));

      if (GUILayout.Button("Done", GUILayout.Height(40)))
        onSubmit?.Invoke();
    }

    private void RenderDesktopInput(ref string inputText, bool isVisible, Action onSubmit)
    {
      GUILayout.BeginHorizontal();

      GUI.SetNextControlName("ConsoleInput");
      inputText = GUILayout.TextField(inputText, _styles.InputStyle, GUILayout.Height(30));

      if (isVisible && Event.current.type == EventType.Layout)
        GUI.FocusControl("ConsoleInput");

      if (GUILayout.Button("Submit", GUILayout.Width(80), GUILayout.Height(30)))
        onSubmit?.Invoke();

      GUILayout.EndHorizontal();
    }

    private float CalculateScrollHeight(float consoleHeight) =>
      _platform.IsMobile ? consoleHeight - 120 : consoleHeight - 80;

    private Rect CalculateScrollViewRect(float scrollHeight)
    {
      Rect rootRect = _platform.GetConsoleRootRect();
      const float headerHeight = 20;

      return new Rect(
        rootRect.x,
        rootRect.y + headerHeight,
        rootRect.width,
        scrollHeight);
    }

    private float CalculateContentHeight(IReadOnlyList<string> messages)
    {
      if (messages.Count == 0)
        return 100f;

      float totalHeight = 0f;
      float availableWidth = Screen.width * 0.4f - 30;

      foreach (string message in messages)
      {
        GUIContent content = new GUIContent(message);
        float messageHeight = _styles.OutputStyle.CalcHeight(content, availableWidth);
        totalHeight += messageHeight;
      }

      return totalHeight + 10f;
    }
  }
}
