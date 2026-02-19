// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using DevConsole.Backend.Data.Configs.Types;

namespace DevConsole.Backend.Infrastructure.Services.StaticDataService.Interfaces.Subservice
{
  public interface IBuildConfigSubservice
  {
    BuildConfiguration Current { get; }
    public bool IsDevelopment();
  }
}
