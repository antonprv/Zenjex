// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using JetBrains.Annotations;
using System;

namespace Code.Zenjex.Extensions.Attribute
{
  /// <summary>
  /// Drop-in replacement for [Inject].
  /// Supports fields, properties, and Construct-methods on classes that inherit ZenjexBehaviour.
  /// </summary>
  [MeansImplicitUse(ImplicitUseKindFlags.Assign)]
  [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method)]
  public sealed class ZenjexAttribute : System.Attribute { }
}
