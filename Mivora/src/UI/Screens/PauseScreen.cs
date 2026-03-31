using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Mivora.UI.Screens;

public enum PauseAction { None, Resume, Settings, LeaveWorld }

public class PauseScreen
{
    private UIRenderer _ui;
    private float      _openTimer = 0f;
    private const float OpenDelay = 0.15f;

    public void Initialize(UIRenderer ui) => _ui = ui;
    public void OnOpen() => _openTimer = 0f;

    public PauseAction Draw(SpriteBatch sb, GraphicsDevice gd, GameTime gt)
    {
        int sw = gd.Viewport.Width;
        int sh = gd.Viewport.Height;
        var action = PauseAction.None;

        _openTimer += (float)gt.ElapsedGameTime.TotalSeconds;
        bool canClick = _openTimer >= OpenDelay;

        sb.Begin();
        _ui.FillRect(new Rectangle(0, 0, sw, sh), new Color(0, 0, 0, 160));

        int pw = 320, ph = 240;
        var panel = new Rectangle(sw / 2 - pw / 2, sh / 2 - ph / 2, pw, ph);
        _ui.Panel(panel);

        _ui.DrawTextCentered("Paused",
            new Rectangle(panel.X, panel.Y + 10, pw, 36), UITheme.Accent, large: true);

        int bw = 260, bh = 44;
        int bx = sw / 2 - bw / 2;

        if (canClick)
        {
            if (_ui.Button(new Rectangle(bx, panel.Y + 56,  bw, bh), "Return to Game")) action = PauseAction.Resume;
            if (_ui.Button(new Rectangle(bx, panel.Y + 108, bw, bh), "Settings"))        action = PauseAction.Settings;
            if (_ui.Button(new Rectangle(bx, panel.Y + 160, bw, bh), "Leave World", danger: true)) action = PauseAction.LeaveWorld;
        }
        else
        {
            _ui.FillRect(new Rectangle(bx, panel.Y + 56,  bw, bh), UITheme.Panel);
            _ui.BorderRect(new Rectangle(bx, panel.Y + 56,  bw, bh), UITheme.PanelBorder, 2);
            _ui.DrawTextCentered("Return to Game", new Rectangle(bx, panel.Y + 56,  bw, bh), UITheme.TextSecondary);
            _ui.FillRect(new Rectangle(bx, panel.Y + 108, bw, bh), UITheme.Panel);
            _ui.BorderRect(new Rectangle(bx, panel.Y + 108, bw, bh), UITheme.PanelBorder, 2);
            _ui.DrawTextCentered("Settings",       new Rectangle(bx, panel.Y + 108, bw, bh), UITheme.TextSecondary);
            _ui.FillRect(new Rectangle(bx, panel.Y + 160, bw, bh), UITheme.Panel);
            _ui.BorderRect(new Rectangle(bx, panel.Y + 160, bw, bh), UITheme.PanelBorder, 2);
            _ui.DrawTextCentered("Leave World",    new Rectangle(bx, panel.Y + 160, bw, bh), UITheme.TextSecondary);
        }

        sb.End();
        return action;
    }
}
