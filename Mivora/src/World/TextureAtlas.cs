using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Mivora.World;

public class TextureAtlas
{
    public void Load(GraphicsDevice gd)
    {
        BlockDefinitionLoader.Load(gd);
    }

    public Texture2D GetTexture(BlockType bt)
        => BlockDefinitionLoader.GetTexture(bt);

    public Vector4 GetUV(BlockType bt, Face face)
        => new Vector4(0, 0, 1, 1);
}
