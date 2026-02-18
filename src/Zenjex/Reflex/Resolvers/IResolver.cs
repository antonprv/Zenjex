// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using Reflex.Core;
using Reflex.Enums;
using System;

namespace Reflex.Resolvers
{
  public interface IResolver : IDisposable
  {
    Lifetime Lifetime { get; }
    Resolution Resolution { get; }
    Container DeclaringContainer { get; set; }
    object Resolve(Container resolvingContainer);
  }
}