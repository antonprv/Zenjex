# Zenjex

**Zenject-compatible DI layer on top of Reflex — ported and fixed for Unity 6.3 LTS.**

---

## Going deeper

If you want to understand how everything works under the hood — the container hierarchy, resolver lifetimes, expression-tree-based activation, injection passes and their timing — all of that is covered in detail in the **[Architecture wiki](../../wiki/Architecture)**.

---

## What is this?

Zenjex is a production-ready dependency injection solution for Unity that solves a very specific, very painful problem: **your team uses Zenject/Extenject, you want to move to Unity 6, and you don't want to rewrite your injection layer from scratch or retrain anyone**.

Here's what's in the box:

- **Reflex 14.1.0** — the fastest DI framework for Unity, [benchmarked significantly ahead of Zenject](https://github.com/gustavopsantos/reflex#performance) — ported to **Unity 6.3 LTS** with all compatibility issues fixed
- **Zenjex Extensions** — a Zenject-style API written on top of Reflex, so the bindings your team already knows (`Bind<T>().To<TImpl>().AsSingle()`) just work
- **Fixed Reflex Debugger Window** — the editor debugging window is fully repaired for Unity 6.3; the original breaks in this version
- **DevConsole** — a complete real-world sample project showing the full integration pattern in action

---

## Why should a team care?

If your project is on an older Unity version and you're planning a migration to Unity 6, the DI framework is one of the first blockers. Extenject is not actively maintained, and its Unity 6 support is fragile. Reflex is maintained, fast, and clean — but switching to it cold means learning a new API, updating every installer, and retooling muscle memory across the whole team.

Zenjex eliminates that cost. The binding syntax is intentionally identical to Zenject. The injection attributes, the installer pattern, the way you resolve things — all of it maps to what your team already knows. Underneath, everything runs on Reflex, which means you get the performance benefit for free, without a rewrite.

Concretely:
- **Drop-in migration path**: if your team uses `Bind<T>().To<TImpl>().AsSingle()` today, that line works unchanged in Zenjex
- **Faster than Zenject**: Reflex resolves dependencies faster due to expression-tree-based activation and smarter caching — on Mono and especially on IL2CPP
- **Unity 6.3 LTS out of the box**: no fighting the editor, no broken debugger window, no hidden runtime crashes from version incompatibilities
- **No retraining**: the three injection patterns (attribute, base class, manual resolve) are familiar to anyone who has used Zenject or Extenject

---

## Integration guide

### Step 1 — Copy the Zenjex folder into your project

Drop the entire `Zenjex` folder into your project's `Assets`. That's it — no package manager, no git submodule. The folder contains both Reflex and the Zenjex extension layer, each with its own `.asmdef`, and Unity will pick them up automatically.

> **Unity version**: the included Reflex build targets Unity 6.3 LTS. It will not work correctly on older versions without reverting the Unity 6-specific fixes.

### Step 2 — Create a ReflexSettings asset

In the Project window, find or create a `Resources` folder. Right-click inside it and choose **Create → Reflex → Settings**. This creates the `ReflexSettings.asset` that Reflex requires at runtime. Without it, Reflex will fail to locate its configuration and the container will not initialize.

In the inspector you will find the **Root Scopes** list — this cannot be left empty. It needs a **RootScope prefab**, which you create by right-clicking in the Project window and choosing **Create → Reflex → RootScope**. Assign that prefab to the list.

You will add your installer component to the RootScope prefab in Step 4.

### Step 3 — Add a SceneScope to every scene

Reflex requires a **SceneScope** in each scene. The container is not global or static — it is created fresh per scene and destroyed when the scene unloads, so every scene needs its own scope.

For each scene in your project, go to **GameObject → Reflex → SceneScope** in the top menu. This adds a `SceneScope` GameObject with a `ContainerScope` component to the scene.

> Without a `SceneScope`, Reflex won't initialize a container for that scene and nothing will be injected.

### Step 4 — Create a class inheriting from `ProjectRootInstaller`

Create a new C# class that inherits from `ProjectRootInstaller`. Add it as a component both to the **RootScope prefab** created in Step 2 and to the **SceneScope** GameObject in each scene. Reflex picks up installers via `GetComponentsInChildren` on the scope.

```csharp
using System.Collections;
using Reflex.Core;
using Zenjex.Extensions.Core;

[DefaultExecutionOrder(-250)]
public class GameInstaller : ProjectRootInstaller
{
    public override void InstallBindings(ContainerBuilder builder)
    {
        // Bind all your services here — see Step 4
    }

    // Optional. Implement the Unreal Engine-style GameInstance pattern:
    // run coroutines here if you need async initialization before the game starts
    // (e.g. loading remote config, warming up an asset bundle, etc.)
    // If you don't need this, just leave the yield return null.
    public override IEnumerator InstallGameInstanceRoutine()
    {
        yield return null;
    }

    // Called after InstallGameInstanceRoutine() completes.
    // Can be left empty, or used to kick off your game's entry point —
    // for example, starting a StateMachine, loading the first scene,
    // or initializing a GameInstance that owns the rest of the startup flow.
    public override void LaunchGame()
    {
        // _gameInstance.Launch();
    }
}
```

`ProjectRootInstaller` runs at execution order `-280`, which guarantees the container is fully built before any other `Awake()` in the scene fires.

### Step 5 — Bind your dependencies inside `InstallBindings`

Use the Zenject-style fluent API to register everything the scene needs:

```csharp
public override void InstallBindings(ContainerBuilder builder)
{
    // Bind interface to implementation, singleton lifetime
    builder.Bind<IInputService>().To<InputService>().AsSingle();

    // Bind concrete type directly
    builder.Bind<AnalyticsManager>().AsSingle();

    // Bind a pre-existing instance
    builder.Bind<IConfig>().FromInstance(myConfigAsset).AsSingle();

    // Bind to all implemented interfaces at once
    builder.Bind<PlayerController>().BindInterfacesAndSelf().AsSingle();

    // Transient — new instance on every resolve
    builder.Bind<IEnemyFactory>().To<EnemyFactory>().AsTransient();
}
```

---

## Bonus: post-initialization binding

Sometimes you need to register something into the container *after* it's already been built — for example, a runtime-loaded config or a service created during `InstallGameInstanceRoutine`. Use `RootContext.Runtime` for this:

```csharp
// Anywhere, after the container is built:
RootContext.Runtime
    .Bind<IRuntimeConfig>()
    .FromInstance(loadedConfig)
    .AsSingle();
```

`RootContext.Runtime` gives you direct access to the live container. `RootContext.HasInstance` lets you safely check whether it exists yet before calling it.

---

## Injecting dependencies

Zenjex supports three injection patterns. Pick the one that fits the situation.

### 1. Direct resolve — `RootContext.Resolve<T>()`

Works everywhere, at any time after the container is built. No base class required, no attribute needed. Just call it.

```csharp
private void Awake()
{
    var config = RootContext.Resolve<IGameConfig>();
    var input  = RootContext.Resolve<IInputService>();
}
```

Best for: controllers, managers, or any class where you want an explicit, traceable dependency grab.

---

### 2. Attribute injection on a plain `MonoBehaviour` — `[Zenjex]`

Mark fields (or properties, or inject-methods) with `[Zenjex]`. **The object must already be present in the scene** when the bootstrap scene loads. Injection happens during `ProjectRootInstaller.Awake()`, before any other `Awake()` in the scene runs — so by the time your `Awake()` fires, the fields are already populated.

```csharp
using Zenjex.Extensions.Attribute;

public class HUDController : MonoBehaviour
{
    [Zenjex] private IPlayerService _player;
    [Zenjex] private IAudioService _audio;

    private void Awake()
    {
        // _player and _audio are already injected here
        _player.OnHealthChanged += UpdateHealthBar;
    }
}
```

> If the object is in an **additively loaded scene**, injection happens after that scene loads — which means it arrives *after* `Awake()` has already run. In that case, the fields will be null inside `Awake()`. Zenjex will log a `ZNX-LATE` warning to make this visible. Use pattern #3 below if you need guaranteed pre-`Awake()` injection for dynamically loaded objects.

---

### 3. `ZenjexBehaviour` — guaranteed pre-`Awake()` injection, works for runtime-instantiated objects

Inherit from `ZenjexBehaviour` instead of `MonoBehaviour`. This gives the object its own `Awake()` at execution order `-100`, which means injection is guaranteed to happen before any user-level `Awake()` — even for prefabs that are `Instantiate()`-d at runtime. No manual wiring after `Instantiate()` needed — it just works.

Instead of `Awake()`, override `OnAwake()`. The injected fields are already populated when it runs.

```csharp
using Zenjex.Extensions.Attribute;
using Zenjex.Extensions.Injector;

public class Enemy : ZenjexBehaviour
{
    [Zenjex] private IEnemyConfig _config;
    [Zenjex] private IAudioService _audio;

    protected override void OnAwake()
    {
        base.OnAwake(); // always call this first

        // _config and _audio are injected — safe to use
        _audio.Play(_config.SpawnSound);
    }
}
```

---

## Injection timing summary

| Pattern | Object must be in scene? | Fields ready in `Awake()`? |
|---|---|---|
| `RootContext.Resolve<T>()` | No | Yes (you control it) |
| `[Zenjex]` on `MonoBehaviour` | Yes (at load time) | Yes |
| `[Zenjex]` on `MonoBehaviour` (additive scene) | Yes | **No** — ZNX-LATE warning |
| `ZenjexBehaviour` + `[Zenjex]` | No | Yes |

---

## Sample projects

The included **DevConsole** project is a full working implementation: it has a `GameInstaller`, multiple services bound via interface, `[Zenjex]` fields on scene objects, and `RootContext.Resolve<T>()` used in controllers. It's the fastest way to see everything in context.

For a larger-scale example, **[LoneBrawler](https://github.com/antonprv/LoneBrawler)** is a complete midcore browser/mobile game built with this framework. It shows how Zenjex holds up across a full production codebase — multiple scenes, complex service graphs, real gameplay systems.

If you want to understand how the framework works internally — how the container hierarchy is structured, how injection passes are timed, how expression-tree activation works — the **[project wiki](../../wiki)** has a full architecture breakdown of both Reflex and the Zenjex layer.

---

## Requirements

- **Unity 6.3+** — tested on Unity 6.3 LTS, compatible with every Unity version past 6.3

---

*Created by Anton Piruev, 2026. Any direct commercial use of derivative work is strictly prohibited.*