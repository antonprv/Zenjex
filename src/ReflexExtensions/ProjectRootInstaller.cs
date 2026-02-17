// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using Reflex.Core;

using UnityEngine;

namespace Code.Common.Extensions.ReflexExtensions
{
  public abstract class ProjectRootInstaller : MonoBehaviour, IInstaller
  {
    public static Container RootContainer { get; private set; }

    private void Awake()
    {
      // If RootContainer is already created - do not create a second time
      if (RootContainer != null)
        return;

      // Create builder
      var builder = new ContainerBuilder();

      // Always register GameInstance first
      InstallGameInstance(builder);

      // Register all dependencies
      InstallBindings(builder);

      // Build container and save it as RootContainer
      RootContainer = builder.Build();

      LaunchGame();
    }

    public abstract void InstallGameInstance(ContainerBuilder builder);
    public abstract void InstallBindings(ContainerBuilder builder);
    public abstract void LaunchGame();
  }
}
