// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using DevConsole.Backend.Infrastructure.Services.StaticDataService.Interfaces;
using DevConsole.Backend.Infrastructure.Services.StaticDataService.Interfaces.Subservice;

namespace DevConsole.Backend.Infrastructure.Services.StaticDataService
{
  public class StaticDataService : IStaticDataService
  {
    public IBuildConfigSubservice BuildConfig { get; private set; }

    public StaticDataService(IBuildConfigSubservice buildConfigSubservice) =>
      BuildConfig = buildConfigSubservice;
  }
}
