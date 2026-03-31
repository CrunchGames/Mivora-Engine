using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Mivora.UI.Screens;

public class WorldLoadingScreen
{
    private UIRenderer _ui;
    private float      _progress  = 0f;
    private float      _elapsed   = 0f;
    private float      _duration  = 3f;
    private string     _worldName = "";
    public  bool       Done       => _progress >= 1f;

    public void Initialize(UIRenderer ui) => _ui = ui;

    public void StartLoading(string worldName)
    {
        _worldName = worldName;
        _progress  = 0f;
        _elapsed   = 0f;
    }

    public void Update(GameTime gt)
    {
        _elapsed  += (float)gt.ElapsedGameTime.TotalSeconds;
        _progress  = Math.Min(_elapsed / _duration, 1f);
    }

    public void Draw(SpriteBatch sb, GraphicsDevice gd)
    {
        int sw = gd.Viewport.Width;
        int sh = gd.Viewport.Height;

        sb.Begin();
        _ui.FillRect(new Rectangle(0, 0, sw, sh), UITheme.Background);

        _ui.DrawTextCentered("Mivora",
            new Rectangle(0, sh / 2 - 120, sw, 60), UITheme.Accent, large: true);
        _ui.DrawTextCentered($"Loading \"{_worldName}\"...",
            new Rectangle(0, sh / 2 - 40, sw, 30), UITheme.TextSecondary);

        string[] tips =
        {
            "Tip: Press F3 to open the debug menu",
            "Tip: Press T or Enter to open chat",
            "Tip: Use /help to see all commands",
            "Tip: Press V to toggle first person",
            "Tip: Scroll wheel to zoom camera",
            "Tip: Press F11 for fullscreen",
        };
        int tipIndex = (int)(_elapsed * 0.5f) % tips.Length;
        _ui.DrawTextCentered(tips[tipIndex],
            new Rectangle(0, sh / 2 + 20, sw, 24), UITheme.TextDisabled);

        int bw = 500, bh = 20;
        _ui.ProgressBar(new Rectangle(sw / 2 - bw / 2, sh / 2 + 60, bw, bh), _progress);
        _ui.DrawTextCentered($"{(int)(_progress * 100)}%",
            new Rectangle(0, sh / 2 + 90, sw, 20), UITheme.TextSecondary);

        _ui.DrawText("Strike Studio INC. Copyright @2026",
            new Vector2(10, sh - 24), UITheme.TextSecondary);

        sb.End();
    }
}
