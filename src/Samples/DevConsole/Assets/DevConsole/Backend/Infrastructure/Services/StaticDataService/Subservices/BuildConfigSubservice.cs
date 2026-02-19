// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using DevConsole.Backend.Data.Configs;
using DevConsole.Backend.Data.Configs.Types;
using DevConsole.Backend.Infrastructure.Services.StaticDataService.Interfaces.Subservice;
using Code.Infrastructure.Services.AssetManagement.Interfaces;
using DevConsole.Backend.Infrastructure.Services.AssetManagement;

namespace DevConsole.Backend.Infrastructure.Services.StaticDataService.Subservices
{
  public class BuildConfigSubservice : IBuildConfigSubservice
  {
    public BuildConfiguration Current { get; private set; }

    private static GameBuildData _buildConfig;

    public BuildConfigSubservice(IAssetLoader assetLoader)
    {
      _buildConfig = assetLoader
      .Load<GameBuildData>(StaticDataPaths.BuildConfigPath);
      Current = _buildConfig.BuildConfiguration;
    }

    public bool IsDevelopment() => Current == BuildConfiguration.Development;
  }
}
