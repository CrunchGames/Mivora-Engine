using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Mivora.Graphics;

public struct ModelVertex : IVertexType
{
    public Vector3 Position;
    public Vector3 Normal;
    public Vector2 TexCoord;

    public static readonly VertexDeclaration VertexDeclaration = new(
        new VertexElement(0,  VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
        new VertexElement(12, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
        new VertexElement(24, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0)
    );

    VertexDeclaration IVertexType.VertexDeclaration => VertexDeclaration;
}

public class MivoraModel
{
    public VertexBuffer VertexBuffer;
    public IndexBuffer  IndexBuffer;
    public int          IndexCount;
    public Texture2D    Texture;
}

public static class ObjModelLoader
{
    public static MivoraModel Load(GraphicsDevice gd, string objPath, Texture2D texture = null)
    {
        var positions = new List<Vector3>();
        var normals   = new List<Vector3>();
        var uvs       = new List<Vector2>();
        var vertices  = new List<ModelVertex>();
        var indices   = new List<int>();
        var indexMap  = new Dictionary<string, int>();

        foreach (var raw in File.ReadLines(objPath))
        {
            var line = raw.Trim();
            if (line.StartsWith("v "))
            {
                var p = ParseFloats(line, 1);
                positions.Add(new Vector3(p[0], p[1], p[2]));
            }
            else if (line.StartsWith("vn "))
            {
                var p = ParseFloats(line, 1);
                normals.Add(new Vector3(p[0], p[1], p[2]));
            }
            else if (line.StartsWith("vt "))
            {
                var p = ParseFloats(line, 1);
                uvs.Add(new Vector2(p[0], 1f - p[1]));
            }
            else if (line.StartsWith("f "))
            {
                var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                var faceIndices = new List<int>();
                for (int i = 1; i < parts.Length; i++)
                {
                    var key = parts[i];
                    if (!indexMap.TryGetValue(key, out int idx))
                    {
                        idx = vertices.Count;
                        indexMap[key] = idx;
                        vertices.Add(BuildVertex(key, positions, normals, uvs));
                    }
                    faceIndices.Add(idx);
                }
                for (int i = 1; i < faceIndices.Count - 1; i++)
                {
                    indices.Add(faceIndices[0]);
                    indices.Add(faceIndices[i]);
                    indices.Add(faceIndices[i + 1]);
                }
            }
        }

        var vb = new VertexBuffer(gd, ModelVertex.VertexDeclaration, vertices.Count, BufferUsage.WriteOnly);
        vb.SetData(vertices.ToArray());

        var ib = new IndexBuffer(gd, IndexElementSize.ThirtyTwoBits, indices.Count, BufferUsage.WriteOnly);
        ib.SetData(indices.ToArray());

        return new MivoraModel { VertexBuffer = vb, IndexBuffer = ib, IndexCount = indices.Count, Texture = texture };
    }

    static ModelVertex BuildVertex(string token, List<Vector3> pos, List<Vector3> nrm, List<Vector2> uvs)
    {
        var parts = token.Split('/');
        var v = new ModelVertex();
        v.Position = pos[int.Parse(parts[0]) - 1];
        if (parts.Length > 1 && parts[1] != "") v.TexCoord = uvs[int.Parse(parts[1]) - 1];
        if (parts.Length > 2 && parts[2] != "") v.Normal   = nrm[int.Parse(parts[2]) - 1];
        return v;
    }

    static float[] ParseFloats(string line, int skip)
    {
        var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var result = new float[parts.Length - skip];
        for (int i = 0; i < result.Length; i++)
            result[i] = float.Parse(parts[i + skip], System.Globalization.CultureInfo.InvariantCulture);
        return result;
    }
}
