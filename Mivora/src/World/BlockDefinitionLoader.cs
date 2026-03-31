using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Microsoft.Xna.Framework.Graphics;
using Mivora.Graphics;

namespace Mivora.World;

public static class BlockDefinitionLoader
{
    private static Dictionary<int, BlockDefinition> _definitions = new();
    private static Dictionary<int, Texture2D>       _textures    = new();
    private static Dictionary<int, MivoraModel>    _models      = new();

    public static IReadOnlyDictionary<int, BlockDefinition> Definitions => _definitions;

    public static void Load(GraphicsDevice gd, string jsonPath = "Content/World/blocks.json")
    {
        if (!File.Exists(jsonPath))
        {
            Console.WriteLine($"[BlockDef] blocks.json not found at {jsonPath}");
            return;
        }

        var json     = File.ReadAllText(jsonPath);
        var registry = JsonSerializer.Deserialize<BlockRegistry>(json);
        if (registry == null) return;

        foreach (var def in registry.Blocks)
        {
            _definitions[def.Id] = def;

            if (File.Exists(def.TexturePath))
            {
                using var stream = File.OpenRead(def.TexturePath);
                _textures[def.Id] = Texture2D.FromStream(gd, stream);
                Console.WriteLine($"[BlockDef] Loaded texture: {def.Name}");
            }
            else
                Console.WriteLine($"[BlockDef] Missing texture: {def.TexturePath}");

            if (File.Exists(def.ObjPath))
            {
                _models[def.Id] = ObjModelLoader.Load(gd, def.ObjPath,
                    _textures.TryGetValue(def.Id, out var t) ? t : null);
                Console.WriteLine($"[BlockDef] Loaded OBJ: {def.Name}");
            }
        }
    }

    public static BlockDefinition GetDefinition(int id)
        => _definitions.TryGetValue(id, out var d) ? d : null;
    public static Texture2D GetTexture(int id)
        => _textures.TryGetValue(id, out var t) ? t : null;
    public static MivoraModel GetModel(int id)
        => _models.TryGetValue(id, out var m) ? m : null;

    public static BlockDefinition GetDefinition(BlockType bt) => GetDefinition((int)bt);
    public static Texture2D       GetTexture(BlockType bt)    => GetTexture((int)bt);
    public static MivoraModel    GetModel(BlockType bt)      => GetModel((int)bt);
}
