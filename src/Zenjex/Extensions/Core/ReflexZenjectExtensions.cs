// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using Code.Zenjex.Extensions.Core;
using Reflex.Core;

namespace Code.Zenjex.Extensions.Core
{
  public static class ReflexZenjectExtensions
  {
    public static BindingBuilder<T> Bind<T>(this ContainerBuilder builder)
    {
      return new BindingBuilder<T>(builder);
    }
  }
}
