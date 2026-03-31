using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Mivora.UI.Screens;

public enum MainMenuAction { None, Play, Settings, Quit }

public class MainMenuScreen
{
    private UIRenderer _ui;

    public void Initialize(UIRenderer ui) => _ui = ui;

    public MainMenuAction Draw(SpriteBatch sb, GraphicsDevice gd)
    {
        int sw = gd.Viewport.Width;
        int sh = gd.Viewport.Height;
        var action = MainMenuAction.None;

        sb.Begin();

        _ui.FillRect(new Rectangle(0, 0, sw, sh), UITheme.Background);
        _ui.DrawTextCentered("Mivora",
            new Rectangle(0, sh / 4 - 40, sw, 80), UITheme.Accent, large: true);

        int pw = 320, ph = 220;
        var panel = new Rectangle(sw / 2 - pw / 2, sh / 2 - ph / 2, pw, ph);
        _ui.Panel(panel);

        int bw = 260, bh = 44;
        int bx = sw / 2 - bw / 2;

        if (_ui.Button(new Rectangle(bx, panel.Y + 20,  bw, bh), "Play"))
            action = MainMenuAction.Play;
        if (_ui.Button(new Rectangle(bx, panel.Y + 76,  bw, bh), "Settings"))
            action = MainMenuAction.Settings;
        if (_ui.Button(new Rectangle(bx, panel.Y + 132, bw, bh), "Quit", danger: true))
            action = MainMenuAction.Quit;

        _ui.DrawText("Version: 2k09P Beta",
            new Vector2(sw - 160, 10), UITheme.TextSecondary);
        _ui.DrawText("Strike Studio INC. Copyright @2026",
            new Vector2(10, sh - 24), UITheme.TextSecondary);

        sb.End();
        return action;
    }
}
