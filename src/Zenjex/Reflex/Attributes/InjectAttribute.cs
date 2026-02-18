// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using JetBrains.Annotations;
using System;

namespace Reflex.Attributes
{
  [MeansImplicitUse(ImplicitUseKindFlags.Assign)]
  [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method)]
  public class InjectAttribute : Attribute
  {
  }
}