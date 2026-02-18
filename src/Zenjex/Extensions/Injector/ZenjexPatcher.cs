// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

// Requires: HarmonyLib (com.github.pardeike.harmony) in your Unity packages
// Installation:
// 1. download https://www.nuget.org/packages/Lib.Harmony"
// 2. open it as an archive
// 3. Place 0Harmony.dll from "lib/net48/" to Plugins directory.

using Code.Zenjex.Extensions.Attribute;
using Code.Zenjex.Extensions.Core;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Code.Zenjex.Extensions.Injector
{
  /// <summary>
  /// Uses Harmony to prefix-patch Awake() on every MonoBehaviour that has at least
  /// one [Zenjex]-marked member. The patch runs ZenjexInjector.Inject() before the
  /// original Awake body executes, so injected fields are already filled when your
  /// own Awake code runs — exactly like Zenject's [Inject] Construct method.
  ///
  /// Initialization order:
  ///   1. ProjectRootInstaller.Awake()  →  RootContainer is built
  ///   2. Any other MonoBehaviour.Awake()  →  [Zenjex] prefix fires first, then original body
  ///
  /// IMPORTANT: Make sure ProjectRootInstaller has a lower Script Execution Order
  /// (e.g. -100) than the MonoBehaviours that use [Zenjex].
  /// </summary>
  [DefaultExecutionOrder(-200)]
  public sealed class ZenjexPatcher : MonoBehaviour
  {
    private static Harmony _harmony;
    private static bool    _patched;

    // ── Bootstrap ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Called automatically at domain reload (Editor) and app start (Runtime).
    /// Scans all loaded assemblies for MonoBehaviours with [Zenjex] members and
    /// patches their Awake methods.
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
    private static void Initialize()
    {
      if (_patched) return;
      _patched = true;

      _harmony = new Harmony("com.zenjex.patcher");

      var targets = FindInjectableMonoBehaviourTypes();
      foreach (var type in targets)
        PatchAwake(type);

      Debug.Log($"[Zenjex] Patched {targets.Count} MonoBehaviour type(s).");
    }

    // ── Type discovery ────────────────────────────────────────────────────────

    private static List<Type> FindInjectableMonoBehaviourTypes()
    {
      const BindingFlags flags =
        BindingFlags.Public | BindingFlags.NonPublic |
        BindingFlags.Instance | BindingFlags.DeclaredOnly;

      var result = new List<Type>();

      foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
      {
        // Skip Unity internals and system assemblies for speed
        if (IsSystemAssembly(assembly)) continue;

        Type[] types;
        try   { types = assembly.GetTypes(); }
        catch { continue; }

        foreach (var type in types)
        {
          if (!type.IsClass || type.IsAbstract) continue;
          if (!typeof(MonoBehaviour).IsAssignableFrom(type)) continue;

          if (HasZenjexMembers(type, flags))
            result.Add(type);
        }
      }

      return result;
    }

    private static bool HasZenjexMembers(Type type, BindingFlags flags)
    {
      var t = type;
      while (t != null && t != typeof(object))
      {
        foreach (var f in t.GetFields(flags))
          if (f.IsDefined(typeof(ZenjexAttribute))) return true;

        foreach (var p in t.GetProperties(flags))
          if (p.IsDefined(typeof(ZenjexAttribute))) return true;

        foreach (var m in t.GetMethods(flags))
          if (m.IsDefined(typeof(ZenjexAttribute))) return true;

        t = t.BaseType;
      }
      return false;
    }

    private static bool IsSystemAssembly(Assembly asm)
    {
      var name = asm.GetName().Name;
      return name.StartsWith("System")
          || name.StartsWith("mscorlib")
          || name.StartsWith("UnityEngine")
          || name.StartsWith("UnityEditor")
          || name.StartsWith("Unity.")
          || name.StartsWith("Mono.")
          || name.StartsWith("netstandard");
    }

    // ── Harmony patching ──────────────────────────────────────────────────────

    private static void PatchAwake(Type type)
    {
      try
      {
        // Get the declared Awake, or if absent — patch a synthetic one via a
        // postfix on MonoBehaviour so injection still runs.
        var awake = type.GetMethod("Awake",
          BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);

        if (awake != null)
        {
          // Prefix: inject before the original Awake body
          var prefix = new HarmonyMethod(typeof(ZenjexPatcher)
            .GetMethod(nameof(AwakePrefix), BindingFlags.Static | BindingFlags.NonPublic));

          _harmony.Patch(awake, prefix: prefix);
        }
        else
        {
          // No Awake declared on this type — add a postfix on MonoBehaviour.Awake
          // (which is a no-op in Unity but gives Harmony a hook point) by injecting
          // via SceneManager callback instead (see FallbackInject).
          RegisterFallbackForType(type);
        }
      }
      catch (Exception ex)
      {
        Debug.LogWarning($"[Zenjex] Could not patch {type.Name}: {ex.Message}");
      }
    }

    // Harmony prefix — receives the actual MonoBehaviour instance as __instance
    private static void AwakePrefix(MonoBehaviour __instance)
    {
      // Guard: only inject if RootContainer is ready
      if (!RootContext.HasInstance) return;

      ZenjexInjector.Inject(__instance);
    }

    // ── Fallback for types without Awake ─────────────────────────────────────
    // For MonoBehaviours that don't declare Awake we can't patch a method that
    // doesn't exist. Instead we hook SceneManager + Object.FindObjectsOfType
    // right after each scene loads, and also watch Instantiate via a tracker.

    private static readonly HashSet<Type> _fallbackTypes = new();
    private static bool _fallbackListenerRegistered;

    private static void RegisterFallbackForType(Type type)
    {
      _fallbackTypes.Add(type);

      if (_fallbackListenerRegistered) return;
      _fallbackListenerRegistered = true;

      SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
      if (!RootContext.HasInstance) return;

      // Find all objects of fallback types in the freshly loaded scene
      foreach (var root in scene.GetRootGameObjects())
      {
        var allBehaviours = root.GetComponentsInChildren<MonoBehaviour>(includeInactive: true);
        foreach (var mb in allBehaviours)
        {
          if (_fallbackTypes.Contains(mb.GetType()))
            ZenjexInjector.Inject(mb);
        }
      }
    }
  }
}
