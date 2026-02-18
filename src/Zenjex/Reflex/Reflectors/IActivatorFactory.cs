// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using Reflex.Caching;
using Reflex.Delegates;
using System;
using System.Reflection;

namespace Reflex.Reflectors
{
  internal interface IActivatorFactory
  {
    ObjectActivator GenerateActivator(Type type, ConstructorInfo constructor, MemberParamInfo[] parameters);
    ObjectActivator GenerateDefaultActivator(Type type);
  }
}