using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mivora.UI.Screens;

namespace Mivora.UI;

public enum GameScreen
{
    Loading,
    MainMenu,
    Play,
    Settings,
    WorldLoading,
    InGame,
    Paused,
    PausedSettings
}

public class UIManager
{
    private GameScreen         _current      = GameScreen.Loading;
    private UIRenderer         _ui;
    private LoadingScreen      _loading       = new();
    private MainMenuScreen     _mainMenu      = new();
    private PlayScreen         _play          = new();
    private SettingsScreen     _settings      = new();
    private SettingsScreen     _pauseSettings = new();
    private PauseScreen        _pause         = new();
    private WorldLoadingScreen _worldLoading  = new();

    public GameScreen   Current         => _current;
    public GameSettings CurrentSettings => _settings.Settings;

    public void Initialize(SpriteBatch sb, GraphicsDevice gd,
                           SpriteFont font, SpriteFont fontLarge)
    {
        var pixel = new Texture2D(gd, 1, 1);
        pixel.SetData(new[] { Color.White });

        _ui = new UIRenderer(sb, pixel, font, fontLarge);
        _loading.Initialize(_ui);
        _mainMenu.Initialize(_ui);
        _play.Initialize(_ui);
        _settings.Initialize(_ui);
        _pauseSettings.Initialize(_ui);
        _pause.Initialize(_ui);
        _worldLoading.Initialize(_ui);
    }

    public void GoToScreen(GameScreen screen) => _current = screen;

    public void TogglePause()
    {
        if (_current == GameScreen.InGame)
        {
            _current = GameScreen.Paused;
            _pause.OnOpen();
        }
        else if (_current == GameScreen.Paused)
            _current = GameScreen.InGame;
    }

    public void StartWorldLoading(string worldName)
    {
        _worldLoading.StartLoading(worldName);
        _current = GameScreen.WorldLoading;
    }

    public void Update(GameTime gt)
    {
        _ui?.UpdateInput();

        if (_current == GameScreen.Loading)
        {
            _loading.Update(gt);
            if (_loading.Done) _current = GameScreen.MainMenu;
        }
        else if (_current == GameScreen.WorldLoading)
        {
            _worldLoading.Update(gt);
            if (_worldLoading.Done) _current = GameScreen.InGame;
        }
    }

    public void Draw(SpriteBatch sb, GraphicsDevice gd, GameTime gt)
    {
        switch (_current)
        {
            case GameScreen.Loading:
                _loading.Draw(sb, gd);
                break;

            case GameScreen.MainMenu:
                var menuAction = _mainMenu.Draw(sb, gd);
                if (menuAction == MainMenuAction.Play)     _current = GameScreen.Play;
                if (menuAction == MainMenuAction.Settings) _current = GameScreen.Settings;
                if (menuAction == MainMenuAction.Quit)     System.Environment.Exit(0);
                break;

            case GameScreen.Play:
                var playAction = _play.Draw(sb, gd);
                if (playAction == PlayScreenAction.Back)
                    _current = GameScreen.MainMenu;
                if (playAction == PlayScreenAction.StartSinglePlayer)
                    StartWorldLoading(_play.ActiveWorld?.Name ?? "World");
                break;

            case GameScreen.Settings:
                var settingsAction = _settings.Draw(sb, gd, gt);
                if (settingsAction == SettingsAction.Back) _current = GameScreen.MainMenu;
                break;

            case GameScreen.WorldLoading:
                _worldLoading.Draw(sb, gd);
                break;

            case GameScreen.Paused:
                var pauseAction = _pause.Draw(sb, gd, gt);
                if (pauseAction == PauseAction.Resume)     _current = GameScreen.InGame;
                if (pauseAction == PauseAction.Settings)   _current = GameScreen.PausedSettings;
                if (pauseAction == PauseAction.LeaveWorld) _current = GameScreen.MainMenu;
                break;

            case GameScreen.PausedSettings:
                var psAction = _pauseSettings.Draw(sb, gd, gt);
                if (psAction == SettingsAction.Back) _current = GameScreen.Paused;
                break;
        }
    }
}
