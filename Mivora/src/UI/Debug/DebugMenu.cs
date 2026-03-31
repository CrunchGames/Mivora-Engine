using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Mivora.Entities;
using Mivora.Input;
using Mivora.World;

namespace Mivora.UI.Debug;

public class DebugMenu
{
    private bool        _visible   = false;
    private SpriteFont  _font;
    private SpriteBatch _sb;
    private Texture2D   _pixel;

    private float _fps        = 0f;
    private float _fpsTimer   = 0f;
    private int   _frameCount = 0;
    private float _minFps     = float.MaxValue;
    private float _maxFps     = 0f;
    private float _ping       = 0f;
    private float _pingTimer  = 0f;
    private Random _rng       = new();
    private KeyboardState _prevKey;

    public bool IsVisible => _visible;

    public void Initialize(SpriteBatch sb, SpriteFont font, GraphicsDevice gd)
    {
        _sb   = sb;
        _font = font;
        _pixel = new Texture2D(gd, 1, 1);
        _pixel.SetData(new[] { Color.White });
    }

    public void Update(GameTime gt)
    {
        var   kb = Keyboard.GetState();
        var   km = KeybindManager.Instance;
        float dt = (float)gt.ElapsedGameTime.TotalSeconds;

        if (kb.IsKeyDown(km.Get(GameAction.DebugMenu)) &&
            !_prevKey.IsKeyDown(km.Get(GameAction.DebugMenu)))
            _visible = !_visible;

        _frameCount++;
        _fpsTimer += dt;
        if (_fpsTimer >= 0.5f)
        {
            _fps        = _frameCount / _fpsTimer;
            _minFps     = MathF.Min(_minFps, _fps);
            _maxFps     = MathF.Max(_maxFps, _fps);
            _frameCount = 0;
            _fpsTimer   = 0f;
        }

        _pingTimer += dt;
        if (_pingTimer >= 2f) { _ping = _rng.Next(10, 60); _pingTimer = 0f; }

        _prevKey = kb;
    }

    public void Draw(GraphicsDevice gd, Player player, MivoraWorld world)
    {
        if (!_visible) return;

        int sw = gd.Viewport.Width;
        int sh = gd.Viewport.Height;

        var pos    = player.Position;
        int chunkX = (int)MathF.Floor(pos.X / Chunk.Width);
        int chunkZ = (int)MathF.Floor(pos.Z / Chunk.Depth);
        int localX = (int)MathF.Floor(pos.X) - chunkX * Chunk.Width;
        int localZ = (int)MathF.Floor(pos.Z) - chunkZ * Chunk.Depth;

        Color fpsColor  = _fps  >= 60 ? Color.Green : _fps  >= 30 ? Color.Yellow : Color.Red;
        Color pingColor = _ping < 30  ? Color.Green : _ping < 60  ? Color.Yellow : Color.Red;

        var km = KeybindManager.Instance;

        var left = new List<(string, Color)>
        {
            ("Mivora 2k09P Beta | Strike Studio INC.", Color.Yellow),
            ("", Color.White),
            ($"FPS: {_fps:F0}  min:{_minFps:F0}  max:{_maxFps:F0}", fpsColor),
            ($"Ping: {_ping:F0}ms (LAN)", pingColor),
            ("", Color.White),
            ($"XYZ: {pos.X:F2} / {pos.Y:F2} / {pos.Z:F2}", Color.White),
            ($"Block: {(int)pos.X} {(int)pos.Y} {(int)pos.Z}", Color.White),
            ($"Chunk: {chunkX} {chunkZ}  (local {localX} {localZ})", Color.White),
            ($"Yaw: {MathHelper.ToDegrees(player.Yaw):F1} deg", Color.White),
            ("", Color.White),
            ($"Facing: {GetFacing(player.Yaw)}", Color.Cyan),
        };

        var right = new List<(string, Color)>
        {
            ("Renderer: MonoGame OpenGL", Color.White),
            ($"Display: {sw}x{sh}", Color.White),
            ($"Render dist: {world.RenderDistance} chunks", Color.White),
            ("", Color.White),
            ("Controls:", Color.Yellow),
            ($"{km.Get(GameAction.MoveForward)}/{km.Get(GameAction.MoveBackward)}/{km.Get(GameAction.MoveLeft)}/{km.Get(GameAction.MoveRight)} - Move", Color.Gray),
            ($"{km.Get(GameAction.Jump)} - Jump", Color.Gray),
            ($"{km.Get(GameAction.Sprint)} - Sprint", Color.Gray),
            ($"{km.Get(GameAction.Chat)} / Enter - Chat", Color.Gray),
            ($"{km.Get(GameAction.TogglePerson)} - Toggle Person", Color.Gray),
            ($"{km.Get(GameAction.Pause)} - Pause", Color.Gray),
            ($"{km.Get(GameAction.DebugMenu)} - Debug Menu", Color.Gray),
            ($"{km.Get(GameAction.Fullscreen)} - Fullscreen", Color.Gray),
        };

        int rightPanelW = 0;
        foreach (var (text, _) in right)
        {
            if (text == "") continue;
            int w = (int)_font.MeasureString(text).X;
            if (w > rightPanelW) rightPanelW = w;
        }

        _sb.Begin();
        DrawPanel(left,  10, 10, false);
        DrawPanel(right, sw - rightPanelW - 16, 10, true);
        _sb.End();
    }

    private void DrawPanel(List<(string text, Color color)> lines, int x, int y, bool rightAlign)
    {
        int lineH = 18, padding = 6, maxW = 0;
        foreach (var (text, _) in lines)
        {
            if (text == "") continue;
            int w = (int)_font.MeasureString(text).X;
            if (w > maxW) maxW = w;
        }

        _sb.Draw(_pixel,
            new Rectangle(x - padding, y - padding, maxW + padding * 2, lines.Count * lineH + padding * 2),
            new Color(0, 0, 0, 160));

        for (int i = 0; i < lines.Count; i++)
        {
            var (text, color) = lines[i];
            if (text == "") continue;
            float tx = rightAlign ? x + maxW - _font.MeasureString(text).X : x;
            float ty = y + i * lineH;
            _sb.DrawString(_font, text, new Vector2(tx + 1, ty + 1), Color.Black * 0.8f);
            _sb.DrawString(_font, text, new Vector2(tx, ty), color);
        }
    }

    private string GetFacing(float yaw)
    {
        float deg = MathHelper.ToDegrees(yaw) % 360f;
        if (deg < 0) deg += 360f;
        if (deg >= 315 || deg <  45) return "South (+Z)";
        if (deg >=  45 && deg < 135) return "West (-X)";
        if (deg >= 135 && deg < 225) return "North (-Z)";
        return "East (+X)";
    }
}
