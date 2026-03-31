using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Mivora.Graphics;
using Mivora.Entities;
using Mivora.World;
using Mivora.UI;
using Mivora.UI.Chat;
using Mivora.UI.Debug;
using Mivora.Input;

namespace Mivora.Core;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch           _spriteBatch;
    private MivoraModel          _playerModel;
    private BasicEffect           _effect;
    private Player                _player;
    private Camera                _camera;
    private MivoraWorld          _world;
    private UIManager             _ui;
    private ChatSystem            _chat;
    private DebugMenu             _debug;
    private KeyboardState         _prevKey;
    private bool                  _fullscreen = false;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        _graphics.PreferredBackBufferWidth  = 1280;
        _graphics.PreferredBackBufferHeight = 720;
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        _player = new Player { Position = new Vector3(8, 30, 8) };
        _camera = new Camera();
        _camera.Initialize(GraphicsDevice);
        _world  = new MivoraWorld();
        _world.Initialize(GraphicsDevice);
        _ui     = new UIManager();
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        KeybindManager.Instance.Load();

        var font      = Content.Load<SpriteFont>("Fonts/DefaultFont");
        var fontLarge = Content.Load<SpriteFont>("Fonts/DefaultFont");
        _ui.Initialize(_spriteBatch, GraphicsDevice, font, fontLarge);

        _chat = new ChatSystem();
        _chat.Initialize(_spriteBatch, font, GraphicsDevice, "Player");

        _debug = new DebugMenu();
        _debug.Initialize(_spriteBatch, font, GraphicsDevice);

        Texture2D playerTex = null;
        if (File.Exists("Content/Models/player_texture.png"))
        {
            using var stream = File.OpenRead("Content/Models/player_texture.png");
            playerTex = Texture2D.FromStream(GraphicsDevice, stream);
        }
        if (File.Exists("Content/Models/player.obj"))
            _playerModel = ObjModelLoader.Load(GraphicsDevice,
                "Content/Models/player.obj", playerTex);

        _effect = new BasicEffect(GraphicsDevice)
        {
            VertexColorEnabled = false,
            LightingEnabled    = false,
            TextureEnabled     = true,
        };
    }

    private void ToggleFullscreen()
    {
        _fullscreen = !_fullscreen;
        ApplyFullscreen();
        _ui.CurrentSettings.Fullscreen = _fullscreen;
    }

    private void ApplyFullscreen()
    {
        if (_fullscreen)
        {
            _graphics.PreferredBackBufferWidth  = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            _graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            _graphics.HardwareModeSwitch        = false;
            _graphics.IsFullScreen              = true;
        }
        else
        {
            _graphics.IsFullScreen              = false;
            _graphics.HardwareModeSwitch        = true;
            _graphics.PreferredBackBufferWidth  = 1280;
            _graphics.PreferredBackBufferHeight = 720;
        }

        _graphics.ApplyChanges();
    }

    private void ApplyGraphicsSettings()
    {
        var s = _ui.CurrentSettings;

        if (s.Fullscreen != _fullscreen)
        {
            _fullscreen = s.Fullscreen;
            ApplyFullscreen();
        }

        _graphics.SynchronizeWithVerticalRetrace = s.VSync;
        _graphics.ApplyChanges();
        _world.SetRenderDistance(s.RenderDistance);
        _camera.UpdateProjection(GraphicsDevice, s.FOV);
    }

    protected override void Update(GameTime gameTime)
    {
        var kb = Keyboard.GetState();
        var km = KeybindManager.Instance;

        bool escJustPressed = kb.IsKeyDown(km.Get(GameAction.Pause)) &&
                              !_prevKey.IsKeyDown(km.Get(GameAction.Pause));

        if (kb.IsKeyDown(km.Get(GameAction.Fullscreen)) &&
            !_prevKey.IsKeyDown(km.Get(GameAction.Fullscreen)))
            ToggleFullscreen();

        if (_ui.Current == GameScreen.InGame)
        {
            IsMouseVisible = _chat.IsOpen;

            if (escJustPressed)
            {
                if (_chat.IsOpen)
                    _chat.Close();
                else
                {
                    IsMouseVisible = true;
                    _ui.TogglePause();
                }
            }
            else
            {
                _chat.Update(gameTime);
                _debug.Update(gameTime);

                if (!_chat.IsOpen)
                {
                    _camera.Update(GraphicsDevice, _player.Position);
                    _player.Update(gameTime, _camera.Yaw, _world);
                    _world.Update(GraphicsDevice, _player.Position);
                }
                else
                {
                    _camera.Update(GraphicsDevice, _player.Position);
                    _world.Update(GraphicsDevice, _player.Position);
                }
            }
        }
        else if (_ui.Current == GameScreen.Paused ||
                 _ui.Current == GameScreen.PausedSettings)
        {
            IsMouseVisible = true;
            _ui.Update(gameTime);

            if (escJustPressed)
                _ui.GoToScreen(GameScreen.InGame);
        }
        else
        {
            IsMouseVisible = true;
            _ui.Update(gameTime);
            ApplyGraphicsSettings();
        }

        _prevKey = kb;
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        if (_ui.Current == GameScreen.InGame  ||
            _ui.Current == GameScreen.Paused  ||
            _ui.Current == GameScreen.PausedSettings)
        {
            GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;

            _effect.View            = _camera.View;
            _effect.Projection      = _camera.Projection;
            _effect.DiffuseColor    = Vector3.One;
            _effect.Alpha           = 1f;
            _effect.LightingEnabled = false;

            _world.Draw(GraphicsDevice, _effect, _camera.View, _camera.Projection);

            if (_playerModel != null)
            {
                _effect.World = Matrix.CreateRotationY(_player.VisualYaw + MathHelper.Pi)
                              * Matrix.CreateTranslation(_player.Position);
                _effect.Texture        = _playerModel.Texture;
                _effect.TextureEnabled = _playerModel.Texture != null;

                GraphicsDevice.SetVertexBuffer(_playerModel.VertexBuffer);
                GraphicsDevice.Indices = _playerModel.IndexBuffer;

                foreach (var pass in _effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    GraphicsDevice.DrawIndexedPrimitives(
                        PrimitiveType.TriangleList, 0, 0,
                        _playerModel.IndexCount / 3);
                }
            }

            if (_ui.Current == GameScreen.InGame)
            {
                _chat.Draw(GraphicsDevice);
                _debug.Draw(GraphicsDevice, _player, _world);
            }

            if (_ui.Current == GameScreen.Paused ||
                _ui.Current == GameScreen.PausedSettings)
                _ui.Draw(_spriteBatch, GraphicsDevice, gameTime);
        }
        else
        {
            _ui.Draw(_spriteBatch, GraphicsDevice, gameTime);
        }

        base.Draw(gameTime);
    }
}