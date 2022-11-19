using System;
using System.Numerics;

using Raylib_cs;

namespace Kabufuda
{
	public class CardStack : Graphic, ICardHolder
	{
		protected const int SPACE_BORDER = 3;

		const int cardDistance = 20;

		public bool Locked { get; private set; } = false;

		protected Texture2D? lockedTexture;

		private Stack<Card> stack = new Stack<Card>();
		public List<Card> Stack {get => stack.ToList(); }

		public CardStack(Vector2 _pos)
		{
			Size = Card.DEFAULT_CARD_SIZE + new Vector2(SPACE_BORDER, SPACE_BORDER) * 2;

			Position = _pos;

			image = Raylib.GenImageColor((int)Size.X, (int)Size.Y, Game.TERTIARY_BG_COLOR);
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

		private int CardPositionInStack(Card card)
		{
			int cardPos = Stack.IndexOf(card);
			cardPos = stack.Count - cardPos - 1;
			return Math.Max(cardPos, 0);
		}

		private bool CheckForStack(Card checkCard)
		{
			if(Stack.Count != 4)
				return false;

			foreach (var item in Stack)
			{
				if(item.Number != checkCard.Number)
					return false;
			}

			return true;
		}

		public override void Render()
		{
			if (texture.HasValue)
			{
				Raylib.DrawTexture(texture.Value, (int)Position.X, (int)Position.Y, color);
			}

			if (Locked && lockedTexture.HasValue)
			{
				//Raylib.DrawRectangleRec(new Rectangle(Position.X + 5, Position.Y + 5, Size.X - 10, Size.Y - 10), Color.BLACK);
				Raylib.DrawTexture(lockedTexture.Value, (int)Position.X, (int)Position.Y, color);
			}
		}


		#region ICardHolder Interface

		public void PositionCardOnHolder(Card card)
		{
			if(!Stack.Contains(card))
			{
				int stackNum = stack.Count - 1;

				card.Position = new Vector2(Position.X + (Rectangle.width - card.Rectangle.width) / 2,
									Position.Y + ((Rectangle.height - card.Rectangle.height) / 2) + (cardDistance * stackNum));
			}
			else
			{
				int stackNum = CardPositionInStack(card);
				
				card.Position = new Vector2(Position.X + (Rectangle.width - card.Rectangle.width) / 2,
					Position.Y + ((Rectangle.height - card.Rectangle.height) / 2) + (cardDistance * stackNum));
			}
			
		}

		public void OnCardActed(Card actedCard)
		{
			bool isNestedCard = false; 
			for (int i = stack.Count - 1; i >= 0; i--)
			{
				if(Stack[i] == actedCard)
				{
					isNestedCard = true;
				}
				else if(isNestedCard && Stack[i].Number == actedCard.Number)
				{
					Stack[i].ParentCard = actedCard;
					actedCard.AddChild(Stack[i]);
				}
			}
		}

		public void AddCard(Card card)
		{
			card.OwnerHolder = this;

			stack.Push(card);

			PositionCardOnHolder(card);

			card.SetLayer(stack.Count);

			if (Game.PlayArea.GameSetup && CheckForStack(card))
			{
				LockHolder();
			}
		}

		public void ReturnCard(Card card)
		{
			PositionCardOnHolder(card);
			card.SetLayer(CardPositionInStack(card) + 1);
		}

		public bool PeekCard(out Card? outCard)
		{
			if (Locked)
			{
				outCard = null;
				return false;
			}

			for (int i = 0; i < stack.Count; i++)
			{
				if(Stack[i] == Card.ActiveCard || Stack[i].ChildCards.Count > 0 || Stack[i].ParentCard != null)
					continue;
				
				outCard = Stack[i];
				return true;
			}

			outCard = null;
			return false;
		}

		public Card PopCard()
		{
			return stack.Pop();
		}

		public void LockHolder()
		{
			Locked = true;
		}

		public void CleanHolder()
		{
			Locked = false;
			stack.Clear();
		}

		#endregion

	}
}