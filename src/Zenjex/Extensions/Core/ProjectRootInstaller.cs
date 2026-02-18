// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using Reflex.Core;
using System;
using System.Collections;
using UnityEngine;

namespace Code.Zenjex.Extensions.Core
{
  [DefaultExecutionOrder(-280)]
  public abstract class ProjectRootInstaller : MonoBehaviour, IInstaller
  {
    public static Container RootContainer { get; private set; }

    /// <summary>
    /// Fires synchronously inside Awake(), immediately after RootContainer is built.
    /// At this point all subsequent Awake() calls in the scene have not yet run,
    /// so this is the earliest safe moment to inject [Zenjex] fields.
    /// </summary>
    public static event Action OnContainerReady;

    /// <summary>
    /// Fires after InstallGameInstanceRoutine() completes and LaunchGame() is called.
    /// Use this for a second injection pass if InstallGameInstanceRoutine() added
    /// new bindings to the container that earlier-injected objects may depend on.
    /// </summary>
    public static event Action OnGameLaunched;

    private void Awake()
    {
      if (RootContainer != null)
        return;

      var builder = new ContainerBuilder();
      InstallBindings(builder);
      RootContainer = builder.Build();

      // First injection pass — before any other Awake() in the scene runs.
      OnContainerReady?.Invoke();

      StartCoroutine(LateInitRoutine());
    }

    private IEnumerator LateInitRoutine()
    {
      yield return InstallGameInstanceRoutine();
      LaunchGame();

      // Second injection pass — after InstallGameInstanceRoutine() may have
      // added new bindings that the first pass could not resolve.
      OnGameLaunched?.Invoke();
    }

    public abstract IEnumerator InstallGameInstanceRoutine();
    public abstract void InstallBindings(ContainerBuilder builder);
    public abstract void LaunchGame();
  }
}