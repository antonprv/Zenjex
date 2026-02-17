// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using UnityEditor.IMGUI.Controls;

namespace Reflex.Editor.DebuggingWindow
{
  internal class TreeViewItem<T> : TreeViewItem where T : TreeElement
  {
    public T Data { get; }

    public TreeViewItem(int id, int depth, string displayName, T data) : base(id, depth, displayName)
    {
      Data = data;
    }
  }
}