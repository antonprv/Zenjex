// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using System.Collections;

using Reflex.Core;

using UnityEngine;

namespace Code.Common.Extensions.ReflexExtensions
{
  public abstract class ProjectRootInstaller : MonoBehaviour, IInstaller
  {
    public static Container RootContainer { get; private set; }

    private void Awake()
    {
      if (RootContainer != null)
        return;

      var builder = new ContainerBuilder();
      InstallBindings(builder);
      RootContainer = builder.Build();

      StartCoroutine(LateInitRoutine());
    }

    private IEnumerator LateInitRoutine()
    {
      yield return InstallGameInstanceRoutine();
      LaunchGame();
    }

    public abstract IEnumerator InstallGameInstanceRoutine();
    public abstract void InstallBindings(ContainerBuilder builder);
    public abstract void LaunchGame();
  }
}
