using Mivora;

public class Engine {
  
  public bool IsRaining = true;
  public bool IsSunny = true;
  public bool IsKhaos = false; // NEVER USE THIS IMPORT
  public bool IsWin = false;

  public int DayCount = 5; // 5x5 = 25 Minutes
  public double Days = 999999999.9999;
  public int Weather = -1;

  public String version = "Engine 1.0.0";
  public String hotfix = "Engine Hotfix - 1";
  public String news = "Eninge News";

  public String framework = "MonoGame";
  public String framework2 = "OpenTK";

  protected class Engine() {

    IsRaining;
    IsSunny;
    IsKhaos;
    IsWin;

    DayCount;
    Days;
    Weather;

    version;
    hotifx;
    news;
    framework;
    framework2;
  }
}
