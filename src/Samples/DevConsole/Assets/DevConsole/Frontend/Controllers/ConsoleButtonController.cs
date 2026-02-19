// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using DevConsole.Backend.Infrastructure.Services.StaticDataService.Interfaces;
using Zenjex.Extensions.Attribute;
using Zenjex.Extensions.Injector;

namespace DevConsole.Frontend.Controllers
{
  public class ConsoleButtonController : ZenjexBehaviour
  {
    [Zenjex] private IStaticDataService _staticData;

    protected override void OnAwake()
    {
      if (!IsDevelopmentBuild())
        gameObject.SetActive(false);
    }

    private bool IsDevelopmentBuild() =>
      _staticData.BuildConfig.IsDevelopment();
  }
}
