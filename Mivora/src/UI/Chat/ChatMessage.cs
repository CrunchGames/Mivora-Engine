using Microsoft.Xna.Framework;

namespace Mivora.UI.Chat;

public class ChatMessage
{
    public string Text;
    public Color  Color;
    public float  Age;
    public const float FadeAfter = 6f;
    public const float MaxAge    = 8f;

    public ChatMessage(string text, Color color)
    {
        Text  = text;
        Color = color;
        Age   = 0f;
    }

    public float Alpha => Age < FadeAfter ? 1f :
        1f - (Age - FadeAfter) / (MaxAge - FadeAfter);

    public bool Expired => Age >= MaxAge;
}
