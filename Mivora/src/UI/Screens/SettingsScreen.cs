using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mivora.Input;

namespace Mivora.UI.Screens;

public enum SettingsAction { None, Back }
public enum SettingsTab   { Graphics, Credits, Keybinds }

public class SettingsScreen
{
    private UIRenderer    _ui;
    private SettingsTab   _tab           = SettingsTab.Graphics;
    private KeybindScreen _keybindScreen = new();
    public  GameSettings  Settings       = new();

    public void Initialize(UIRenderer ui)
    {
        _ui = ui;
        _keybindScreen.Initialize(ui, KeybindManager.Instance);
    }

    public SettingsAction Draw(SpriteBatch sb, GraphicsDevice gd, GameTime gt)
    {
        int sw     = gd.Viewport.Width;
        int sh     = gd.Viewport.Height;
        var action = SettingsAction.None;

        if (_tab == SettingsTab.Keybinds)
        {
            var kbAction = _keybindScreen.Draw(sb, gd, gt);
            if (kbAction == KeybindScreenAction.Back) _tab = SettingsTab.Graphics;
            return action;
        }

        sb.Begin();
        _ui.FillRect(new Rectangle(0, 0, sw, sh), UITheme.Background);
        _ui.DrawTextCentered("Settings", new Rectangle(0, 20, sw, 40), UITheme.TextPrimary, large: true);

        int tw = 140, th = 40;
        int tx = sw / 2 - (tw * 3 + 16) / 2;
        if (_ui.Tab(new Rectangle(tx,              80, tw, th), "Graphics", _tab == SettingsTab.Graphics)) _tab = SettingsTab.Graphics;
        if (_ui.Tab(new Rectangle(tx + tw + 8,     80, tw, th), "Credits",  _tab == SettingsTab.Credits))  _tab = SettingsTab.Credits;
        if (_ui.Tab(new Rectangle(tx + (tw+8) * 2, 80, tw, th), "Keybinds", _tab == SettingsTab.Keybinds)) _tab = SettingsTab.Keybinds;

        int pw = 520, ph = 380;
        var panel = new Rectangle(sw / 2 - pw / 2, 130, pw, ph);
        _ui.Panel(panel);

        if (_tab == SettingsTab.Graphics)
        {
            int lx = panel.X + 24, vx = panel.X + pw - 224, rw = 200, ry = panel.Y + 24, gap = 56;

            _ui.DrawText("Resolution", new Vector2(lx, ry + 10), UITheme.TextSecondary);
            if (_ui.Button(new Rectangle(vx, ry, rw, 36),
                $"{GameSettings.Resolutions[Settings.ResolutionIndex].w}x{GameSettings.Resolutions[Settings.ResolutionIndex].h}"))
                Settings.ResolutionIndex = (Settings.ResolutionIndex + 1) % GameSettings.Resolutions.Length;

            _ui.DrawText("Fullscreen", new Vector2(lx, ry + gap + 10), UITheme.TextSecondary);
            Settings.Fullscreen = _ui.Toggle(new Rectangle(vx, ry + gap, rw, 36), Settings.Fullscreen, "");

            _ui.DrawText("VSync", new Vector2(lx, ry + gap * 2 + 10), UITheme.TextSecondary);
            Settings.VSync = _ui.Toggle(new Rectangle(vx, ry + gap * 2, rw, 36), Settings.VSync, "");

            _ui.DrawText($"Render Distance: {Settings.RenderDistance}", new Vector2(lx, ry + gap * 3 + 10), UITheme.TextSecondary);
            Settings.RenderDistance = (int)_ui.Slider(new Rectangle(vx, ry + gap * 3, rw, 36), Settings.RenderDistance, 2, 16);

            _ui.DrawText($"FOV: {(int)Settings.FOV}", new Vector2(lx, ry + gap * 4 + 10), UITheme.TextSecondary);
            Settings.FOV = _ui.Slider(new Rectangle(vx, ry + gap * 4, rw, 36), Settings.FOV, 40f, 110f);
        }
        else if (_tab == SettingsTab.Credits)
        {
            int cy = panel.Y + 40;
            _ui.DrawTextCentered("Mivora",                          new Rectangle(panel.X, cy,       pw, 36), UITheme.Accent, large: true);
            _ui.DrawTextCentered("Version: 2k09P Beta",              new Rectangle(panel.X, cy + 50,  pw, 24), UITheme.TextSecondary);
            _ui.DrawTextCentered("Developed by Strike Studio INC.",   new Rectangle(panel.X, cy + 90,  pw, 24), UITheme.TextSecondary);
            _ui.DrawTextCentered("Copyright @2026 Strike Studio INC.",new Rectangle(panel.X, cy + 120, pw, 24), UITheme.TextSecondary);
            _ui.DrawTextCentered("Built with MonoGame",               new Rectangle(panel.X, cy + 170, pw, 24), UITheme.TextDisabled);
            _ui.DrawTextCentered("3D Models made in BlockBench",      new Rectangle(panel.X, cy + 200, pw, 24), UITheme.TextDisabled);
        }

        if (_ui.Button(new Rectangle(20, 20, 100, 36), "< Back")) action = SettingsAction.Back;

        sb.End();
        return action;
    }
}
