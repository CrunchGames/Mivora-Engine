namespace Mivora.UI;

public class GameSettings
{
    public bool  Fullscreen      = false;
    public int   RenderDistance  = 4;
    public bool  VSync           = true;
    public float FOV             = 60f;
    public int   ResolutionIndex = 0;

    public static readonly (int w, int h)[] Resolutions = new[]
    {
        (1280, 720), (1920, 1080), (2560, 1440)
    };
}
