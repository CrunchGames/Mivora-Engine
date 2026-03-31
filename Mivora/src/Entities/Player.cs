using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Mivora.Input;
using Mivora.World;

namespace Mivora.Entities;

public class Player
{
    public Vector3 Position;
    public float   Yaw;
    public float   Speed       = 5f;
    public float   SprintSpeed = 9f;

    private float       _velocityY    = 0f;
    private const float Gravity       = -20f;
    private const float JumpForce     =  8f;
    private bool        _isGrounded   = false;
    private float       _visualYaw    = 0f;
    private const float RotationSpeed = 10f;

    public float VisualYaw => _visualYaw;

    public void Update(GameTime gameTime, float cameraYaw, MivoraWorld world)
    {
        Yaw = cameraYaw;
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        var   kb = Keyboard.GetState();
        var   km = KeybindManager.Instance;

        bool  sprinting = kb.IsKeyDown(km.Get(GameAction.Sprint));
        float speed     = sprinting ? SprintSpeed : Speed;

        var forward = new Vector3(-MathF.Sin(cameraYaw), 0, -MathF.Cos(cameraYaw));
        var right   = new Vector3(forward.Z, 0, -forward.X);

        var move = Vector3.Zero;
        if (kb.IsKeyDown(km.Get(GameAction.MoveForward)))  move += forward;
        if (kb.IsKeyDown(km.Get(GameAction.MoveBackward))) move -= forward;
        if (kb.IsKeyDown(km.Get(GameAction.MoveLeft)))     move -= right;
        if (kb.IsKeyDown(km.Get(GameAction.MoveRight)))    move += right;

        if (move.LengthSquared() > 0)
        {
            move = Vector3.Normalize(move) * speed * dt;

            float targetYaw = MathF.Atan2(-move.X, -move.Z);
            float diff      = targetYaw - _visualYaw;
            while (diff >  MathF.PI) diff -= MathF.PI * 2f;
            while (diff < -MathF.PI) diff += MathF.PI * 2f;
            _visualYaw += diff * MathF.Min(RotationSpeed * dt, 1f);
        }

        Position += move;

        _velocityY += Gravity * dt;
        Position   += new Vector3(0, _velocityY * dt, 0);

        int   groundY = world.GetSurfaceY((int)MathF.Floor(Position.X),
                                           (int)MathF.Floor(Position.Z));
        float feetY   = groundY + 1f;

        if (Position.Y <= feetY)
        {
            Position.Y  = feetY;
            _velocityY  = 0f;
            _isGrounded = true;
        }
        else
        {
            _isGrounded = false;
        }

        if (kb.IsKeyDown(km.Get(GameAction.Jump)) && _isGrounded)
        {
            _velocityY  = JumpForce;
            _isGrounded = false;
        }
    }
}
