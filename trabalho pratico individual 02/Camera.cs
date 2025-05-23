using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;

public class Camera
{
    private Vector2 _position;
    private Viewport _viewport;
    private float _zoom = 0.7f;
    private const float ZoomSpeed = 0.1f;
    private const float MinZoom = 0.5f;
    private const float MaxZoom = 2.0f;

    private const int ScrollBorder = 20;
    private const float MoveSpeed = 20f;

    private int _previousScrollValue;

    private Point _worldSize;

    public Matrix Transform => Matrix.CreateTranslation(new Vector3(-_position, 0f)) *
                               Matrix.CreateScale(_zoom, _zoom, 1f);

    public Camera(Viewport viewport, Point worldSize)
    {
        _viewport = viewport;
        _worldSize = worldSize;
        _position = Vector2.Zero;
        _previousScrollValue = Mouse.GetState().ScrollWheelValue;
    }

    public void Update(GraphicsDevice graphicsDevice)
    {
        _viewport = graphicsDevice.Viewport;

        KeyboardState ks = Keyboard.GetState();
        MouseState ms = Mouse.GetState();

        Vector2 movement = Vector2.Zero;

        if (ks.IsKeyDown(Keys.Left)) movement.X -= 5;
        if (ks.IsKeyDown(Keys.Right)) movement.X += 5;
        if (ks.IsKeyDown(Keys.Up)) movement.Y -= 5;
        if (ks.IsKeyDown(Keys.Down)) movement.Y += 5;

        if (ms.X <= ScrollBorder) movement.X -= 1;
        if (ms.X >= _viewport.Width - ScrollBorder) movement.X += 1;
        if (ms.Y <= ScrollBorder) movement.Y -= 1;
        if (ms.Y >= _viewport.Height - ScrollBorder) movement.Y += 1;

        if (movement != Vector2.Zero)
        {
            movement.Normalize();
            _position += movement * MoveSpeed / _zoom;
        }

        int scroll = ms.ScrollWheelValue;
        if (scroll != _previousScrollValue)
        {
            int delta = scroll - _previousScrollValue;
            _zoom += delta > 0 ? ZoomSpeed : -ZoomSpeed;
            _zoom = MathHelper.Clamp(_zoom, MinZoom, MaxZoom);
            _previousScrollValue = scroll;
        }

        // aplicar limites do mundo
        float viewWidth = _viewport.Width / _zoom;
        float viewHeight = _viewport.Height / _zoom;

        _position.X = MathHelper.Clamp(_position.X, 0, _worldSize.X - viewWidth);
        _position.Y = MathHelper.Clamp(_position.Y, 0, _worldSize.Y - viewHeight);
    }

    public Vector2 ScreenToWorld(Vector2 screenPos)
    {
        return Vector2.Transform(screenPos, Matrix.Invert(Transform));
    }

    public Vector2 WorldToScreen(Vector2 worldPos)
    {
        return Vector2.Transform(worldPos, Transform);
    }
}
