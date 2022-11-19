using System;
using System.Numerics;

using Raylib_cs;

namespace Kabufuda
{
	public class BaseInputEvent
	{
		public bool Consumed { get; private set; }

		public BaseInputEvent()
		{
			Consumed = false;
		}

		public void ConsumeInput()
		{
			Consumed = true;
		}
	}

	public class MouseInputEvent : BaseInputEvent
	{
		public Vector2 MousePosition { get; private set; }

		public ButtonState? MouseButtonLeft { get; private set; }

		public enum ButtonState
		{
			None = -1,
			Pressed,
			Down,
			Released,
			Up,
		}

		public bool MouseButtonLeftDown { get => MouseButtonLeft == ButtonState.Pressed || MouseButtonLeft == ButtonState.Down; }

		public MouseInputEvent()
		{
			MousePosition = Raylib.GetMousePosition();

			MouseButtonLeft = GetMouseState(MouseButton.MOUSE_BUTTON_LEFT);
		}
		
		private static ButtonState GetMouseState(MouseButton button)
		{
			if(Raylib.IsMouseButtonPressed(button))
			{
				return ButtonState.Pressed;
			}
			if(Raylib.IsMouseButtonDown(button))
			{
				return ButtonState.Down;
			}
			if(Raylib.IsMouseButtonReleased(button))
			{
				return ButtonState.Released;
			}
			if(Raylib.IsMouseButtonUp(button))
			{
				return ButtonState.Up;
			}

			return ButtonState.None;
		}

	}
}