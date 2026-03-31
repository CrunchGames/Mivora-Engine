using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Mivora.Input;

namespace Mivora.UI.Screens;

public enum KeybindScreenAction { None, Back }

public class KeybindScreen
{
    private UIRenderer     _ui;
    private KeybindManager _keybinds;
    private GameAction?    _listening     = null;
    private string         _conflict      = "";
    private float          _conflictTimer = 0f;
    private int            _scrollOffset  = 0;
    private const int      RowsPerPage    = 8;

    public void Initialize(UIRenderer ui, KeybindManager keybinds)
    {
        _ui       = ui;
        _keybinds = keybinds;
    }

    public KeybindScreenAction Draw(SpriteBatch sb, GraphicsDevice gd, GameTime gt)
    {
        int sw     = gd.Viewport.Width;
        int sh     = gd.Viewport.Height;
        var action = KeybindScreenAction.None;
        var kb     = Keyboard.GetState();

        if (_listening.HasValue)
        {
            foreach (var key in kb.GetPressedKeys())
            {
                if (key == Keys.Escape) { _listening = null; break; }
                if (_keybinds.IsConflict(_listening.Value, key))
                {
                    _conflict      = $"Key {key} is already used!";
                    _conflictTimer = 2f;
                    _listening     = null;
                    break;
                }
                _keybinds.Set(_listening.Value, key);
                _keybinds.Save();
                _listening = null;
                break;
            }
        }

        if (_conflictTimer > 0f) _conflictTimer -= (float)gt.ElapsedGameTime.TotalSeconds;

        sb.Begin();
        _ui.FillRect(new Rectangle(0, 0, sw, sh), UITheme.Background);
        _ui.DrawTextCentered("Keybinds", new Rectangle(0, 20, sw, 40), UITheme.TextPrimary, large: true);

        int pw = 560, ph = 420;
        var panel = new Rectangle(sw / 2 - pw / 2, 70, pw, ph);
        _ui.Panel(panel);

        int lx = panel.X + 20, valX = panel.X + pw - 220;
        _ui.DrawText("Action",  new Vector2(lx,   panel.Y + 12), UITheme.TextSecondary);
        _ui.DrawText("Keybind", new Vector2(valX, panel.Y + 12), UITheme.TextSecondary);
        _ui.FillRect(new Rectangle(panel.X + 10, panel.Y + 32, pw - 20, 1), UITheme.PanelBorder);

        var actions = Enum.GetValues<GameAction>();
        int rowH    = 44, startY = panel.Y + 40;
        int visible = Math.Min(RowsPerPage, actions.Length - _scrollOffset);

        for (int i = 0; i < visible; i++)
        {
            var  gameAction  = actions[i + _scrollOffset];
            var  currentKey  = _keybinds.Get(gameAction);
            bool isListening = _listening == gameAction;

            var rowRect = new Rectangle(panel.X + 8, startY + i * rowH, pw - 16, rowH - 4);
            _ui.FillRect(rowRect, i % 2 == 0 ? UITheme.Panel : UITheme.Background);
            _ui.DrawText(KeybindManager.ActionNames[gameAction],
                new Vector2(lx, rowRect.Y + rowH / 2 - 8), UITheme.TextPrimary);

            var keyRect = new Rectangle(valX, rowRect.Y + 4, 160, rowH - 12);
            if (isListening)
            {
                _ui.FillRect(keyRect, UITheme.Accent);
                _ui.BorderRect(keyRect, UITheme.TextPrimary, 2);
                _ui.DrawTextCentered("Press a key...", keyRect, UITheme.Background);
            }
            else
            {
                if (_ui.Button(keyRect, currentKey.ToString())) _listening = gameAction;
            }
        }

        if (_scrollOffset > 0)
            if (_ui.Button(new Rectangle(panel.Right - 44, panel.Y + 40, 32, 32), "^")) _scrollOffset--;
        if (_scrollOffset + RowsPerPage < actions.Length)
            if (_ui.Button(new Rectangle(panel.Right - 44, panel.Bottom - 72, 32, 32), "v")) _scrollOffset++;

        if (_conflictTimer > 0f && _conflict != "")
            _ui.DrawTextCentered(_conflict, new Rectangle(panel.X, panel.Bottom - 30, pw, 24), new Color(255, 80, 80));
        if (_listening.HasValue)
            _ui.DrawTextCentered("Press any key to bind. Escape to cancel.",
                new Rectangle(panel.X, panel.Bottom - 30, pw, 24), UITheme.TextSecondary);

        int by = panel.Bottom + 12;
        if (_ui.Button(new Rectangle(sw / 2 - 170, by, 160, 40), "< Back")) action = KeybindScreenAction.Back;
        if (_ui.Button(new Rectangle(sw / 2 + 10,  by, 160, 40), "Reset Defaults")) { _keybinds.ResetToDefaults(); _keybinds.Save(); }

        sb.End();
        return action;
    }
}
