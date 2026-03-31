using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Mivora.UI;

public class UIRenderer
{
    private SpriteBatch _sb;
    private Texture2D   _pixel;
    private SpriteFont  _font;
    private SpriteFont  _fontLarge;
    public  InputState  Input = new();

    public UIRenderer(SpriteBatch sb, Texture2D pixel, SpriteFont font, SpriteFont fontLarge)
    {
        _sb        = sb;
        _pixel     = pixel;
        _font      = font;
        _fontLarge = fontLarge;
    }

    public void UpdateInput() => Input.Update();

    public void FillRect(Rectangle r, Color c)
        => _sb.Draw(_pixel, r, c);

    public void BorderRect(Rectangle r, Color c, int thickness = 1)
    {
        _sb.Draw(_pixel, new Rectangle(r.X, r.Y, r.Width, thickness), c);
        _sb.Draw(_pixel, new Rectangle(r.X, r.Bottom - thickness, r.Width, thickness), c);
        _sb.Draw(_pixel, new Rectangle(r.X, r.Y, thickness, r.Height), c);
        _sb.Draw(_pixel, new Rectangle(r.Right - thickness, r.Y, thickness, r.Height), c);
    }

    public void Panel(Rectangle r)
    {
        FillRect(r, UITheme.Panel);
        BorderRect(r, UITheme.PanelBorder, 2);
    }

    public bool Button(Rectangle r, string label, bool danger = false)
    {
        bool hover = r.Contains(Input.MousePos);
        bool click = hover && Input.JustClicked;

        Color bg = danger
            ? (hover ? UITheme.ButtonDangerHover : UITheme.ButtonDanger)
            : (hover ? UITheme.ButtonHover : UITheme.ButtonNormal);

        FillRect(r, bg);
        BorderRect(r, hover ? UITheme.Accent : UITheme.PanelBorder, 2);
        DrawTextCentered(label, r, UITheme.TextPrimary);
        return click;
    }

    public bool Tab(Rectangle r, string label, bool active)
    {
        bool hover = r.Contains(Input.MousePos);
        bool click = hover && Input.JustClicked;

        Color bg = active ? UITheme.TabActive
                 : hover  ? UITheme.ButtonHover
                 :          UITheme.TabInactive;

        FillRect(r, bg);
        BorderRect(r, active ? UITheme.Accent : UITheme.PanelBorder, 2);
        DrawTextCentered(label, r, active ? UITheme.TextPrimary : UITheme.TextSecondary);
        return click;
    }

    public void TextInput(Rectangle r, string value, bool active)
    {
        FillRect(r, UITheme.InputBg);
        BorderRect(r, active ? UITheme.InputActive : UITheme.InputBorder, 2);
        var pos = new Vector2(r.X + 8, r.Y + r.Height / 2 - _font.LineSpacing / 2);
        _sb.DrawString(_font, value + (active ? "_" : ""), pos, UITheme.TextPrimary);
    }

    public void ProgressBar(Rectangle r, float progress)
    {
        FillRect(r, UITheme.ProgressBg);
        BorderRect(r, UITheme.PanelBorder, 2);
        int fillW = (int)((r.Width - 4) * progress);
        if (fillW > 0)
            FillRect(new Rectangle(r.X + 2, r.Y + 2, fillW, r.Height - 4), UITheme.ProgressFill);
    }

    public float Slider(Rectangle r, float value, float min, float max)
    {
        FillRect(r, UITheme.InputBg);
        BorderRect(r, UITheme.InputBorder, 2);

        float t       = (value - min) / (max - min);
        int   handleX = r.X + (int)(t * (r.Width - 12));
        FillRect(new Rectangle(r.X, r.Y + r.Height / 2 - 2, r.Width, 4), UITheme.PanelBorder);
        FillRect(new Rectangle(handleX, r.Y + 2, 12, r.Height - 4), UITheme.Accent);

        var ms = Microsoft.Xna.Framework.Input.Mouse.GetState();
        if (r.Contains(Input.MousePos) &&
            ms.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed)
        {
            float newT = Math.Clamp((float)(ms.X - r.X) / r.Width, 0f, 1f);
            return min + newT * (max - min);
        }
        return value;
    }

    public bool Toggle(Rectangle r, bool value, string label)
    {
        bool hover = r.Contains(Input.MousePos);
        bool click = hover && Input.JustClicked;

        FillRect(r, value ? UITheme.ButtonNormal : UITheme.InputBg);
        BorderRect(r, hover ? UITheme.Accent : UITheme.InputBorder, 2);
        DrawTextCentered(value ? "ON" : "OFF", r,
            value ? UITheme.TextPrimary : UITheme.TextSecondary);

        if (click) return !value;
        return value;
    }

    public void DrawText(string text, Vector2 pos, Color color, bool large = false)
        => _sb.DrawString(large ? _fontLarge : _font, text, pos, color);

    public void DrawTextCentered(string text, Rectangle r, Color color, bool large = false)
    {
        var font = large ? _fontLarge : _font;
        var size = font.MeasureString(text);
        var pos  = new Vector2(r.X + (r.Width - size.X) / 2, r.Y + (r.Height - size.Y) / 2);
        _sb.DrawString(font, text, pos, color);
    }
}
