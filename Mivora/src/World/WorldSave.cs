using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Mivora.World;

public class WorldInfo
{
    public string Name       { get; set; }
    public string GenType    { get; set; }
    public string DropRate   { get; set; }
    public string LastPlayed { get; set; }
}

public static class WorldSaveManager
{
    private static string SaveDir = "Saves";

    public static void EnsureSaveDir() => Directory.CreateDirectory(SaveDir);

    public static List<WorldInfo> GetWorlds()
    {
        EnsureSaveDir();
        var worlds = new List<WorldInfo>();
        foreach (var dir in Directory.GetDirectories(SaveDir))
        {
            var infoPath = Path.Combine(dir, "world.json");
            if (File.Exists(infoPath))
            {
                var json = File.ReadAllText(infoPath);
                var info = JsonSerializer.Deserialize<WorldInfo>(json);
                if (info != null) worlds.Add(info);
            }
        }
        return worlds;
    }

    public static void SaveWorld(WorldInfo info)
    {
        EnsureSaveDir();
        var dir = Path.Combine(SaveDir, SanitizeName(info.Name));
        Directory.CreateDirectory(dir);
        info.LastPlayed = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
        var json = JsonSerializer.Serialize(info, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(Path.Combine(dir, "world.json"), json);
    }

    private static string SanitizeName(string name)
        => string.Join("_", name.Split(Path.GetInvalidFileNameChars()));
}
