using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Mivora.UI;

public class InputState
{
    private MouseState _prev;
    private MouseState _curr;

    public void Update()
    {
        _prev = _curr;
        _curr = Mouse.GetState();
    }

    public bool JustClicked =>
        _curr.LeftButton == ButtonState.Released &&
        _prev.LeftButton == ButtonState.Pressed;

    public Point MousePos => _curr.Position;
}
