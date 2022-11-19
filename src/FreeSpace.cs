using System.Numerics;
using Raylib_cs;

namespace Kabufuda
{
	public class FreeSpace : Graphic, ICardHolder
	{
		protected const int SPACE_BORDER = 3;

		public bool Locked {get; private set;} = false;

		private Card? heldCard;

		protected Texture2D? lockedTexture;

		public FreeSpace(Vector2 _pos)
		{
			Layer = 0;
			Size = Card.DEFAULT_CARD_SIZE + new Vector2(SPACE_BORDER, SPACE_BORDER) * 2;

			Position = _pos;

			image = Raylib.GenImageColor((int)Size.X, (int)Size.Y, Color.RED);
			Raylib.ImageDrawRectangleRec(ref image, new Rectangle(5, 5, Size.X - 10, Size.Y - 10), Color.WHITE);

			texture = Raylib.LoadTextureFromImage(image);

			Image stackImage = Raylib.LoadImage($"img/CompleteStack.png");
			Image lockedImage = Raylib.GenImageColor((int)Size.X, (int)Size.Y, Color.WHITE);
			Raylib.ImageDraw(ref lockedImage,
					stackImage,
					new Rectangle(0, 0, stackImage.width, stackImage.height),
					new Rectangle(0, 0, Size.X, Size.Y),
					Color.WHITE);
			lockedTexture = Raylib.LoadTextureFromImage(lockedImage);
		}

		public override void Render()
		{
			if (texture.HasValue)
			{
				Raylib.DrawTexture(texture.Value, (int)Position.X, (int)Position.Y, color);
			}

			if(Locked && lockedTexture.HasValue)
			{
				//Raylib.DrawRectangleRec(new Rectangle(Position.X + 5, Position.Y + 5, Size.X - 10, Size.Y - 10), Color.BLACK);
				Raylib.DrawTexture(lockedTexture.Value, (int)Position.X, (int)Position.Y, color);
			}
		}

		#region ICardHolder Interface

		public void PositionCardOnHolder(Card card)
		{
			card.Position = new Vector2(Position.X + (Rectangle.width - card.Rectangle.width) / 2,
								Position.Y + ((Rectangle.height - card.Rectangle.height) / 2));
		}

		public void OnCardActed(Card actedCard)
		{
			// Free Spaces don't need to act
			return;
		}

		public void AddCard(Card card)
		{
			heldCard = card;

			PositionCardOnHolder(card);
			card.SetLayer(1);
		}

		public bool PeekCard(out Card? outCard)
		{
			if(Locked)
			{
				outCard = null;
				return false;
			}

			outCard = heldCard;
			return heldCard != null;
		}

		public Card? PopCard()
		{
			heldCard = null;
			return null;
		}

		public void ReturnCard(Card card)
		{
			PositionCardOnHolder(card);
			card.SetLayer(1);
		}

		public void LockHolder()
		{
			Locked = true;
		}

		public void CleanHolder()
		{
			heldCard = null;
			Locked = false;
		}

		#endregion

	}
}