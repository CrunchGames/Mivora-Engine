using System;
using Microsoft.Xna.Framework;

namespace Mivora.UI.Chat;

public class CommandHandler
{
    private ChatSystem _chat;
    private string     _playerName;

    public CommandHandler(ChatSystem chat, string playerName)
    {
        _chat       = chat;
        _playerName = playerName;
    }

    public bool TryHandle(string input)
    {
        if (!input.StartsWith("/")) return false;

        var parts = input[1..].Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0) return true;

        switch (parts[0].ToLower())
        {
            case "help":
                _chat.AddMessage("[Commands]", Color.Yellow);
                _chat.AddMessage("/help - show commands", Color.Yellow);
                _chat.AddMessage("/version - show game version", Color.Yellow);
                _chat.AddMessage("/pos - show your position", Color.Yellow);
                _chat.AddMessage("/clear - clear chat", Color.Yellow);
                _chat.AddMessage("/name <name> - set your name", Color.Yellow);
                _chat.AddMessage("/gamemode <creative|survival> - set gamemode", Color.Yellow);
                break;
            case "version":
                _chat.AddMessage("Mivora 2k09P Beta | Strike Studio INC.", Color.Cyan);
                break;
            case "pos":
                _chat.AddMessage("Use F3 to see your position.", Color.Cyan);
                break;
            case "clear":
                _chat.ClearHistory();
                break;
            case "name":
                if (parts.Length > 1)
                {
                    _playerName = string.Join(" ", parts[1..]);
                    _chat.AddMessage($"Name set to: {_playerName}", Color.Cyan);
                }
                else
                    _chat.AddMessage("Usage: /name <yourname>", Color.Orange);
                break;
            case "gamemode":
                if (parts.Length > 1)
                    _chat.AddMessage($"Gamemode set to: {parts[1]} (coming soon)", Color.Cyan);
                else
                    _chat.AddMessage("Usage: /gamemode <creative|survival>", Color.Orange);
                break;
            default:
                _chat.AddMessage($"Unknown command: /{parts[0]}", Color.OrangeRed);
                break;
        }

        return true;
    }

    public string PlayerName => _playerName;
}
