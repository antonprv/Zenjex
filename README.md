🇬🇧 [English](#english) · 🇷🇺 [Русский](#русский)

---

## English

**Zenject-compatible DI layer on top of Reflex - ported and fixed for Unity 6.**

---

### Going deeper

If you want to understand how everything works under the hood - the container hierarchy, resolver lifetimes, expression-tree-based activation, injection passes and their timing - all of that is covered in detail in the **[Architecture wiki](https://github.com/antonprv/Zenjex/wiki)**.

---

### What is this?

Zenjex is a production-ready dependency injection solution for Unity that solves a very specific, very painful problem: **your team uses Zenject/Extenject, you want to move to Unity 6, and you don't want to rewrite your injection layer from scratch or retrain anyone**.

Here's what's in the box:

- **Reflex 14.1.0** - the fastest DI framework for Unity, [benchmarked significantly ahead of Zenject](https://github.com/gustavopsantos/reflex#performance) - ported to **Unity 6.3 LTS** with all compatibility issues fixed
- **Zenjex Extensions** - a Zenject-style API written on top of Reflex, so the bindings your team already knows (`Bind<T>().To<TImpl>().AsSingle()`) just work
- **Fixed Reflex Debugger Window** - the editor debugging window is fully repaired for Unity 6.3; the original breaks in this version
- **DevConsole** - a complete real-world sample project showing the full integration pattern in action

---

### Why should a team care?

If your project is on an older Unity version and you're planning a migration to Unity 6, the DI framework is one of the first blockers. Extenject is not actively maintained, and its Unity 6 support is fragile. Reflex is maintained, fast, and clean - but switching to it cold means learning a new API, updating every installer, and retooling muscle memory across the whole team.

Zenjex eliminates that cost. The binding syntax is intentionally identical to Zenject. The injection attributes, the installer pattern, the way you resolve things - all of it maps to what your team already knows. Underneath, everything runs on Reflex, which means you get the performance benefit for free, without a rewrite.

Concretely:
- **Drop-in migration path**: if your team uses `Bind<T>().To<TImpl>().AsSingle()` today, that line works unchanged in Zenjex
- **Faster than Zenject**: Reflex resolves dependencies faster due to expression-tree-based activation and smarter caching - on Mono and especially on IL2CPP
- **Unity 6 out of the box**: no fighting the editor, no broken debugger window, no hidden runtime crashes from version incompatibilities
- **No retraining**: the three injection patterns (attribute, base class, manual resolve) are familiar to anyone who has used Zenject or Extenject

---

### Integration guide

#### Step 1 - Copy the Zenjex folder into your project

Drop the entire `Zenjex` folder into your project's `Assets`. That's it - no package manager, no git submodule. The folder contains both Reflex and the Zenjex extension layer, each with its own `.asmdef`, and Unity will pick them up automatically.

> **Unity version**: the included Reflex build targets Unity 6.3 LTS. It will not work correctly on older versions without reverting the Unity 6-specific fixes.

#### Step 2 - Create a class inheriting from `ProjectRootInstaller`

`ProjectRootInstaller` is the global composition root - the Zenjex equivalent of Zenject's `ProjectContext + MonoInstaller`. Create one concrete subclass, add it as a component to a **persistent GameObject** in your bootstrap scene, and implement the three abstract members.

```csharp
using System.Collections;
using Reflex.Core;
using Zenjex.Extensions.Core;

[DefaultExecutionOrder(-250)]
public class GameInstaller : ProjectRootInstaller
{
    public override void InstallBindings(ContainerBuilder builder)
    {
        // Register all global services here - see Step 4
    }

    // Async setup between InstallBindings and LaunchGame.
    // Yield Addressables, register runtime-only bindings, etc.
    // If unused, just leave the yield return null.
    public override IEnumerator InstallGameInstanceRoutine()
    {
        yield return null;
    }

    // Called after InstallGameInstanceRoutine() completes and all IInitializables are initialized.
    // Use to kick off your game's entry point - for example, starting a StateMachine,
    // loading the first scene, or launching a GameInstance.
    // If you use IInitializable for your GSM entry point, leave this empty.
    public override void LaunchGame()
    {
        // _gameInstance.Launch();
    }
}
```

`ProjectRootInstaller` runs at execution order `-280`, which guarantees the container is fully built before any other `Awake()` in the scene fires.

For larger projects, organise bindings into multiple child `MonoInstaller` components and delegate to them from the root installer:

```csharp
public class AppInstaller : ProjectRootInstaller
{
    [SerializeField] private InfrastructureInstaller _infra;
    [SerializeField] private GameplayInstaller       _gameplay;

    public override void InstallBindings(ContainerBuilder builder)
    {
        _infra.InstallBindings(builder);
        _gameplay.InstallBindings(builder);
    }

    public override IEnumerator InstallGameInstanceRoutine()
    {
        yield return _infra.LoadAddressablesRoutine();
    }

    public override void LaunchGame() { }
}
```

#### Step 3 - Add a `SceneInstaller` to every gameplay scene

For each scene that has scene-local services, create a class inheriting from `SceneInstaller` and add it to any GameObject in that scene. `SceneInstaller` creates a **child container** that inherits all global bindings from `RootContainer` and adds scene-local bindings on top.

The child container is disposed automatically when the scene unloads. All scene-local `IInitializable` and `IDisposable` services are lifecycle-managed by the installer.

`SceneInstaller` runs at execution order `-200` - after `ProjectRootInstaller` (`-280`), but before any regular `Awake()` (`0`).

```csharp
using Reflex.Core;
using Zenjex.Extensions.SceneContext;

public class GameplaySceneInstaller : SceneInstaller
{
    [SerializeField] private LevelProgressWatcher _levelProgressWatcher;

    public override void InstallBindings(ContainerBuilder builder)
    {
        // Scene-local bindings - only visible within this scene's container
        builder.BindInstance(_levelProgressWatcher).AsSingle();

        // Scoped binding from the global installer gets a fresh instance
        // here, with access to both global services and the local watcher above
        builder.Bind<LevelProgressServiceResolver>()
               .BindInterfacesAndSelf()
               .CopyIntoDirectSubContainers()
               .NonLazy();
    }

    // Optional: called once after the scene container is ready
    protected override void OnInstalled() { }
}
```

To resolve from the active scene container anywhere in code, use `ZenjexSceneContext`:

```csharp
// From any global service or GSM state:
var sceneContainer = ZenjexSceneContext.GetActive();
var watcher = sceneContainer.Resolve<LevelProgressWatcher>();

// Or directly:
ZenjexSceneContext.Resolve<LevelProgressWatcher>();
```

`ZenjexSceneContext.HasActiveScene` lets you check whether a scene container is currently registered before calling it.

> Scenes that have no scene-local services do not need a `SceneInstaller` - global bindings from `RootContainer` are always available via `RootContext.Resolve<T>()`.

#### Step 4 - Bind your dependencies inside `InstallBindings`

Use the Zenject-style fluent API to register everything the container needs.

**Basic bindings**

```csharp
public override void InstallBindings(ContainerBuilder builder)
{
    // Bind interface to implementation, lazy singleton
    builder.Bind<IInputService>().To<InputService>().AsSingle();

    // Bind concrete type directly
    builder.Bind<AnalyticsManager>().AsSingle();

    // Bind a pre-existing instance (always registers as singleton)
    builder.Bind<IConfig>().FromInstance(myConfigAsset).AsSingle();

    // Shorthand for the above - identical result
    builder.BindInstance(myConfigAsset).AsSingle();

    // Bind to all implemented interfaces at once
    builder.Bind<PlayerController>().BindInterfacesAndSelf().AsSingle();

    // Bind to interfaces only (without the concrete type)
    builder.Bind<AudioService>().BindInterfaces().AsSingle();

    // Transient - new instance on every resolve
    builder.Bind<IEnemyFactory>().To<EnemyFactory>().AsTransient();

    // Eager singleton - created immediately when the container is built
    // Zenject: .AsSingle().NonLazy()  ->  Zenjex: .NonLazy()
    builder.Bind<IGameStateMachine>().To<GameStateMachine>().BindInterfacesAndSelf().NonLazy();
}
```

**Prefab-based bindings**

```csharp
// Instantiate a prefab at resolve-time and extract a component from it.
// Always singleton + lazy - prefab is spawned once, on first resolve.
builder.Bind<ICurtainService>()
       .To<CurtainService>()
       .FromComponentInNewPrefab(curtainPrefab)
       .WithGameObjectName("Curtain")         // renames the root GameObject
       .UnderTransformGroup("Infrastructure") // places it under a named group at scene root
       .BindInterfacesAndSelf()
       .NonLazy();

// Or place under a specific Transform instead of a named group:
builder.Bind<IHUDService>()
       .To<HUDService>()
       .FromComponentInNewPrefab(hudPrefab)
       .UnderTransform(uiRootTransform)
       .AsSingle();

// Find an existing component in the scene hierarchy:
builder.Bind<IPlayerCamera>()
       .To<PlayerCamera>()
       .FromComponentInHierarchy()  // includeInactive: true by default
       .AsSingle();
```

**Constructor arguments**

```csharp
// Mix runtime-created objects with container-managed services.
// Provided values are matched to constructor parameters by type.
// Unmatched parameters are resolved from the container as usual.
var playerInput = Instantiate(playerInputPrefab).GetComponent<PlayerInput>();

builder.Bind<IInputService>()
       .To<InputService>()
       .WithArguments(playerInput, cinemachineInputProvider)
       .AsSingle();

// InputService constructor:
// public InputService(PlayerInput pi, CinemachineInputProvider cip, ILoggingService log)
// pi, cip <- WithArguments  |  log <- container
```

**Scoped bindings**

```csharp
// Scoped: fresh instance per SceneInstaller sub-container.
// The scoped object gets both global and scene-local dependencies in its constructor.
builder.Bind<LevelProgressServiceResolver>()
       .BindInterfacesAndSelf()
       .CopyIntoDirectSubContainers()
       .NonLazy();

// Equivalent shorthand terminator:
builder.Bind<LevelProgressServiceResolver>()
       .BindInterfacesAndSelf()
       .AsScoped();
```

---

### Lifecycle

`ProjectRootInstaller` guarantees the following execution order every time the game starts:

1. **`InstallBindings(builder)`** - register all global services. Runs synchronously inside `Awake()` at execution order `-280`.
2. **`OnContainerReady`** - first `[Zenjex]` injection pass across all loaded scenes. Because this fires inside `Awake()` at order `-280`, all subsequent `Awake()` calls in the scene already see injected fields.
3. **`InstallGameInstanceRoutine()`** - async setup: load Addressables, register runtime-only bindings, etc.
4. **`IInitializable.Initialize()`** - called automatically on every service that implements `IInitializable` and is registered under that interface. Fires after step 3, before `LaunchGame()`.
5. **`LaunchGame()`** - your game's entry point.
6. **`OnGameLaunched`** - second `[Zenjex]` injection pass, covers objects that depend on bindings added in step 3.

---

### IInitializable

Implement `IInitializable` on any service to receive a guaranteed `Initialize()` call after the container is fully built and all dependencies are injected - but before `LaunchGame()` runs. This is a drop-in replacement for Zenject's `IInitializable`.

To be discovered, the service **must** expose `IInitializable` as a contract - use `BindInterfaces()` or `BindInterfacesAndSelf()` in the installer.

```csharp
using Zenjex.Extensions.Lifecycle;

public class GameStateMachine : IInitializable
{
    private readonly StateFactory _stateFactory;

    public GameStateMachine(StateFactory stateFactory) =>
        _stateFactory = stateFactory;

    public void Initialize() => Enter<BootstrapState>();
}

// In installer:
builder.Bind<GameStateMachine>()
       .BindInterfacesAndSelf()
       .AsEagerSingleton();
```

`IInitializable` works in both the global container (`ProjectRootInstaller`) and scene-scoped containers (`SceneInstaller`). Scene-local initializables are called right after the scene container is built, before `OnInstalled()`.

---

### Post-initialization binding

Sometimes you need to register something into the container after it's already been built - for example, a runtime-loaded config or a service created during `InstallGameInstanceRoutine`. Use `RootContext.Runtime` for this:

```csharp
// Anywhere, after the container is built:
RootContext.Runtime
    .Bind<IRuntimeConfig>()
    .FromInstance(loadedConfig)
    .AsSingle();
```

`RootContext.Runtime` gives you direct access to the live container. `RootContext.HasInstance` lets you safely check whether it exists yet before calling it.

---

### Injecting dependencies

Zenjex supports three injection patterns. Pick the one that fits the situation.

#### 1. Direct resolve - `RootContext.Resolve<T>()`

Works everywhere, at any time after the container is built. No base class required, no attribute needed. Just call it.

```csharp
private void Awake()
{
    var config = RootContext.Resolve<IGameConfig>();
    var input  = RootContext.Resolve<IInputService>();
}
```

Best for: controllers, managers, or any class where you want an explicit, traceable dependency grab.

#### 2. Attribute injection on a plain `MonoBehaviour` - `[Zenjex]`

Mark fields, properties, or inject-methods with `[Zenjex]`. **The object must already be present in the scene** when the bootstrap scene loads. Injection happens during `ProjectRootInstaller.Awake()`, before any other `Awake()` in the scene runs - so by the time your `Awake()` fires, the fields are already populated.

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

> If the object is in an **additively loaded scene**, injection happens after that scene loads - which means it arrives *after* `Awake()` has already run. In that case, the fields will be null inside `Awake()`. Zenjex will log a `ZNX-LATE` warning to make this visible. Use pattern #3 below if you need guaranteed pre-`Awake()` injection for dynamically loaded objects.

#### 3. `ZenjexBehaviour` - guaranteed pre-`Awake()` injection, works for runtime-instantiated objects

Inherit from `ZenjexBehaviour` instead of `MonoBehaviour`. This gives the object its own `Awake()` at execution order `-100`, which means injection is guaranteed to happen before any user-level `Awake()` - even for prefabs that are `Instantiate()`-d at runtime.

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

        // _config and _audio are injected - safe to use
        _audio.Play(_config.SpawnSound);
    }
}
```

#### 4. Manual injection - `ZenjexRunner.InjectGameObject()`

For objects created at runtime via `Instantiate()` that do not inherit from `ZenjexBehaviour`, call `InjectGameObject` immediately after instantiation. It walks the full hierarchy and injects all `[Zenjex]`-marked members.

```csharp
var enemy = Instantiate(enemyPrefab);
ZenjexRunner.InjectGameObject(enemy);
```

> This is only needed for plain `MonoBehaviour` subclasses. `ZenjexBehaviour` handles itself automatically.

---

### Injection timing summary

| Pattern | Object must be in scene at load? | Fields ready in `Awake()`? |
|---|---|---|
| `RootContext.Resolve<T>()` | No | Yes (you control it) |
| `[Zenjex]` on `MonoBehaviour` (bootstrap scene) | Yes | Yes |
| `[Zenjex]` on `MonoBehaviour` (additive scene) | Yes | **No** - ZNX-LATE warning |
| `ZenjexBehaviour` + `[Zenjex]` | No | Yes |
| `ZenjexRunner.InjectGameObject()` | No | After manual call |
| `SceneInstaller` (inherits `ZenjexBehaviour`) | No | Yes |

---

### Sample projects

The included **DevConsole** project is a full working implementation: it has a `GameInstaller`, multiple services bound via interface, `[Zenjex]` fields on scene objects, and `RootContext.Resolve<T>()` used in controllers. It's the fastest way to see everything in context.

For a larger-scale example, **[LoneBrawler](https://github.com/antonprv/LoneBrawler)** is a complete midcore browser/mobile game built with this framework. It shows how Zenjex holds up across a full production codebase - multiple scenes, complex service graphs, real gameplay systems.

If you want to understand how the framework works internally - how the container hierarchy is structured, how injection passes are timed, how expression-tree activation works - the **[project wiki](../../wiki)** has a full architecture breakdown of both Reflex and the Zenjex layer.

---

### Requirements

- **Unity 6.3+** - tested on Unity 6.3 LTS, compatible with every Unity version past 6.3

---

*Created by Anton Piruev, 2026. Any direct commercial use of derivative work is strictly prohibited.*

---
---

## Русский

**DI-слой, совместимый с Zenject, поверх Reflex - портирован и исправлен для Unity 6.**

---

### Подробнее о внутреннем устройстве

Если хочется разобраться, как всё работает под капотом - иерархия контейнеров, лайфтаймы резолверов, активация через expression tree, порядок injection-пассов - всё это подробно описано в **[вики проекта](https://github.com/antonprv/Zenjex/wiki)**.

---

### Что это такое?

Zenjex - production-ready решение для dependency injection в Unity, которое закрывает конкретную и болезненную проблему: **команда работает на Zenject/Extenject, нужно переехать на Unity 6, и никто не хочет переписывать DI-слой с нуля или переучиваться**.

Что входит в комплект:

- **Reflex 14.1.0** - самый быстрый DI-фреймворк для Unity, [заметно опережающий Zenject в бенчмарках](https://github.com/gustavopsantos/reflex#performance) - портирован на **Unity 6.3 LTS** с исправлением всех проблем совместимости
- **Zenjex Extensions** - Zenject-подобный API поверх Reflex, так что привычные биндинги (`Bind<T>().To<TImpl>().AsSingle()`) работают без изменений
- **Исправленное окно отладчика Reflex** - редакторное окно дебаггера полностью починено для Unity 6.3, в оригинале оно сломано в этой версии
- **DevConsole** - полноценный пример проекта, демонстрирующий всю интеграцию в действии

---

### Зачем это нужно команде?

Если проект сидит на старой версии Unity и переезд на Unity 6 в планах, DI-фреймворк - один из первых стопоров. Extenject фактически не поддерживается, и его работа с Unity 6 держится на честном слове. Reflex поддерживается, работает быстро и чисто - но холодная миграция на него означает изучение нового API, переписывание каждого инсталлера и переучивание всей команды.

Zenjex снимает эту проблему. Синтаксис биндингов намеренно идентичен Zenject. Атрибуты инъекций, паттерн инсталлеров, способ резолва - всё это совпадает с тем, что команда уже знает. Под капотом работает Reflex, так что прирост производительности достаётся бесплатно, без переписывания.

Конкретно:
- **Путь миграции без переписывания**: если сейчас используется `Bind<T>().To<TImpl>().AsSingle()`, эта строчка работает в Zenjex без изменений
- **Быстрее Zenject**: Reflex резолвит зависимости быстрее за счёт активации через expression tree и умного кэширования - как на Mono, так и особенно на IL2CPP
- **Unity 6 из коробки**: никаких конфликтов с редактором, никакого сломанного окна дебаггера, никаких скрытых рантайм-крэшей из-за несовместимости версий
- **Без переобучения**: три паттерна инъекций (атрибут, базовый класс, ручной резолв) знакомы каждому, кто работал с Zenject или Extenject

---

### Интеграция

#### Шаг 1 - Скопируйте папку Zenjex в проект

Перенесите папку `Zenjex` целиком в `Assets`. Всё - никакого пакетного менеджера, никаких git-сабмодулей. В папке лежат Reflex и Zenjex extension layer, каждый со своим `.asmdef`, Unity подхватит их автоматически.

> **Версия Unity**: включённая сборка Reflex нацелена на Unity 6.3 LTS. На более старых версиях без отката Unity 6-специфичных правок она работать корректно не будет.

#### Шаг 2 - Создайте класс-наследник `ProjectRootInstaller`

`ProjectRootInstaller` - глобальный composition root, аналог `ProjectContext + MonoInstaller` из Zenject. Нужно создать один конкретный подкласс, повесить его как компонент на **персистентный GameObject** в загрузочной сцене и реализовать три абстрактных метода.

```csharp
using System.Collections;
using Reflex.Core;
using Zenjex.Extensions.Core;

[DefaultExecutionOrder(-250)]
public class GameInstaller : ProjectRootInstaller
{
    public override void InstallBindings(ContainerBuilder builder)
    {
        // Регистрация всех глобальных сервисов - см. Шаг 4
    }

    // Асинхронная настройка между InstallBindings и LaunchGame.
    // Сюда идут загрузка Addressables, рантайм-биндинги и т.д.
    // Если не нужно - просто оставьте yield return null.
    public override IEnumerator InstallGameInstanceRoutine()
    {
        yield return null;
    }

    // Вызывается после InstallGameInstanceRoutine() и инициализации всех IInitializable.
    // Отсюда стартует точка входа игры - например, запуск StateMachine,
    // загрузка первой сцены или запуск GameInstance.
    // Если точка входа реализована через IInitializable, оставьте пустым.
    public override void LaunchGame()
    {
        // _gameInstance.Launch();
    }
}
```

`ProjectRootInstaller` запускается с порядком исполнения `-280`, что гарантирует: контейнер полностью собран до того, как сработает чей-либо `Awake()` в сцене.

Для крупных проектов удобно разбивать биндинги на несколько дочерних `MonoInstaller`-компонентов и делегировать им из корневого инсталлера:

```csharp
public class AppInstaller : ProjectRootInstaller
{
    [SerializeField] private InfrastructureInstaller _infra;
    [SerializeField] private GameplayInstaller       _gameplay;

    public override void InstallBindings(ContainerBuilder builder)
    {
        _infra.InstallBindings(builder);
        _gameplay.InstallBindings(builder);
    }

    public override IEnumerator InstallGameInstanceRoutine()
    {
        yield return _infra.LoadAddressablesRoutine();
    }

    public override void LaunchGame() { }
}
```

#### Шаг 3 - Добавьте `SceneInstaller` в каждую геймплейную сцену

Для сцен с локальными сервисами создайте класс-наследник `SceneInstaller` и добавьте его как компонент на любой GameObject в этой сцене. `SceneInstaller` создаёт **дочерний контейнер**, который наследует все глобальные биндинги из `RootContainer` и дополняет их локальными для данной сцены.

Дочерний контейнер автоматически диспозится при выгрузке сцены. Все локальные `IInitializable`- и `IDisposable`-сервисы управляются инсталлером.

`SceneInstaller` запускается с порядком исполнения `-200` - после `ProjectRootInstaller` (`-280`), но до любого обычного `Awake()` (`0`).

```csharp
using Reflex.Core;
using Zenjex.Extensions.SceneContext;

public class GameplaySceneInstaller : SceneInstaller
{
    [SerializeField] private LevelProgressWatcher _levelProgressWatcher;

    public override void InstallBindings(ContainerBuilder builder)
    {
        // Локальные биндинги - видны только внутри контейнера этой сцены
        builder.BindInstance(_levelProgressWatcher).AsSingle();

        // Scoped-биндинг из глобального инсталлера получит здесь новый экземпляр,
        // у которого есть доступ как к глобальным сервисам, так и к локальному watcher'у выше
        builder.Bind<LevelProgressServiceResolver>()
               .BindInterfacesAndSelf()
               .CopyIntoDirectSubContainers()
               .NonLazy();
    }

    // Опционально: вызывается один раз после того, как контейнер сцены готов
    protected override void OnInstalled() { }
}
```

Чтобы получить что-то из контейнера активной сцены из любого места в коде, используйте `ZenjexSceneContext`:

```csharp
// Из любого глобального сервиса или состояния GSM:
var sceneContainer = ZenjexSceneContext.GetActive();
var watcher = sceneContainer.Resolve<LevelProgressWatcher>();

// Или напрямую:
ZenjexSceneContext.Resolve<LevelProgressWatcher>();
```

`ZenjexSceneContext.HasActiveScene` позволяет проверить, зарегистрирован ли сейчас контейнер сцены, прежде чем обращаться к нему.

> Сценам без локальных сервисов `SceneInstaller` не нужен - глобальные биндинги из `RootContainer` всегда доступны через `RootContext.Resolve<T>()`.

#### Шаг 4 - Зарегистрируйте зависимости в `InstallBindings`

Используйте Zenject-подобный fluent API для регистрации всего, что нужно контейнеру.

**Базовые биндинги**

```csharp
public override void InstallBindings(ContainerBuilder builder)
{
    // Интерфейс к реализации, ленивый синглтон
    builder.Bind<IInputService>().To<InputService>().AsSingle();

    // Конкретный тип напрямую
    builder.Bind<AnalyticsManager>().AsSingle();

    // Готовый экземпляр (всегда регистрируется как синглтон)
    builder.Bind<IConfig>().FromInstance(myConfigAsset).AsSingle();

    // Сокращённая запись для того же самого
    builder.BindInstance(myConfigAsset).AsSingle();

    // Зарегистрировать под всеми интерфейсами и под самим типом
    builder.Bind<PlayerController>().BindInterfacesAndSelf().AsSingle();

    // Только под интерфейсами, без конкретного типа
    builder.Bind<AudioService>().BindInterfaces().AsSingle();

    // Transient - новый экземпляр на каждый резолв
    builder.Bind<IEnemyFactory>().To<EnemyFactory>().AsTransient();

    // Eager singleton - создаётся сразу при сборке контейнера
    // В Zenject: .AsSingle().NonLazy()  ->  в Zenjex: .NonLazy()
    builder.Bind<IGameStateMachine>().To<GameStateMachine>().BindInterfacesAndSelf().NonLazy();
}
```

**Биндинги на основе префаба**

```csharp
// Инстанциировать префаб при первом резолве и достать из него компонент.
// Всегда singleton + lazy - префаб спавнится один раз, при первом обращении.
builder.Bind<ICurtainService>()
       .To<CurtainService>()
       .FromComponentInNewPrefab(curtainPrefab)
       .WithGameObjectName("Curtain")         // переименовывает корневой GameObject
       .UnderTransformGroup("Infrastructure") // помещает под именованную группу в корне сцены
       .BindInterfacesAndSelf()
       .NonLazy();

// Или разместить под конкретным Transform вместо именованной группы:
builder.Bind<IHUDService>()
       .To<HUDService>()
       .FromComponentInNewPrefab(hudPrefab)
       .UnderTransform(uiRootTransform)
       .AsSingle();

// Найти существующий компонент в иерархии сцены:
builder.Bind<IPlayerCamera>()
       .To<PlayerCamera>()
       .FromComponentInHierarchy()  // includeInactive: true по умолчанию
       .AsSingle();
```

**Аргументы конструктора**

```csharp
// Смешать рантайм-объекты с сервисами из контейнера.
// Переданные значения сопоставляются с параметрами конструктора по типу.
// Остальные параметры резолвятся из контейнера как обычно.
var playerInput = Instantiate(playerInputPrefab).GetComponent<PlayerInput>();

builder.Bind<IInputService>()
       .To<InputService>()
       .WithArguments(playerInput, cinemachineInputProvider)
       .AsSingle();

// Конструктор InputService:
// public InputService(PlayerInput pi, CinemachineInputProvider cip, ILoggingService log)
// pi, cip <- WithArguments  |  log <- контейнер
```

**Scoped-биндинги**

```csharp
// Scoped: новый экземпляр в каждом дочернем контейнере SceneInstaller'а.
// Scoped-объект получает в конструкторе как глобальные, так и локальные зависимости.
builder.Bind<LevelProgressServiceResolver>()
       .BindInterfacesAndSelf()
       .CopyIntoDirectSubContainers()
       .NonLazy();

// Эквивалентная сокращённая запись:
builder.Bind<LevelProgressServiceResolver>()
       .BindInterfacesAndSelf()
       .AsScoped();
```

---

### Жизненный цикл

`ProjectRootInstaller` гарантирует следующий порядок исполнения при каждом старте игры:

1. **`InstallBindings(builder)`** - регистрация всех глобальных сервисов. Выполняется синхронно внутри `Awake()` с порядком `-280`.
2. **`OnContainerReady`** - первый проход инъекций `[Zenjex]` по всем загруженным сценам. Поскольку это происходит внутри `Awake()` с порядком `-280`, все последующие `Awake()` в сцене уже видят заполненные поля.
3. **`InstallGameInstanceRoutine()`** - асинхронная настройка: загрузка Addressables, рантайм-биндинги и т.д.
4. **`IInitializable.Initialize()`** - автоматически вызывается на каждом сервисе, реализующем `IInitializable` и зарегистрированном под этим интерфейсом. Выполняется после шага 3, до `LaunchGame()`.
5. **`LaunchGame()`** - точка входа в игру.
6. **`OnGameLaunched`** - второй проход инъекций `[Zenjex]`, покрывающий объекты, зависящие от биндингов, добавленных на шаге 3.

---

### IInitializable

Реализуйте `IInitializable` на любом сервисе, чтобы гарантированно получить вызов `Initialize()` после того, как контейнер полностью собран и все зависимости заинжекчены - но до вызова `LaunchGame()`. Это прямая замена `IInitializable` из Zenject.

Чтобы сервис был обнаружен автоматически, он **обязательно** должен быть зарегистрирован под контрактом `IInitializable` - используйте `BindInterfaces()` или `BindInterfacesAndSelf()` в инсталлере.

```csharp
using Zenjex.Extensions.Lifecycle;

public class GameStateMachine : IInitializable
{
    private readonly StateFactory _stateFactory;

    public GameStateMachine(StateFactory stateFactory) =>
        _stateFactory = stateFactory;

    public void Initialize() => Enter<BootstrapState>();
}

// В инсталлере:
builder.Bind<GameStateMachine>()
       .BindInterfacesAndSelf()
       .AsEagerSingleton();
```

`IInitializable` работает как в глобальном контейнере (`ProjectRootInstaller`), так и в контейнерах уровня сцены (`SceneInstaller`). Локальные инициализируемые сервисы вызываются сразу после сборки контейнера сцены, до `OnInstalled()`.

---

### Биндинг после инициализации

Иногда нужно зарегистрировать что-то в контейнере уже после его сборки - например, конфиг, загруженный в рантайме, или сервис, созданный в `InstallGameInstanceRoutine`. Для этого используйте `RootContext.Runtime`:

```csharp
// Из любого места, после сборки контейнера:
RootContext.Runtime
    .Bind<IRuntimeConfig>()
    .FromInstance(loadedConfig)
    .AsSingle();
```

`RootContext.Runtime` даёт прямой доступ к живому контейнеру. `RootContext.HasInstance` позволяет безопасно проверить, существует ли он уже, перед вызовом.

---

### Инъекции зависимостей

Zenjex поддерживает три паттерна инъекций. Выбирайте тот, что подходит к ситуации.

#### 1. Прямой резолв - `RootContext.Resolve<T>()`

Работает в любом месте и в любое время после сборки контейнера. Не нужен ни базовый класс, ни атрибуты. Просто вызвать и использовать.

```csharp
private void Awake()
{
    var config = RootContext.Resolve<IGameConfig>();
    var input  = RootContext.Resolve<IInputService>();
}
```

Подходит для: контроллеров, менеджеров и любых классов, где нужен явный и легко отслеживаемый захват зависимости.

#### 2. Инъекция атрибутом на обычном `MonoBehaviour` - `[Zenjex]`

Пометьте поля, свойства или inject-методы атрибутом `[Zenjex]`. **Объект должен присутствовать в сцене** на момент загрузки загрузочной сцены. Инъекция происходит в `ProjectRootInstaller.Awake()`, до любого другого `Awake()` в сцене - к тому моменту, как сработает ваш `Awake()`, поля уже заполнены.

```csharp
using Zenjex.Extensions.Attribute;

public class HUDController : MonoBehaviour
{
    [Zenjex] private IPlayerService _player;
    [Zenjex] private IAudioService _audio;

    private void Awake()
    {
        // _player и _audio уже заинжекчены
        _player.OnHealthChanged += UpdateHealthBar;
    }
}
```

> Если объект находится в **аддитивно загруженной сцене**, инъекция происходит после загрузки этой сцены - то есть уже после того, как `Awake()` успел отработать. В таком случае поля в `Awake()` будут `null`. Zenjex выведет предупреждение `ZNX-LATE`, чтобы это было заметно. Используйте паттерн №3 ниже, если нужна гарантированная инъекция до `Awake()` в динамически загружаемых объектах.

#### 3. `ZenjexBehaviour` - гарантированная инъекция до `Awake()`, работает для рантайм-инстанциированных объектов

Унаследуйтесь от `ZenjexBehaviour` вместо `MonoBehaviour`. Это даёт объекту собственный `Awake()` с порядком `-100`, что гарантирует инъекцию до любого пользовательского `Awake()` - даже для префабов, созданных через `Instantiate()` в рантайме.

Вместо `Awake()` переопределяйте `OnAwake()`. К моменту его вызова поля уже заполнены.

```csharp
using Zenjex.Extensions.Attribute;
using Zenjex.Extensions.Injector;

public class Enemy : ZenjexBehaviour
{
    [Zenjex] private IEnemyConfig _config;
    [Zenjex] private IAudioService _audio;

    protected override void OnAwake()
    {
        base.OnAwake(); // всегда вызывайте в первую очередь

        // _config и _audio уже заинжекчены
        _audio.Play(_config.SpawnSound);
    }
}
```

#### 4. Ручная инъекция - `ZenjexRunner.InjectGameObject()`

Для объектов, созданных в рантайме через `Instantiate()`, которые не наследуются от `ZenjexBehaviour`, вызовите `InjectGameObject` сразу после инстанциирования. Метод обходит всю иерархию и инжектит все `[Zenjex]`-помеченные члены.

```csharp
var enemy = Instantiate(enemyPrefab);
ZenjexRunner.InjectGameObject(enemy);
```

> Это нужно только для обычных `MonoBehaviour`-подклассов. `ZenjexBehaviour` обрабатывает себя автоматически.

---

### Порядок и гарантии инъекций

| Паттерн | Объект должен быть в сцене при загрузке? | Поля заполнены в `Awake()`? |
|---|---|---|
| `RootContext.Resolve<T>()` | Нет | Да (контроль на вашей стороне) |
| `[Zenjex]` на `MonoBehaviour` (загрузочная сцена) | Да | Да |
| `[Zenjex]` на `MonoBehaviour` (аддитивная сцена) | Да | **Нет** - предупреждение ZNX-LATE |
| `ZenjexBehaviour` + `[Zenjex]` | Нет | Да |
| `ZenjexRunner.InjectGameObject()` | Нет | После ручного вызова |
| `SceneInstaller` (наследник `ZenjexBehaviour`) | Нет | Да |

---

### Примеры проектов

Включённый **DevConsole** - полная рабочая реализация: есть `GameInstaller`, несколько сервисов, забинденных через интерфейс, `[Zenjex]`-поля на объектах сцены и `RootContext.Resolve<T>()` в контроллерах. Это самый быстрый способ увидеть всё в контексте.

Для более масштабного примера - **[LoneBrawler](https://github.com/antonprv/LoneBrawler)**, полноценная мидкор-игра для браузера и мобильных, собранная на этом фреймворке. Показывает, как Zenjex держится в настоящей продакшн-кодовой базе: несколько сцен, сложные графы сервисов, реальные геймплейные системы.

Если интересно понять, как фреймворк устроен внутри - как выстроена иерархия контейнеров, как расставлены по времени injection-пассы, как работает активация через expression tree - **[вики проекта](../../wiki)** содержит полный разбор архитектуры Reflex и слоя Zenjex.

---

### Требования

- **Unity 6.3+** - протестировано на Unity 6.3 LTS, совместимо со всеми версиями Unity начиная с 6.3

---

*Создано Антоном Пируевым, 2026. Любое прямое коммерческое использование производных работ строго запрещено.*
