using System;

namespace Mivora.World;

public class WorldGenerator
{
    private const float Scale         = 0.008f;
    private const int   BaseHeight    = 32;
    private const int   HeightRange   = 24;
    private const int   WaterLevel    = 28;
    private const float CaveScale     = 0.04f;
    private const float CaveThreshold = 0.72f;

    public WorldGenerator(int seed)
    {
        NoiseGenerator.Initialize(seed);
    }

    public void GenerateChunk(Chunk chunk)
    {
        int chunkX = (int)chunk.ChunkPos.X;
        int chunkZ = (int)chunk.ChunkPos.Y;

        for (int x = 0; x < Chunk.Width; x++)
        for (int z = 0; z < Chunk.Depth; z++)
        {
            int worldX = chunkX * Chunk.Width  + x;
            int worldZ = chunkZ * Chunk.Depth  + z;

            float continentalness = NoiseGenerator.OctavePerlin(
                worldX * Scale * 0.5f, worldZ * Scale * 0.5f, 4, 0.5f, 2.0f);
            float erosion = NoiseGenerator.OctavePerlin(
                worldX * Scale + 100f, worldZ * Scale + 100f, 3, 0.4f, 2.2f);
            float peaksValleys = NoiseGenerator.OctavePerlin(
                worldX * Scale * 2f + 200f, worldZ * Scale * 2f + 200f, 2, 0.3f, 2.5f);

            float heightNoise = continentalness * 0.6f
                              + erosion         * 0.3f
                              + peaksValleys    * 0.1f;

            int surfaceY   = BaseHeight + (int)(heightNoise * HeightRange);
            surfaceY       = Math.Clamp(surfaceY, 1, Chunk.Height - 2);
            bool nearWater = surfaceY <= WaterLevel + 2;

            for (int y = 0; y < Chunk.Height; y++)
            {
                bool isCave = false;
                if (y < surfaceY - 3 && y > 2)
                {
                    float cave3D = NoiseGenerator.OctavePerlin(
                        worldX * CaveScale,
                        worldZ * CaveScale + y * CaveScale,
                        2, 0.5f, 2f);
                    isCave = cave3D > CaveThreshold;
                }

                if (isCave) { chunk.SetBlock(x, y, z, BlockType.Air); continue; }
                if (y == 0) { chunk.SetBlock(x, y, z, BlockType.Stone); continue; }

                if (y > surfaceY)
                {
                    chunk.SetBlock(x, y, z, y <= WaterLevel ? BlockType.Water : BlockType.Air);
                    continue;
                }

                BlockType placed = BlockType.Stone;

                if (y == surfaceY)
                    placed = nearWater ? BlockType.Sand : BlockType.Grass;
                else if (y >= surfaceY - 3)
                    placed = BlockType.Dirt;
                else
                    placed = BlockType.Stone;

                chunk.SetBlock(x, y, z, placed);
            }
        }
    }
}
