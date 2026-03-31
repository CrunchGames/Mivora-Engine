using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Mivora.Input;

namespace Mivora.UI.Chat;

public class ChatSystem
{
    private List<ChatMessage> _history      = new();
    private string            _input        = "";
    private bool              _open         = false;
    private int               _scrollOffset = 0;
    private const int         MaxHistory    = 100;
    private const int         VisibleLines  = 8;

    private SpriteFont     _font;
    private SpriteBatch    _sb;
    private Texture2D      _pixel;
    private CommandHandler _commands;

    private UdpClient _udp;
    private Thread    _listenThread;
    private const int Port       = 25566;
    private string    _playerName = "Player";
    private bool      _lanEnabled = false;

    private KeyboardState _prevKey;

    public bool IsOpen => _open;

    public void Initialize(SpriteBatch sb, SpriteFont font,
                           GraphicsDevice gd, string playerName = "Player")
    {
        _sb         = sb;
        _font       = font;
        _playerName = playerName;
        _commands   = new CommandHandler(this, _playerName);

        _pixel = new Texture2D(gd, 1, 1);
        _pixel.SetData(new[] { Color.White });

        TryStartLAN();
        AddMessage("Chat ready. Press T or Enter to chat. Type /help for commands.", Color.Gray);
    }

    private void TryStartLAN()
    {
        try
        {
            _udp = new UdpClient(Port);
            _udp.EnableBroadcast = true;
            _lanEnabled = true;
            _listenThread = new Thread(ListenLoop) { IsBackground = true, Name = "ChatListen" };
            _listenThread.Start();
            AddMessage($"LAN chat active on port {Port}", Color.Gray);
        }
        catch
        {
            _lanEnabled = false;
            AddMessage("LAN chat unavailable (port in use)", Color.Orange);
        }
    }

    private void ListenLoop()
    {
        var endpoint = new IPEndPoint(IPAddress.Any, Port);
        while (true)
        {
            try
            {
                var data = _udp.Receive(ref endpoint);
                var msg  = Encoding.UTF8.GetString(data);
                if (!msg.StartsWith($"[{_playerName}]"))
                    AddMessage(msg, Color.White);
            }
            catch { break; }
        }
    }

    private void BroadcastMessage(string msg)
    {
        if (!_lanEnabled) return;
        try
        {
            var data     = Encoding.UTF8.GetBytes(msg);
            var endpoint = new IPEndPoint(IPAddress.Broadcast, Port);
            _udp.Send(data, data.Length, endpoint);
        }
        catch { }
    }

    public void AddMessage(string text, Color color)
    {
        _history.Add(new ChatMessage(text, color));
        if (_history.Count > MaxHistory) _history.RemoveAt(0);
        _scrollOffset = 0;
    }

    public void ClearHistory() => _history.Clear();
    public void Open()  { _open = true;  _input = ""; }
    public void Close() { _open = false; _input = ""; }

    public void Update(GameTime gt)
    {
        float dt = (float)gt.ElapsedGameTime.TotalSeconds;
        var   kb = Keyboard.GetState();
        var   km = KeybindManager.Instance;

        for (int i = _history.Count - 1; i >= 0; i--)
        {
            if (!_open) _history[i].Age += dt;
            if (_history[i].Expired && !_open) _history.RemoveAt(i);
        }

        if (!_open)
        {
            if ((kb.IsKeyDown(km.Get(GameAction.Chat)) &&
                 !_prevKey.IsKeyDown(km.Get(GameAction.Chat))) ||
                (kb.IsKeyDown(Keys.Enter) && !_prevKey.IsKeyDown(Keys.Enter)))
                Open();
        }
        else
        {
            foreach (var key in kb.GetPressedKeys())
            {
                if (_prevKey.IsKeyDown(key)) continue;
                switch (key)
                {
                    case Keys.Escape:      Close(); break;
                    case Keys.Enter:       SendMessage(); break;
                    case Keys.Back:
                        if (_input.Length > 0) _input = _input[..^1];
                        break;
                    case Keys.Space:       _input += " ";  break;
                    case Keys.OemPeriod:   _input += ".";  break;
                    case Keys.OemComma:    _input += ",";  break;
                    case Keys.OemMinus:    _input += "-";  break;
                    case Keys.OemPlus:     _input += "+";  break;
                    case Keys.OemQuestion: _input += "/";  break;
                    default:
                        if (key >= Keys.A && key <= Keys.Z)
                        {
                            bool shift = kb.IsKeyDown(Keys.LeftShift) || kb.IsKeyDown(Keys.RightShift);
                            _input += shift ? key.ToString() : key.ToString().ToLower();
                        }
                        else if (key >= Keys.D0 && key <= Keys.D9)
                            _input += ((int)key - (int)Keys.D0).ToString();
                        break;
                }
            }

            var ms     = Mouse.GetState();
            int scroll = ms.ScrollWheelValue - 0;
            if (scroll != 0)
                _scrollOffset = Math.Clamp(_scrollOffset - scroll / 120,
                    0, Math.Max(0, _history.Count - VisibleLines));
        }

        _prevKey = kb;
    }

    private void SendMessage()
    {
        if (string.IsNullOrWhiteSpace(_input)) { Close(); return; }
        string text = _input.Trim();
        if (!_commands.TryHandle(text))
        {
            string full = $"[{_playerName}] {text}";
            AddMessage(full, Color.White);
            BroadcastMessage(full);
        }
        Close();
    }

    public void Draw(GraphicsDevice gd)
    {
        int sw    = gd.Viewport.Width;
        int sh    = gd.Viewport.Height;
        int chatX = 16;
        int chatY = sh - 160;
        int chatW = 500;
        int lineH = 20;

        _sb.Begin();

        if (_open)
            _sb.Draw(_pixel,
                new Rectangle(chatX - 4, chatY - 8, chatW + 8, VisibleLines * lineH + 44),
                new Color(0, 0, 0, 160));

        var visible = _history
            .Skip(Math.Max(0, _history.Count - VisibleLines - _scrollOffset))
            .Take(VisibleLines)
            .ToList();

        for (int i = 0; i < visible.Count; i++)
        {
            var msg   = visible[i];
            var color = msg.Color * (_open ? 1f : msg.Alpha);
            if (color.A == 0) continue;

            _sb.DrawString(_font, msg.Text,
                new Vector2(chatX + 1, chatY + i * lineH + 1),
                Color.Black * (_open ? 0.8f : msg.Alpha * 0.8f));
            _sb.DrawString(_font, msg.Text,
                new Vector2(chatX, chatY + i * lineH), color);
        }

        if (_open)
        {
            int inputY = chatY + VisibleLines * lineH + 8;
            _sb.Draw(_pixel, new Rectangle(chatX - 4, inputY - 4, chatW + 8, 28), new Color(0, 0, 0, 200));
            _sb.Draw(_pixel, new Rectangle(chatX - 4, inputY - 4, chatW + 8, 2), new Color(80, 160, 255, 200));
            _sb.DrawString(_font, _input + "_", new Vector2(chatX, inputY), Color.White);

            if (_history.Count > VisibleLines)
                _sb.DrawString(_font, "scroll to view history",
                    new Vector2(chatX + chatW - 130, inputY), Color.Gray * 0.6f);
        }

        _sb.End();
    }
}
