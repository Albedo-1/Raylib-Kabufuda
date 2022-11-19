using System.Numerics;
using Raylib_cs;

namespace Kabufuda
{
	public class GUIButton : Graphic
	{
		private const float TIME_FOR_CLICK = 0.2f;
		private float _clickTimer = 0;

		public delegate void OnMouseClick();
		public OnMouseClick MouseClickEvent;

		// Inputs
		Color hoverColor = new Color(230, 230, 230, 255);
		Color clickColor = new Color(200, 200, 200, 255);
		bool mouseOver = false;
		bool mouseDown = false;

		public GUIButton(Vector2 _pos, Vector2 _size, string _textureFile)
		{
			Position = _pos;
			Size = _size;

			image = Raylib.GenImageColor((int)Size.X, (int)Size.Y, Color.WHITE);

			Image fileImage = Raylib.LoadImage(_textureFile);
			Raylib.ImageDraw(ref image,
					fileImage,
					new Rectangle(0, 0, fileImage.width, fileImage.height),
					new Rectangle(0, 0, Size.X, Size.Y),
					Color.WHITE);

			texture = Raylib.LoadTextureFromImage(image);
		}

		public void Input(MouseInputEvent input)
		{
			if (input.Consumed)
				return;

			var mousePos = input.MousePosition;
			mouseOver = Raylib.CheckCollisionPointRec(mousePos, Rectangle) || mouseDown;

			if (mouseOver)
			{
				if(input.MouseButtonLeft == MouseInputEvent.ButtonState.Pressed)
				{
					mouseDown = true;
					_clickTimer = 0;
				}

				input.ConsumeInput();
			}

			
			if(mouseDown && input.MouseButtonLeft == MouseInputEvent.ButtonState.Released)
			{
				mouseDown = false;

				if(_clickTimer <= TIME_FOR_CLICK)
				{
					MouseClickEvent?.Invoke();
				}
			}
			
		}

		public void Update()
		{
			color = mouseOver ? (mouseDown ? clickColor : hoverColor) : Color.WHITE;

			if (mouseDown)
			{
				_clickTimer += Raylib.GetFrameTime();
			}
		}
	}
}