// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using Reflex.Core;

namespace Code.Common.Extensions.ReflexExtensions
{
  public static class ReflexZenjectExtensions
  {
    public static BindingBuilder<T> Bind<T>(this ContainerBuilder builder)
    {
      return new BindingBuilder<T>(builder);
    }
  }
}
