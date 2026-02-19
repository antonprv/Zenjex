// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

#if UNITY_EDITOR

using DevConsole.Backend.Data.Configs;
using DevConsole.Backend.Data.Configs.Types;
using Assets.DevConsole.Editor.Utils;
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Assets.DevConsole.Editor
{
  [CustomEditor(typeof(GameBuildData))]
  public class GameBuildDataEditor : ManualSaveEditor
  {
    protected override void DrawInspector() =>
      DrawDefaultInspectorWithManualSave();
  }

  [CustomPropertyDrawer(typeof(NoNoneAttribute))]
  public class NoNoneDrawer : PropertyDrawer
  {
    private const string NoneValueName = "None";

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
      if (!IsEnumProperty(property))
      {
        EditorGUI.PropertyField(position, property, label);
        return;
      }

      Type enumType = fieldInfo?.FieldType;

      if (!IsValidEnumType(enumType))
      {
        EditorGUI.PropertyField(position, property, label);
        return;
      }

      DrawEnumWithoutNone(position, property, label);
    }

    private static bool IsEnumProperty(SerializedProperty property) =>
      property.propertyType == SerializedPropertyType.Enum;

    private static bool IsValidEnumType(Type type) =>
      type != null && type.IsEnum;

    private void DrawEnumWithoutNone(Rect position, SerializedProperty property, GUIContent label)
    {
      string[] enumNames = property.enumNames;
      int noneIndex = FindNoneIndex(enumNames);

      int[] allowedIndices = GetAllowedIndices(enumNames.Length, noneIndex);
      string[] allowedNames = GetAllowedNames(enumNames, allowedIndices);

      int currentIndex = GetCurrentAllowedIndex(property, allowedIndices);
      int newIndex = EditorGUI.Popup(position, label.text, currentIndex, allowedNames);

      UpdatePropertyIfChanged(property, allowedIndices, newIndex);
    }

    private static int FindNoneIndex(string[] enumNames) =>
      Array.FindIndex(enumNames, name => name == NoneValueName);

    private static int[] GetAllowedIndices(int totalCount, int noneIndex)
    {
      return Enumerable.Range(0, totalCount)
          .Where(i => i != noneIndex)
          .ToArray();
    }

    private static string[] GetAllowedNames(string[] enumNames, int[] allowedIndices) =>
      allowedIndices.Select(i => enumNames[i]).ToArray();

    private static int GetCurrentAllowedIndex(SerializedProperty property, int[] allowedIndices)
    {
      int currentIndex = Array.IndexOf(allowedIndices, property.enumValueIndex);

      // If current value is None or invalid, select first allowed value
      if (currentIndex < 0 && allowedIndices.Length > 0)
      {
        property.enumValueIndex = allowedIndices[0];
        return 0;
      }

      return currentIndex;
    }

    private static void UpdatePropertyIfChanged(
      SerializedProperty property,
      int[] allowedIndices,
      int newIndex
      )
    {
      if (newIndex >= 0 && newIndex < allowedIndices.Length)
        property.enumValueIndex = allowedIndices[newIndex];
    }
  }
}

#endif
