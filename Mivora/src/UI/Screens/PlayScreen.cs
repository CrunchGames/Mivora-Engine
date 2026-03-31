using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Mivora.World;

namespace Mivora.UI.Screens;

public enum PlayScreenAction { None, Back, StartSinglePlayer }

public class PlayScreen
{
    private UIRenderer      _ui;
    private int             _tab           = 0;
    private string          _worldName     = "My World";
    private int             _worldGenType  = 0;
    private int             _dropRate      = 1;
    private bool            _nameActive    = false;
    private bool            _creating      = false;
    private List<WorldInfo> _worlds        = new();
    private int             _selectedWorld = -1;

    private static readonly string[] GenTypes  = { "Normal", "Flat", "Amplified" };
    private static readonly string[] DropRates = { "Low", "Normal", "High" };

    private KeyboardState _prevKey;
    public WorldInfo ActiveWorld { get; private set; }

    public void Initialize(UIRenderer ui) { _ui = ui; RefreshWorlds(); }

    private void RefreshWorlds() => _worlds = WorldSaveManager.GetWorlds();

    public PlayScreenAction Draw(SpriteBatch sb, GraphicsDevice gd)
    {
        int sw = gd.Viewport.Width;
        int sh = gd.Viewport.Height;
        var action = PlayScreenAction.None;
        var kb = Keyboard.GetState();

        sb.Begin();
        _ui.FillRect(new Rectangle(0, 0, sw, sh), UITheme.Background);
        _ui.DrawTextCentered("Select Mode", new Rectangle(0, 20, sw, 40), UITheme.TextPrimary, large: true);

        int tw = 200, th = 40;
        int tx = sw / 2 - tw - 4;
        if (_ui.Tab(new Rectangle(tx,          80, tw, th), "Singleplayer", _tab == 0)) { _tab = 0; _creating = false; }
        if (_ui.Tab(new Rectangle(tx + tw + 8, 80, tw, th), "Multiplayer",  _tab == 1)) _tab = 1;

        int pw = 540, ph = 400;
        var panel = new Rectangle(sw / 2 - pw / 2, 130, pw, ph);
        _ui.Panel(panel);

        if (_tab == 0)
        {
            if (!_creating)
            {
                _ui.DrawTextCentered("Your Worlds", new Rectangle(panel.X, panel.Y + 10, pw, 30), UITheme.TextSecondary);

                int listH = 260;
                var listPanel = new Rectangle(panel.X + 16, panel.Y + 44, pw - 32, listH);
                _ui.FillRect(listPanel, UITheme.Background);
                _ui.BorderRect(listPanel, UITheme.PanelBorder);

                if (_worlds.Count == 0)
                    _ui.DrawTextCentered("No worlds yet - create one!", listPanel, UITheme.TextDisabled);
                else
                {
                    for (int i = 0; i < _worlds.Count; i++)
                    {
                        var row = new Rectangle(listPanel.X + 4, listPanel.Y + 4 + i * 56, listPanel.Width - 8, 52);
                        bool selected = _selectedWorld == i;
                        _ui.FillRect(row,   selected ? UITheme.TabActive  : UITheme.Panel);
                        _ui.BorderRect(row, selected ? UITheme.Accent     : UITheme.PanelBorder);
                        if (_ui.Input.JustClicked && row.Contains(_ui.Input.MousePos)) _selectedWorld = i;
                        _ui.DrawText(_worlds[i].Name, new Vector2(row.X + 10, row.Y + 8), UITheme.TextPrimary);
                        _ui.DrawText($"{_worlds[i].GenType} | Drops: {_worlds[i].DropRate} | Last: {_worlds[i].LastPlayed}",
                            new Vector2(row.X + 10, row.Y + 30), UITheme.TextSecondary);
                    }
                }

                int bw = 160, bh = 40, by = panel.Bottom - 56, bx = panel.X + 16;
                bool canPlay = _selectedWorld >= 0 && _selectedWorld < _worlds.Count;

                if (_ui.Button(new Rectangle(bx, by, bw, bh), "Create World")) { _creating = true; _worldName = "My World"; _selectedWorld = -1; }
                if (_ui.Button(new Rectangle(bx + bw + 10, by, bw, bh), "Play World") && canPlay) { ActiveWorld = _worlds[_selectedWorld]; WorldSaveManager.SaveWorld(ActiveWorld); action = PlayScreenAction.StartSinglePlayer; }
                if (_ui.Button(new Rectangle(bx + (bw + 10) * 2, by, bw, bh), "Delete", danger: true) && canPlay) { _worlds.RemoveAt(_selectedWorld); _selectedWorld = -1; }
            }
            else
            {
                int lx = panel.X + 24, fw = pw - 48;
                _ui.DrawText("World Name", new Vector2(lx, panel.Y + 20), UITheme.TextSecondary);
                var nameRect = new Rectangle(lx, panel.Y + 44, fw, 36);
                _ui.TextInput(nameRect, _worldName, _nameActive);
                if (_ui.Input.JustClicked) _nameActive = nameRect.Contains(_ui.Input.MousePos);

                if (_nameActive)
                    foreach (var key in kb.GetPressedKeys())
                    {
                        if (_prevKey.IsKeyDown(key)) continue;
                        if (key == Keys.Back && _worldName.Length > 0) _worldName = _worldName[..^1];
                        else if (key >= Keys.A && key <= Keys.Z) _worldName += kb.IsKeyDown(Keys.LeftShift) ? key.ToString() : key.ToString().ToLower();
                        else if (key == Keys.Space) _worldName += " ";
                    }

                _ui.DrawText("World Generation", new Vector2(lx, panel.Y + 100), UITheme.TextSecondary);
                int segW = (fw / 3) - 4;
                for (int i = 0; i < GenTypes.Length; i++)
                { var r = new Rectangle(lx + i * (segW + 6), panel.Y + 124, segW, 36); if (_ui.Tab(r, GenTypes[i], _worldGenType == i)) _worldGenType = i; }

                _ui.DrawText("Utility Drop Rate", new Vector2(lx, panel.Y + 178), UITheme.TextSecondary);
                for (int i = 0; i < DropRates.Length; i++)
                { var r = new Rectangle(lx + i * (segW + 6), panel.Y + 202, segW, 36); if (_ui.Tab(r, DropRates[i], _dropRate == i)) _dropRate = i; }

                int bby = panel.Bottom - 56;
                if (_ui.Button(new Rectangle(lx, bby, 160, 40), "Back")) { _creating = false; RefreshWorlds(); }
                if (_ui.Button(new Rectangle(panel.Right - 184, bby, 160, 40), "Create"))
                {
                    var info = new WorldInfo { Name = string.IsNullOrWhiteSpace(_worldName) ? "My World" : _worldName, GenType = GenTypes[_worldGenType], DropRate = DropRates[_dropRate] };
                    WorldSaveManager.SaveWorld(info);
                    ActiveWorld = info;
                    action = PlayScreenAction.StartSinglePlayer;
                }
            }
        }
        else
            _ui.DrawTextCentered("Coming Soon", panel, UITheme.TextSecondary, large: true);

        if (_ui.Button(new Rectangle(20, 20, 100, 36), "Back")) action = PlayScreenAction.Back;

        _prevKey = kb;
        sb.End();
        return action;
    }
}