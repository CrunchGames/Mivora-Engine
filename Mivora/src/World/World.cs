using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Mivora.World;

public class MivoraWorld
{
    private Dictionary<Vector2, Chunk> _chunks        = new();
    private TextureAtlas               _atlas          = new();
    private WorldGenerator             _generator;
    private int                        _renderDistance = 4;
    private int                        _seed;

    public int RenderDistance => _renderDistance;
    public int Seed           => _seed;

    public void Initialize(GraphicsDevice gd, int seed = 0)
    {
        _seed      = seed == 0 ? new Random().Next() : seed;
        _generator = new WorldGenerator(_seed);
        _atlas.Load(gd);
    }

    public void SetRenderDistance(int distance) => _renderDistance = distance;

    public int GetSurfaceY(int worldX, int worldZ)
    {
        int cx  = (int)MathF.Floor((float)worldX / Chunk.Width);
        int cz  = (int)MathF.Floor((float)worldZ / Chunk.Depth);
        var key = new Vector2(cx, cz);

        if (!_chunks.TryGetValue(key, out var chunk)) return 0;

        int lx = Math.Clamp(worldX - cx * Chunk.Width,  0, Chunk.Width  - 1);
        int lz = Math.Clamp(worldZ - cz * Chunk.Depth,  0, Chunk.Depth  - 1);

        for (int y = Chunk.Height - 1; y >= 0; y--)
        {
            var block = chunk.GetBlock(lx, y, lz);
            if (block != BlockType.Air && block != BlockType.Water)
                return y;
        }

        return 0;
    }

    public void Update(GraphicsDevice gd, Vector3 playerPos)
    {
        int cx = (int)MathF.Floor(playerPos.X / Chunk.Width);
        int cz = (int)MathF.Floor(playerPos.Z / Chunk.Depth);

        for (int x = cx - _renderDistance; x <= cx + _renderDistance; x++)
        for (int z = cz - _renderDistance; z <= cz + _renderDistance; z++)
        {
            var key = new Vector2(x, z);
            if (!_chunks.ContainsKey(key))
            {
                var chunk = new Chunk { ChunkPos = key };
                _generator.GenerateChunk(chunk);
                chunk.BuildMesh(gd, _atlas);
                _chunks[key] = chunk;
            }
        }

        var toRemove = new List<Vector2>();
        foreach (var key in _chunks.Keys)
            if (MathF.Abs(key.X - cx) > _renderDistance + 1 ||
                MathF.Abs(key.Y - cz) > _renderDistance + 1)
                toRemove.Add(key);
        foreach (var key in toRemove) _chunks.Remove(key);
    }

    public void Draw(GraphicsDevice gd, BasicEffect effect, Matrix view, Matrix projection)
    {
        gd.SamplerStates[0] = SamplerState.PointClamp;
        effect.View          = view;
        effect.Projection    = projection;

        foreach (var (key, chunk) in _chunks)
        {
            var worldOffset = new Vector3(key.X * Chunk.Width, 0, key.Y * Chunk.Depth);
            chunk.Draw(gd, effect, worldOffset, _atlas);
        }
    }
}
