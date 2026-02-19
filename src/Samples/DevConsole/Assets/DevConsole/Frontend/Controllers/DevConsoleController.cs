// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using DevConsole.Backend.Infrastructure.Services.DevConsole.Interfaces;
using DevConsole.Backend.Infrastructure.Services.Input.Interfaces;
using DevConsole.Backend.Infrastructure.Services.StaticDataService.Interfaces.Subservice;
using DevConsole.Frontend.Model;
using DevConsole.Frontend.Services;
using DevConsole.Frontend.View;
using DevConsole.Frontend.ViewModel;
using UnityEngine;
using Zenjex.Extensions.Core;

// Here we're injecting manually in awake and on demand.
namespace DevConsole.Frontend.Controllers
{
  public class DevConsoleController : MonoBehaviour
  {
    [Header("History")]
    [SerializeField] private int _maxHistoryLines = 10;

    [Header("Navigation")]
    [SerializeField] private float _navigationDeadzone = 0.5f;
    [SerializeField] private float _navigationCooldown = 0.2f;

    [Header("Safe Area")]
    [SerializeField] private float _safeAreaLeftOffset = 5f;
    [SerializeField] private float _safeAreaRightOffset = 10f;
    [SerializeField] private bool _useSafeAreas = true;

    [Header("Visual")]
    [SerializeField] private int _outputFontSize = 20;
    [SerializeField] private int _inputFontSize = 18;

    private ConsoleViewModel _viewModel;
    private ConsoleRenderer _renderer;


    private IBuildConfigSubservice _buildConfig;

    private bool _isInitialized;

    private void Awake()
    {
      _buildConfig = RootContext.Resolve<IBuildConfigSubservice>();

      InitializeComponents();

      _isInitialized = true;
    }

    private void Update()
    {
      if (!_isInitialized) return;

      if (!IsDevelopmentBuild())
        return;

      HandleToggle();

      if (_viewModel.IsVisible)
        _viewModel.HandleInput();
    }

    private void OnGUI()
    {
      if (!_isInitialized) return;

      if (!IsDevelopmentBuild() || !_viewModel.IsVisible)
        return;

      string inputText = _viewModel.InputText;
      _renderer.Render(
        _viewModel.Messages,
        ref inputText,
        _viewModel.IsVisible,
        OnSubmitCommand);
      _viewModel.InputText = inputText;
    }

    private void InitializeComponents()
    {
      PlatformService platform = CreatePlatformService();
      ConsoleStyles styles = CreateStyles();

      _viewModel = CreateViewModel(platform);
      _renderer = new ConsoleRenderer(styles, platform);
    }

    private PlatformService CreatePlatformService() =>
      new PlatformService(
        _safeAreaLeftOffset,
        _safeAreaRightOffset,
        _useSafeAreas
        );

    private ConsoleStyles CreateStyles() =>
      new ConsoleStyles(_outputFontSize, _inputFontSize);

    private ConsoleViewModel CreateViewModel(PlatformService platform)
    {
      IDevConsole console = RootContext.Resolve<IDevConsole>();
      IInputService inputService = RootContext.Resolve<IInputService>();

      ConsoleState state = new ConsoleState();
      CommandHistory history = new CommandHistory(_maxHistoryLines);
      MobileKeyboard keyboard = new MobileKeyboard(platform.IsMobile);

      InputService input = new InputService(
        inputService,
        platform.IsMobile,
        _navigationDeadzone,
        _navigationCooldown);

      return new ConsoleViewModel(
        console,
        state,
        history,
        keyboard,
        input,
        platform);
    }

    private void HandleToggle()
    {
      if (_viewModel.IsTogglePressed())
      {
        _viewModel.ToggleConsole();

        if (_viewModel.IsVisible)
          ScrollToBottom();
      }
    }

    private void OnSubmitCommand()
    {
      _viewModel.SubmitCommand();
      ScrollToBottom();
    }

    private void ScrollToBottom()
    {
      _renderer.ScrollToBottom(
        _viewModel.Messages,
        _viewModel.GetViewHeight());
    }

    private bool IsDevelopmentBuild() =>
      _buildConfig.IsDevelopment();
  }
}
