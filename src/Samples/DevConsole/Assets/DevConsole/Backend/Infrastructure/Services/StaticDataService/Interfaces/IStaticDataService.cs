// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using DevConsole.Backend.Infrastructure.Services.StaticDataService.Interfaces.Subservice;

namespace DevConsole.Backend.Infrastructure.Services.StaticDataService.Interfaces
{
  public interface IStaticDataService
  {
    public IBuildConfigSubservice BuildConfig { get; }
  }
}
