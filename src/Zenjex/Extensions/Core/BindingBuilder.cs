// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using Reflex.Core;
using Reflex.Enums;
using System;
using System.Linq;

namespace Code.Zenjex.Extensions.Core
{
  public sealed class BindingBuilder<T>
  {
    private bool _committed;

    private readonly ContainerBuilder _builder;

    private Type _concrete;
    private Type[] _contracts;
    private Lifetime _lifetime = Lifetime.Transient;
    private Resolution _resolution = Resolution.Lazy;

    private object _instance;

    internal BindingBuilder(ContainerBuilder builder)
    {
      _builder = builder;
      _contracts = new[] { typeof(T) };
    }

    // -------- Zenject-like API --------

    public BindingBuilder<T> To<TConcrete>()
        where TConcrete : T
    {
      _concrete = typeof(TConcrete);
      return this;
    }

    public BindingBuilder<T> FromInstance(T instance)
    {
      _instance = instance;
      _lifetime = Lifetime.Singleton;
      return this;
    }

    public BindingBuilder<T> BindInterfaces()
    {
      var type = _concrete ?? typeof(T);

      _contracts = type.GetInterfaces();
      return this;
    }

    public BindingBuilder<T> BindInterfacesAndSelf()
    {
      var type = _concrete ?? typeof(T);

      _contracts = type
          .GetInterfaces()
          .Append(type)
          .ToArray();

      return this;
    }

    public void AsSingle()
    {
      AsSingleton();
    }

    public void AsSingleton()
    {
      _lifetime = Lifetime.Singleton;
      Commit();
    }

    public void AsTransient()
    {
      _lifetime = Lifetime.Transient;
      Commit();
    }

    public void AsScoped()
    {
      _lifetime = Lifetime.Scoped;
      Commit();
    }

    public void AsEagerSingleton()
    {
      _lifetime = Lifetime.Singleton;
      _resolution = Resolution.Eager;
      Commit();
    }

    // -------- Commit --------

    private void Commit()
    {
      // Protection against re-committing
      if (_committed)
        throw new InvalidOperationException("Binding already committed");

      // Checking for forbidden combination
      if (_lifetime == Lifetime.Transient && _resolution == Resolution.Eager)
        throw new InvalidOperationException("Transient + Eager binding is not allowed");

      _committed = true;

      // FromInstance is always a singleton
      if (_instance != null)
      {
        _builder.RegisterValue(_instance, _contracts);
        return;
      }

      // Type for registration: specific or generic (T)
      var concreteType = _concrete ?? typeof(T);

      // If contracts are not explicitly defined, bind to a concrete type
      var contractsToRegister = _contracts != null && _contracts.Length > 0
          ? _contracts
          : new[] { concreteType };

      // Registration in Reflex
      _builder.RegisterType(concreteType, contractsToRegister, _lifetime, _resolution);
    }
  }
}
