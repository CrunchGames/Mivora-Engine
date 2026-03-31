using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Mivora.Input;

namespace Mivora.Graphics;

public class Camera
{
    public Matrix View;
    public Matrix Projection;

    public float Yaw;
    public float Pitch;

    private float _distance       = 6f;
    private float _minDistance    = 2f;
    private float _maxDistance    = 20f;
    private float _sensitivity    = 0.003f;
    private float _scrollSpeed    = 1f;
    private float _shoulderOffset = 0.6f;
    private bool  _firstPerson    = false;

    private Vector3       _currentPos;
    private float         _posLerp    = 12f;
    private MouseState    _prevMouse;
    private KeyboardState _prevKey;
    private bool          _firstFrame = true;

    public bool IsFirstPerson => _firstPerson;

    public void Initialize(GraphicsDevice gd)
    {
        Projection = Matrix.CreatePerspectiveFieldOfView(
            MathHelper.ToRadians(60),
            gd.Viewport.AspectRatio,
            0.1f, 1000f);
    }

    public void UpdateProjection(GraphicsDevice gd, float fovDegrees)
    {
        Projection = Matrix.CreatePerspectiveFieldOfView(
            MathHelper.ToRadians(fovDegrees),
            gd.Viewport.AspectRatio,
            0.1f, 1000f);
    }

    public void Update(GraphicsDevice gd, Vector3 playerPos)
    {
        var mouse = Mouse.GetState();
        var kb    = Keyboard.GetState();
        var km    = KeybindManager.Instance;

        var center = new Point(gd.Viewport.Width / 2, gd.Viewport.Height / 2);

        if (_firstFrame)
        {
            Mouse.SetPosition(center.X, center.Y);
            _prevMouse  = Mouse.GetState();
            _currentPos = playerPos;
            _firstFrame = false;
            _prevKey    = kb;
            return;
        }

        if (kb.IsKeyDown(km.Get(GameAction.TogglePerson)) &&
            !_prevKey.IsKeyDown(km.Get(GameAction.TogglePerson)))
            _firstPerson = !_firstPerson;

        int dx = mouse.X - center.X;
        int dy = mouse.Y - center.Y;

        Yaw   -= dx * _sensitivity;
        Pitch -= dy * _sensitivity;
        Pitch  = Math.Clamp(Pitch,
            MathHelper.ToRadians(-80),
            MathHelper.ToRadians(45));

        Mouse.SetPosition(center.X, center.Y);

        int scroll = mouse.ScrollWheelValue - _prevMouse.ScrollWheelValue;
        _distance -= scroll * 0.01f * _scrollSpeed;
        _distance  = Math.Clamp(_distance, _minDistance, _maxDistance);

        float dt    = 1f / 60f;
        _currentPos = Vector3.Lerp(_currentPos, playerPos, _posLerp * dt);
        var target  = _currentPos + Vector3.Up * 1.6f;

        if (_firstPerson)
        {
            var eyeOffset = new Vector3(MathF.Sin(Yaw) * 0.1f, 0, MathF.Cos(Yaw) * 0.1f);
            var eyePos    = _currentPos + Vector3.Up * 1.7f - eyeOffset;
            var lookDir   = new Vector3(
                -MathF.Sin(Yaw)   * MathF.Cos(Pitch),
                 MathF.Sin(Pitch),
                -MathF.Cos(Yaw)   * MathF.Cos(Pitch));
            View = Matrix.CreateLookAt(eyePos, eyePos + lookDir, Vector3.Up);
        }
        else
        {
            var camOffset = new Vector3(
                 MathF.Sin(Yaw)   * MathF.Cos(Pitch),
                -MathF.Sin(Pitch),
                 MathF.Cos(Yaw)   * MathF.Cos(Pitch)) * _distance;

            var right  = Vector3.Cross(Vector3.Up, Vector3.Normalize(camOffset));
            var camPos = target + camOffset + right * _shoulderOffset;
            View = Matrix.CreateLookAt(camPos, target, Vector3.Up);
        }

        _prevMouse = mouse;
        _prevKey   = kb;
    }
}
