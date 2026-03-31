using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Mivora.UI.Screens;

public class LoadingScreen
{
    private float      _progress = 0f;
    private float      _elapsed  = 0f;
    private float      _duration = 3f;
    public  bool       Done      => _progress >= 1f;
    private UIRenderer _ui;

    public void Initialize(UIRenderer ui) => _ui = ui;

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
            new Rectangle(0, sh / 2 - 80, sw, 60), UITheme.Accent, large: true);
        _ui.DrawTextCentered("Loading...",
            new Rectangle(0, sh / 2 - 10, sw, 30), UITheme.TextSecondary);

        int bw = 400, bh = 20;
        _ui.ProgressBar(new Rectangle(sw / 2 - bw / 2, sh / 2 + 40, bw, bh), _progress);
        _ui.DrawTextCentered($"{(int)(_progress * 100)}%",
            new Rectangle(0, sh / 2 + 70, sw, 20), UITheme.TextSecondary);

        sb.End();
    }
}
