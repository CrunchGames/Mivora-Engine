using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Mivora.World;

public class Chunk
{
    public const int Width  = 16;
    public const int Height = 64;
    public const int Depth  = 16;

    public Vector2 ChunkPos;
    private BlockType[,,] _blocks = new BlockType[Width, Height, Depth];

    private Dictionary<BlockType, (VertexBuffer vb, IndexBuffer ib, int count)> _meshes = new();
    private bool _dirty = true;

    public void SetBlock(int x, int y, int z, BlockType type)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height || z < 0 || z >= Depth) return;
        _blocks[x, y, z] = type;
        _dirty = true;
    }

    public BlockType GetBlock(int x, int y, int z)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height || z < 0 || z >= Depth)
            return BlockType.Air;
        return _blocks[x, y, z];
    }

    public void BuildMesh(GraphicsDevice gd, TextureAtlas atlas)
    {
        if (!_dirty) return;
        _meshes.Clear();

        var vertMap  = new Dictionary<BlockType, List<VertexPositionTexture>>();
        var indexMap = new Dictionary<BlockType, List<int>>();

        foreach (BlockType bt in new[] {
            BlockType.Grass, BlockType.Dirt, BlockType.Stone,
            BlockType.Sand,  BlockType.Water })
        {
            vertMap[bt]  = new List<VertexPositionTexture>();
            indexMap[bt] = new List<int>();
        }

        for (int x = 0; x < Width;  x++)
        for (int y = 0; y < Height; y++)
        for (int z = 0; z < Depth;  z++)
        {
            BlockType block = _blocks[x, y, z];
            if (block == BlockType.Air || block == BlockType.Placeholder) continue;
            if (!vertMap.ContainsKey(block)) continue;

            var pos = new Vector3(x, y, z);

            void TryFace(int nx, int ny, int nz, Face face)
            {
                var neighbour = GetBlock(nx, ny, nz);
                if (neighbour != BlockType.Air &&
                    neighbour != BlockType.Water &&
                    neighbour != BlockType.Placeholder) return;

                BlockType meshKey = block switch
                {
                    BlockType.Grass when face != Face.Top    => BlockType.Dirt,
                    BlockType.Grass when face == Face.Bottom => BlockType.Dirt,
                    _ => block
                };

                if (!vertMap.ContainsKey(meshKey)) return;
                AddFace(vertMap[meshKey], indexMap[meshKey], pos, face,
                        atlas.GetUV(meshKey, face));
            }

            TryFace(x, y + 1, z, Face.Top);
            TryFace(x, y - 1, z, Face.Bottom);
            TryFace(x + 1, y, z, Face.Right);
            TryFace(x - 1, y, z, Face.Left);
            TryFace(x, y, z + 1, Face.Front);
            TryFace(x, y, z - 1, Face.Back);
        }

        foreach (BlockType bt in new[] {
            BlockType.Grass, BlockType.Dirt, BlockType.Stone,
            BlockType.Sand,  BlockType.Water })
        {
            var verts   = vertMap[bt];
            var indices = indexMap[bt];
            if (verts.Count == 0) continue;

            var vb = new VertexBuffer(gd, VertexPositionTexture.VertexDeclaration,
                                      verts.Count, BufferUsage.WriteOnly);
            vb.SetData(verts.ToArray());

            var ib = new IndexBuffer(gd, IndexElementSize.ThirtyTwoBits,
                                     indices.Count, BufferUsage.WriteOnly);
            ib.SetData(indices.ToArray());

            _meshes[bt] = (vb, ib, indices.Count);
        }

        _dirty = false;
    }

    public void Draw(GraphicsDevice gd, BasicEffect effect,
                     Vector3 worldOffset, TextureAtlas atlas)
    {
        effect.World = Matrix.CreateTranslation(worldOffset);

        foreach (BlockType bt in new[] {
            BlockType.Grass, BlockType.Dirt, BlockType.Stone,
            BlockType.Sand,  BlockType.Water })
        {
            if (!_meshes.ContainsKey(bt)) continue;
            var (vb, ib, count) = _meshes[bt];
            if (count == 0) continue;

            effect.Texture        = atlas.GetTexture(bt);
            effect.TextureEnabled = effect.Texture != null;

            gd.SetVertexBuffer(vb);
            gd.Indices = ib;

            foreach (var pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, count / 3);
            }
        }
    }

    static void AddFace(List<VertexPositionTexture> verts, List<int> indices,
                        Vector3 pos, Face face, Vector4 uv)
    {
        int i = verts.Count;
        float u0 = uv.X, v0 = uv.Y, u1 = uv.Z, v1 = uv.W;

        switch (face)
        {
            case Face.Top:
                verts.Add(new VertexPositionTexture(pos + new Vector3(0,1,0), new Vector2(u0,v0)));
                verts.Add(new VertexPositionTexture(pos + new Vector3(1,1,0), new Vector2(u1,v0)));
                verts.Add(new VertexPositionTexture(pos + new Vector3(1,1,1), new Vector2(u1,v1)));
                verts.Add(new VertexPositionTexture(pos + new Vector3(0,1,1), new Vector2(u0,v1)));
                break;
            case Face.Bottom:
                verts.Add(new VertexPositionTexture(pos + new Vector3(0,0,1), new Vector2(u0,v0)));
                verts.Add(new VertexPositionTexture(pos + new Vector3(1,0,1), new Vector2(u1,v0)));
                verts.Add(new VertexPositionTexture(pos + new Vector3(1,0,0), new Vector2(u1,v1)));
                verts.Add(new VertexPositionTexture(pos + new Vector3(0,0,0), new Vector2(u0,v1)));
                break;
            case Face.Front:
                verts.Add(new VertexPositionTexture(pos + new Vector3(0,0,1), new Vector2(u0,v1)));
                verts.Add(new VertexPositionTexture(pos + new Vector3(1,0,1), new Vector2(u1,v1)));
                verts.Add(new VertexPositionTexture(pos + new Vector3(1,1,1), new Vector2(u1,v0)));
                verts.Add(new VertexPositionTexture(pos + new Vector3(0,1,1), new Vector2(u0,v0)));
                break;
            case Face.Back:
                verts.Add(new VertexPositionTexture(pos + new Vector3(1,0,0), new Vector2(u0,v1)));
                verts.Add(new VertexPositionTexture(pos + new Vector3(0,0,0), new Vector2(u1,v1)));
                verts.Add(new VertexPositionTexture(pos + new Vector3(0,1,0), new Vector2(u1,v0)));
                verts.Add(new VertexPositionTexture(pos + new Vector3(1,1,0), new Vector2(u0,v0)));
                break;
            case Face.Right:
                verts.Add(new VertexPositionTexture(pos + new Vector3(1,0,1), new Vector2(u0,v1)));
                verts.Add(new VertexPositionTexture(pos + new Vector3(1,0,0), new Vector2(u1,v1)));
                verts.Add(new VertexPositionTexture(pos + new Vector3(1,1,0), new Vector2(u1,v0)));
                verts.Add(new VertexPositionTexture(pos + new Vector3(1,1,1), new Vector2(u0,v0)));
                break;
            case Face.Left:
                verts.Add(new VertexPositionTexture(pos + new Vector3(0,0,0), new Vector2(u0,v1)));
                verts.Add(new VertexPositionTexture(pos + new Vector3(0,0,1), new Vector2(u1,v1)));
                verts.Add(new VertexPositionTexture(pos + new Vector3(0,1,1), new Vector2(u1,v0)));
                verts.Add(new VertexPositionTexture(pos + new Vector3(0,1,0), new Vector2(u0,v0)));
                break;
        }

        indices.AddRange(new[] { i, i+1, i+2, i, i+2, i+3 });
    }
}

public enum Face { Top, Bottom, Front, Back, Left, Right }
