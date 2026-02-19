// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using DevConsole.Backend.Infrastructure.Services.DevConsole;
using DevConsole.Backend.Infrastructure.Services.DevConsole.Interfaces;
using DevConsole.Backend.Infrastructure.Services.StaticDataService.Interfaces.Subservice;
using Code.Infrastructure.Services.DevConsole.Types;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenjex.Extensions.Core;
using DevConsole.Backend.Infrastructure.Services.DevConsole.Commands;

namespace DevConsole.Backend.Infrastructure.Services.DevConsole
{
  public class DevConsoleService : IDevConsole
  {
    public string ConsoleMarker => "[Console] ";

    private readonly Dictionary<string, IConsoleCommand> _commands;
    private readonly List<ConsoleMessage> _messages;
    private const int _maxMessages = 500;
    private bool _captureUnityLogs;
    private ConsoleMessageType _logFilter;
    private bool _initialized;

    public bool IsEnabled { get; private set; }

    public DevConsoleService()
    {
      _commands = new Dictionary<string, IConsoleCommand>();
      _messages = new List<ConsoleMessage>();
      IsEnabled = false;
      _captureUnityLogs = true;
      _logFilter = ConsoleMessageType.All; // Show all by default
      _initialized = false;
    }

    public void Initialize()
    {
      if (_initialized)
        return;

      _initialized = true;


      if (CanUseConsole())
      {
        RegisterCommand(new HelpCommand(this, _commands));
        AddMessage("Developer Console initialized. Type 'help' for commands.", ConsoleMessageType.Log);
        SubscribeToUnityLogs();
      }
    }

    ~DevConsoleService()
    {
      UnsubscribeFromUnityLogs();
    }

    public void RegisterCommand(IConsoleCommand command)
    {
      if (!CanUseConsole())
        return;

      string commandName = command.CommandName.ToLower();

      if (_commands.ContainsKey(commandName))
      {
        AddMessage($"Command '{commandName}' is already registered!", ConsoleMessageType.Warning);
        return;
      }

      _commands.Add(commandName, command);
      // Don't log command registration to avoid spam
    }

    public void Toggle()
    {
      if (!CanUseConsole())
      {
        AddMessage("Console is disabled in Shipping builds", ConsoleMessageType.Warning);
        return;
      }

      IsEnabled = !IsEnabled;
    }

    public void ExecuteCommand(string commandLine)
    {
      if (!CanUseConsole())
        return;

      if (string.IsNullOrWhiteSpace(commandLine))
        return;

      // Add command to history
      AddMessage($"> {commandLine}", ConsoleMessageType.Command);

      string[] parts = commandLine.Split(' ');
      string commandName = parts[0].ToLower();
      string[] args = parts.Skip(1).ToArray();

      if (_commands.TryGetValue(commandName, out IConsoleCommand command))
      {
        command.Execute(args);
      }
      else
      {
        AddMessage($"Unknown command: {commandName}", ConsoleMessageType.Error);
        AddMessage($"Type 'help' to see available commands", ConsoleMessageType.Log);
      }
    }

    public void AddMessage(string message, ConsoleMessageType type = ConsoleMessageType.Log)
    {
      _messages.Add(new ConsoleMessage(message, type, this));

      // Limit message history
      if (_messages.Count > _maxMessages)
      {
        _messages.RemoveRange(0, _messages.Count - _maxMessages);
      }
    }

    public string[] GetMessages()
    {
      return _messages
        .Where(m => ShouldShowMessage(m.Type))
        .Select(m => m.FormattedMessage)
        .ToArray();
    }

    public void ClearMessages()
    {
      _messages.Clear();
      AddMessage("Console cleared", ConsoleMessageType.Log);
    }

    public void SetCaptureUnityLogs(bool capture)
    {
      if (_captureUnityLogs == capture)
        return;

      _captureUnityLogs = capture;

      if (capture)
        SubscribeToUnityLogs();
      else
        UnsubscribeFromUnityLogs();

      AddMessage($"Unity log capture {(capture ? "enabled" : "disabled")}", ConsoleMessageType.Log);
    }

    public void SetLogFilter(ConsoleMessageType filter)
    {
      _logFilter = filter;
      AddMessage($"Log filter set to: {filter}", ConsoleMessageType.Success);
    }

    public ConsoleMessageType GetLogFilter()
    {
      return _logFilter;
    }

    private bool ShouldShowMessage(ConsoleMessageType messageType)
    {
      // Always show commands
      if (messageType == ConsoleMessageType.Command)
        return true;

      // Show all if filter is set to All
      if (_logFilter == ConsoleMessageType.All)
        return true;

      // Match specific filter
      return messageType == _logFilter;
    }

    private void SubscribeToUnityLogs()
    {
      Application.logMessageReceived += HandleUnityLog;
    }

    private void UnsubscribeFromUnityLogs()
    {
      Application.logMessageReceived -= HandleUnityLog;
    }

    private void HandleUnityLog(string logString, string stackTrace, LogType type)
    {
      if (!_captureUnityLogs)
        return;

      // Don't capture our own console messages to avoid recursion
      if (logString.StartsWith(ConsoleMarker))
        return;

      ConsoleMessageType messageType = type switch
      {
        LogType.Error => ConsoleMessageType.Error,
        LogType.Assert => ConsoleMessageType.Error,
        LogType.Warning => ConsoleMessageType.Warning,
        LogType.Exception => ConsoleMessageType.Error,
        _ => ConsoleMessageType.UnityLog
      };

      // Format message with timestamp
      string timestamp = System.DateTime.Now.ToString("HH:mm:ss");
      string formattedMessage = $"[{timestamp}] {logString}";

      // Add stack trace for errors and exceptions (collapsed)
      if (type == LogType.Error || type == LogType.Exception)
      {
        if (!string.IsNullOrEmpty(stackTrace))
        {
          // Only add first line of stack trace to save space
          string firstLine = stackTrace.Split('\n')[0];
          formattedMessage += $"\n  → {firstLine}";
        }
      }

      AddMessage(formattedMessage, messageType);
    }

    private bool CanUseConsole() =>
      RootContext.Resolve<IBuildConfigSubservice>().IsDevelopment();
  }
}
