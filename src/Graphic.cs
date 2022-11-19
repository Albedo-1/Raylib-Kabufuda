using System;
using System.Numerics;

using Raylib_cs;

public class Graphic : IDisposable
{

	public Vector2 Position = Vector2.Zero;
	protected Vector2 Size = Vector2.One;

	public Rectangle Rectangle => new Rectangle(Position.X, Position.Y, Size.X, Size.Y);

	protected Color color = Color.WHITE;
	protected Image image;
	protected Texture2D? texture;


	// 0 = Background
	// 1 - 40 = Cards in Play Area
	// 100 - 104 = Active Cards
	public int Layer { get; protected set; } = 0;

	~Graphic()
	{
		if(texture.HasValue)
		{
			Dispose();
		}
	}

	public virtual void Render()
	{
		if(texture.HasValue)
		{
			Raylib.DrawTexture(texture.Value, (int)Position.X, (int)Position.Y, color);
		}
	}

	public void Dispose()
	{
		Raylib.UnloadImage(image);

		if (texture.HasValue)
		{
			Raylib.UnloadTexture(texture.Value);
			texture = null;
		}
			
	}
}