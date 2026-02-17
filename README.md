# Zenjex — Zenject-like API for Reflex

> **Familiar Zenject syntax. Modern Reflex performance. Works with the latest Unity versions.**

---

## 🇬🇧 English

### The Problem

Zenject is widely praised in the Unity community — but it hasn't kept up with modern Unity versions. Projects that rely on it face compatibility issues, abandoned support, and a framework that simply doesn't move forward.

**Reflex** is the answer: it's the most actively maintained and performant DI framework for Unity today. But switching from Zenject means rewriting your entire installer layer and retraining your team.

### The Solution

**Zenjex** is a thin extension layer on top of [Reflex 14.1.0](https://github.com/gustavopsantos/reflex) that brings a Zenject-familiar API to Reflex's modern engine. You keep the syntax your team already knows. You get all the benefits of Reflex under the hood.

On top of that, Zenjex solves a real Reflex limitation: **you can now add bindings to a container even after it has already been built** — a capability the base Reflex framework does not provide.

---

### Features

- **Zenject-style API** — `Bind<T>().To<TImpl>().AsSingle()` works exactly as you'd expect
- **Post-build container registration** — inject new bindings into an existing `Container` instance via `container.Bind<T>().FromInstance(...).AsSingle()`
- **`BindInterfaces()` / `BindInterfacesAndSelf()`** — automatic interface resolution, same as Zenject
- **`AsSingle()` / `AsTransient()` / `AsScoped()` / `AsEagerSingleton()`** — full lifetime control
- **`ProjectRootInstaller`** — a MonoBehaviour base class for global DI setup with lifecycle hooks
- **`RootContext`** — a static access point for resolving from the root container (for GameInstance-style architectures)
- **Built on Reflex 14.1.0** — full IL2CPP support, source generators, scoped containers

---

### Project Structure

```
src/
├── Reflex/              ← Reflex 14.1.0 + modifications to container script.
└── ReflexExtensions/    ← Zenjex extension layer
    ├── BindingBuilder.cs              ← Fluent API for ContainerBuilder (setup phase)
    ├── ContainerBindingBuilder.cs     ← Fluent API for Container (post-build registration)
    ├── ReflexZenjectExtensions.cs     ← Bind<T>() extension on ContainerBuilder
    ├── ContainerZenjectExtensions.cs  ← Bind<T>() extension on built Container
    ├── ProjectRootInstaller.cs        ← Base MonoBehaviour for global DI
    └── RootContext.cs                 ← Static resolver for GameInstance pattern
```

---

### Installation

1. Copy the `Reflex` folder into your Unity project
2. Copy the `ReflexExtensions` folder anywhere in your project

Then follow the standard Reflex setup from the [official Reflex repository](https://github.com/gustavopsantos/reflex) (create a `ProjectScope`, configure scene scopes, etc).

> **Note:** The TreeView debugger window has a known upstream bug in Reflex — the editor debug panel may behave incorrectly. This is a Reflex issue, not a Zenjex one.

---

### Usage

#### 1. Setting up bindings (ContainerBuilder)

```csharp
public class GameInstaller : ProjectRootInstaller
{
    public override void InstallBindings(ContainerBuilder builder)
    {
        // Bind interface to implementation, singleton
        builder.Bind<ISceneLoader>().To<SceneLoader>().AsSingle();

        // Bind with all interfaces of the concrete type
        builder.Bind<PlayerProvider>().BindInterfaces().AsSingle();

        // Bind with interfaces AND the concrete type itself
        builder.Bind<PlayerProvider>().BindInterfacesAndSelf().AsSingle();

        // Transient (new instance on each resolve)
        builder.Bind<IEnemyFactory>().To<EnemyFactory>().AsTransient();

        // Eager singleton (instantiated immediately at build time)
        builder.Bind<IEventBus>().To<EventBus>().AsEagerSingleton();

        // From existing instance
        builder.Bind<ICoroutineRunner>().FromInstance(_myMonoBehaviour).AsSingle();

        // Platform-based conditional binding
        if (Application.platform != RuntimePlatform.Android)
            builder.Bind<IInputService>().To<PCInputService>().AsSingle();
        else
            builder.Bind<IInputService>().To<PhoneInputService>().AsSingle();
    }
}
```

#### 2. Post-build registration (on existing Container)

This is unique to Zenjex — Reflex doesn't support this natively.

```csharp
// GameInstance is created asynchronously AFTER the container is built
public override IEnumerator InstallGameInstanceRoutine()
{
    yield return InstallerFactory.CreateGameInstanceRoutine(instance =>
        _gameInstance = instance);

    // Add GameInstance to the already-built container
    RootContainer.Bind<GameInstance>()
        .FromInstance(_gameInstance)
        .BindInterfacesAndSelf()
        .AsSingle();
}
```

#### 3. ProjectRootInstaller

```csharp
public class GameInstaller : ProjectRootInstaller
{
    private GameInstance _gameInstance;

    // Step 1: Register all services into ContainerBuilder
    public override void InstallBindings(ContainerBuilder builder) { ... }

    // Step 2: Async routine — create late objects, add them to the built container
    public override IEnumerator InstallGameInstanceRoutine()
    {
        yield return InstallerFactory.CreateGameInstanceRoutine(i => _gameInstance = i);
        RootContainer.Bind<GameInstance>().FromInstance(_gameInstance).BindInterfacesAndSelf().AsSingle();
    }

    // Step 3: All bindings done — start the game
    public override void LaunchGame() => _gameInstance.LaunchGame();
}
```

#### 4. RootContext — resolving without injection

For cases where a class cannot receive dependencies through a constructor or `[Inject]` (e.g. a GameInstance singleton that needs services after DI is complete):

```csharp
private void ResolveDependencies()
{
    _staticData = RootContext.Resolve<IStaticDataService>();
}

// Guard check:
if (RootContext.HasInstance)
    var service = RootContext.Resolve<IMyService>();
```

---

### Binding Lifetime Reference

| Method | Lifetime | Notes |
|---|---|---|
| `AsSingle()` | Singleton | Alias for `AsSingleton()` |
| `AsSingleton()` | Singleton | One instance for the container's lifetime |
| `AsTransient()` | Transient | New instance on every resolve |
| `AsScoped()` | Scoped | One instance per scope |
| `AsEagerSingleton()` | Singleton (Eager) | Instantiated immediately when the container is built |

---

### Key Differences from Pure Reflex

| Feature | Pure Reflex | Zenjex |
|---|---|---|
| Fluent binding API | `builder.AddSingleton<T>()` | `builder.Bind<T>().To<TImpl>().AsSingle()` |
| Post-build registration | ❌ Not supported | ✅ `container.Bind<T>().FromInstance(x).AsSingle()` |
| Interface auto-binding | Manual | `BindInterfaces()` / `BindInterfacesAndSelf()` |
| GameInstance pattern | Requires custom setup | Built-in via `ProjectRootInstaller` + `RootContext` |

---

### Requirements

- Unity 2022.3+ (LTS) or newer
- Reflex 14.1.0 (included)
- .NET Standard 2.1

---

### License

© 2026 Anton Piruev. Any direct commercial use of derivative work is strictly prohibited. See [LICENSE](./LICENSE).

---
---

## 🇷🇺 Русский

### Проблема

Zenject высоко ценится среди сообщества Unity-разработчиков, однако он перестал поддерживать современные версии Unity. Проекты, зависящие от Zenject, сталкиваются с проблемами совместимости, отсутствием поддержки и устаревшим фреймворком, который больше не развивается.

Решение - **Reflex**: это наиболее активно поддерживаемый и производительный фреймворк внедрения зависимостей (DI) для Unity на сегодняшний день. Однако переход с Zenject означает переписывание всего слоя инсталлятора и переобучение команды.

### Решение

**Zenjex** - тонкий слой расширения поверх [Reflex 14.1.0](https://github.com/gustavopsantos/reflex), который привносит знакомый API Zenject в современный движок Reflex. Вы сохраняете синтаксис, известный вашей команде, и получаете все преимущества Reflex под капотом. Кроме того, Zenjex решает реальную проблему ограничения Reflex: теперь вы можете добавлять привязки в контейнер даже после того, как он уже построен - способность, которую базовая версия Reflex не предоставляет.

---

### Что внутри

- **Zenject-style API** — `Bind<T>().To<TImpl>().AsSingle()` работает ровно так, как вы к этому привыкли
- **Регистрация после `Build()`** — добавляйте зависимости в уже готовый `Container` через `container.Bind<T>().FromInstance(...).AsSingle()`
- **`BindInterfaces()` / `BindInterfacesAndSelf()`** — автоматическая привязка по интерфейсам, как в Zenject
- **`AsSingle()` / `AsTransient()` / `AsScoped()` / `AsEagerSingleton()`** — полный контроль над временем жизни объекта
- **`ProjectRootInstaller`** — базовый `MonoBehaviour` для глобальной настройки DI с хуками жизненного цикла
- **`RootContext`** — статический доступ к корневому контейнеру для архитектур с GameInstance-синглтоном
- **Основан на Reflex 14.1.0** — полная поддержка IL2CPP, source generators, scoped-контейнеры

---

### Структура проекта

```
src/
├── Reflex/              ← Reflex 14.1.0 + модифицированный Container.cs
└── ReflexExtensions/    ← расширения Zenjex
    ├── BindingBuilder.cs              ← Fluent API для ContainerBuilder (фаза сборки)
    ├── ContainerBindingBuilder.cs     ← Fluent API для уже собранного Container
    ├── ReflexZenjectExtensions.cs     ← Bind<T>() как метод расширения на ContainerBuilder
    ├── ContainerZenjectExtensions.cs  ← Bind<T>() как метод расширения на готовом Container
    ├── ProjectRootInstaller.cs        ← Базовый MonoBehaviour для глобального DI
    └── RootContext.cs                 ← Статический доступ к корневому контейнеру
```

---

### Установка

1. Скопируйте папку `Reflex` в ваш Unity-проект
2. Скопируйте папку `ReflexExtensions` в любое удобное место в проекте

Дальнейшая настройка — стандартная для Reflex, смотрите [официальный репозиторий](https://github.com/gustavopsantos/reflex) (создайте `ProjectScope`, настройте scene scopes и т.д.).

> **Известный баг:** окно дебаггера с TreeView в Reflex недоработано автором фреймворка — редакторская панель отладки может вести себя некорректно. Это проблема Reflex, а не Zenjex.

---

### Использование

#### 1. Регистрация зависимостей (ContainerBuilder)

```csharp
public class GameInstaller : ProjectRootInstaller
{
    public override void InstallBindings(ContainerBuilder builder)
    {
        // Интерфейс → реализация, синглтон
        builder.Bind<ISceneLoader>().To<SceneLoader>().AsSingle();

        // Зарегистрировать по всем интерфейсам конкретного типа
        builder.Bind<PlayerProvider>().BindInterfaces().AsSingle();

        // Зарегистрировать по интерфейсам и по самому типу
        builder.Bind<PlayerProvider>().BindInterfacesAndSelf().AsSingle();

        // Transient — новый объект при каждом запросе
        builder.Bind<IEnemyFactory>().To<EnemyFactory>().AsTransient();

        // Eager singleton — создаётся сразу при Build(), не по запросу
        builder.Bind<IEventBus>().To<EventBus>().AsEagerSingleton();

        // Из готового объекта
        builder.Bind<ICoroutineRunner>().FromInstance(_myMonoBehaviour).AsSingle();

        // Условная регистрация по платформе
        if (Application.platform != RuntimePlatform.Android)
            builder.Bind<IInputService>().To<PCInputService>().AsSingle();
        else
            builder.Bind<IInputService>().To<PhoneInputService>().AsSingle();
    }
}
```

#### 2. Регистрация в уже собранном контейнере

Это уникальная возможность Zenjex — в чистом Reflex так сделать нельзя.

```csharp
// GameInstance создаётся асинхронно, уже ПОСЛЕ того как контейнер собран
public override IEnumerator InstallGameInstanceRoutine()
{
    yield return InstallerFactory.CreateGameInstanceRoutine(instance =>
        _gameInstance = instance);

    // Добавляем GameInstance в уже готовый контейнер
    RootContainer.Bind<GameInstance>()
        .FromInstance(_gameInstance)
        .BindInterfacesAndSelf()
        .AsSingle();
}
```

#### 3. ProjectRootInstaller

```csharp
public class GameInstaller : ProjectRootInstaller
{
    private GameInstance _gameInstance;

    // Шаг 1: регистрируем все сервисы
    public override void InstallBindings(ContainerBuilder builder) { ... }

    // Шаг 2: создаём объекты, которые появляются позже, и добавляем их в контейнер
    public override IEnumerator InstallGameInstanceRoutine()
    {
        yield return InstallerFactory.CreateGameInstanceRoutine(i => _gameInstance = i);
        RootContainer.Bind<GameInstance>().FromInstance(_gameInstance).BindInterfacesAndSelf().AsSingle();
    }

    // Шаг 3: всё готово, запускаем игру
    public override void LaunchGame() => _gameInstance.LaunchGame();
}
```

#### 4. RootContext — получить зависимость без инъекции

Бывают случаи, когда класс не может получить зависимость через конструктор или `[Inject]` — например, GameInstance-синглтон, которому нужны сервисы уже после того как DI завершился:

```csharp
private void ResolveDependencies()
{
    _staticData = RootContext.Resolve<IStaticDataService>();
}

// Проверка перед использованием:
if (RootContext.HasInstance)
    var service = RootContext.Resolve<IMyService>();
```

---

### Время жизни объектов

| Метод | Время жизни | Примечание |
|---|---|---|
| `AsSingle()` | Singleton | Псевдоним для `AsSingleton()` |
| `AsSingleton()` | Singleton | Один объект на весь контейнер |
| `AsTransient()` | Transient | Новый объект при каждом запросе |
| `AsScoped()` | Scoped | Один объект на scope |
| `AsEagerSingleton()` | Singleton (Eager) | Создаётся сразу при `Build()`, не по запросу |

---

### Отличия от чистого Reflex

| Возможность | Чистый Reflex | Zenjex |
|---|---|---|
| Fluent API регистрации | `builder.AddSingleton<T>()` | `builder.Bind<T>().To<TImpl>().AsSingle()` |
| Регистрация после `Build()` | ❌ Недоступно | ✅ `container.Bind<T>().FromInstance(x).AsSingle()` |
| Автопривязка по интерфейсам | Вручную | `BindInterfaces()` / `BindInterfacesAndSelf()` |
| Паттерн GameInstance | Нужно реализовывать самому | Готово: `ProjectRootInstaller` + `RootContext` |

---

### Требования

- Unity 2022.3+ (LTS) или новее
- Reflex 14.1.0 (включён в репозиторий)
- .NET Standard 2.1

---

### Лицензия

© 2026 Anton Piruev. Прямое коммерческое использование производных работ строго запрещено. См. [LICENSE](./LICENSE).
