using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Microsoft.Xna.Framework.Input;

namespace Mivora.Input;

public enum GameAction
{
    MoveForward,
    MoveBackward,
    MoveLeft,
    MoveRight,
    Jump,
    Sprint,
    Chat,
    DebugMenu,
    Fullscreen,
    TogglePerson,
    Pause,
}

public class KeybindManager
{
    public static KeybindManager Instance { get; } = new();

    private static readonly string SavePath = "keybinds.json";

    private Dictionary<GameAction, Keys> _binds = new()
    {
        { GameAction.MoveForward,  Keys.W },
        { GameAction.MoveBackward, Keys.S },
        { GameAction.MoveLeft,     Keys.A },
        { GameAction.MoveRight,    Keys.D },
        { GameAction.Jump,         Keys.Space },
        { GameAction.Sprint,       Keys.LeftShift },
        { GameAction.Chat,         Keys.T },
        { GameAction.DebugMenu,    Keys.F3 },
        { GameAction.Fullscreen,   Keys.F11 },
        { GameAction.TogglePerson, Keys.V },
        { GameAction.Pause,        Keys.Escape },
    };

    public static readonly Dictionary<GameAction, string> ActionNames = new()
    {
        { GameAction.MoveForward,  "Move Forward" },
        { GameAction.MoveBackward, "Move Backward" },
        { GameAction.MoveLeft,     "Move Left" },
        { GameAction.MoveRight,    "Move Right" },
        { GameAction.Jump,         "Jump" },
        { GameAction.Sprint,       "Sprint" },
        { GameAction.Chat,         "Chat" },
        { GameAction.DebugMenu,    "Debug Menu" },
        { GameAction.Fullscreen,   "Fullscreen" },
        { GameAction.TogglePerson, "Toggle Person" },
        { GameAction.Pause,        "Pause" },
    };

    public Keys Get(GameAction action) => _binds[action];
    public void Set(GameAction action, Keys key) => _binds[action] = key;

    public bool IsConflict(GameAction action, Keys key)
    {
        foreach (var (a, k) in _binds)
            if (a != action && k == key) return true;
        return false;
    }

    public void ResetToDefaults()
    {
        _binds = new()
        {
            { GameAction.MoveForward,  Keys.W },
            { GameAction.MoveBackward, Keys.S },
            { GameAction.MoveLeft,     Keys.A },
            { GameAction.MoveRight,    Keys.D },
            { GameAction.Jump,         Keys.Space },
            { GameAction.Sprint,       Keys.LeftShift },
            { GameAction.Chat,         Keys.T },
            { GameAction.DebugMenu,    Keys.F3 },
            { GameAction.Fullscreen,   Keys.F11 },
            { GameAction.TogglePerson, Keys.V },
            { GameAction.Pause,        Keys.Escape },
        };
    }

    public void Save()
    {
        var dict = new Dictionary<string, string>();
        foreach (var (action, key) in _binds)
            dict[action.ToString()] = key.ToString();
        var json = JsonSerializer.Serialize(dict, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(SavePath, json);
    }

    public void Load()
    {
        if (!File.Exists(SavePath)) return;
        try
        {
            var json = File.ReadAllText(SavePath);
            var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
            if (dict == null) return;
            foreach (var (actionStr, keyStr) in dict)
                if (Enum.TryParse<GameAction>(actionStr, out var action) &&
                    Enum.TryParse<Keys>(keyStr, out var key))
                    _binds[action] = key;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Keybinds] Failed to load: {ex.Message}");
        }
    }
}
