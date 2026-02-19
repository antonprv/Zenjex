// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using UnityEngine;

namespace DevConsole.Frontend.View
{
  public class ScrollDragHandler
  {
    private readonly bool _isEnabled;

    private bool _isDragging;
    private Vector2 _lastMousePosition;
    private Vector2 _dragVelocity;
    private float _inertia;

    public ScrollDragHandler(bool isEnabled) => _isEnabled = isEnabled;

    public Vector2 HandleDrag(Vector2 currentScroll, Rect viewRect)
    {
      if (!_isEnabled)
        return currentScroll;

      Event currentEvent = Event.current;

      if (!viewRect.Contains(currentEvent.mousePosition))
        return HandleOutsideView(currentScroll, currentEvent);

      return ProcessDragEvent(currentScroll, currentEvent);
    }

    public void Reset()
    {
      _isDragging = false;
      _dragVelocity = Vector2.zero;
      _inertia = 0f;
    }

    private Vector2 HandleOutsideView(Vector2 currentScroll, Event currentEvent)
    {
      if (currentEvent.type == EventType.MouseUp)
        _isDragging = false;

      return ApplyInertia(currentScroll);
    }

    private Vector2 ProcessDragEvent(Vector2 currentScroll, Event currentEvent)
    {
      switch (currentEvent.type)
      {
        case EventType.MouseDown:
          StartDrag(currentEvent);
          break;

        case EventType.MouseDrag when _isDragging:
          currentScroll = PerformDrag(currentScroll, currentEvent);
          break;

        case EventType.MouseUp when _isDragging:
          EndDrag(currentEvent);
          break;
      }

      return ApplyInertia(currentScroll);
    }

    private void StartDrag(Event currentEvent)
    {
      _isDragging = true;
      _lastMousePosition = currentEvent.mousePosition;
      _dragVelocity = Vector2.zero;
      _inertia = 0f;
      currentEvent.Use();
    }

    private Vector2 PerformDrag(Vector2 currentScroll, Event currentEvent)
    {
      Vector2 delta = currentEvent.mousePosition - _lastMousePosition;
      currentScroll.y -= delta.y;

      _dragVelocity = -delta;
      _inertia = 1f;

      _lastMousePosition = currentEvent.mousePosition;
      currentEvent.Use();

      return currentScroll;
    }

    private void EndDrag(Event currentEvent)
    {
      _isDragging = false;
      currentEvent.Use();
    }

    private Vector2 ApplyInertia(Vector2 currentScroll)
    {
      if (!ShouldApplyInertia())
      {
        StopInertia();
        return currentScroll;
      }

      currentScroll.y += _dragVelocity.y * _inertia;

      DecayInertia();

      return currentScroll;
    }

    private bool ShouldApplyInertia() =>
      _inertia > 0.01f && !_isDragging;

    private void DecayInertia()
    {
      const float decayFactor = 0.95f;
      _inertia *= decayFactor;
      _dragVelocity *= decayFactor;
    }

    private void StopInertia()
    {
      _inertia = 0f;
      _dragVelocity = Vector2.zero;
    }
  }
}
