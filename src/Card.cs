using System.Numerics;

using Raylib_cs;

namespace Kabufuda
{
	public class Card : Graphic, IDisposable 
	{
		// Cards are 3:5 aspect
		private static Vector2 _default_card_size = new Vector2(90, 150);
		public static Vector2 DEFAULT_CARD_SIZE => _default_card_size;

		private Guid _id;
		public Guid Id => _id;
		public static Card? ActiveCard;

		public void SetLayer(int value) => Layer = value;

		private bool acted = false;
		int number;
		public int Number { get => number; }

		Color hoverColor = new Color(230, 230, 230, 255);
		Color clickColor = new Color(200, 200, 200, 255);

		// Inputs
		bool mouseOver = false;
		bool mouseDown = false;
		Vector2 mouseDelta = Vector2.Zero;
		public Vector2 MouseDelta { get => mouseDelta; }
		Vector2 prevMousePos = Vector2.Zero;

		public List<Card> ChildCards = new List<Card>();
		public void AddChild(Card card)
		{
			card.Layer = Layer + 1 + ChildCards.Count;
			ChildCards.Add(card);
		}
		public Card? ParentCard;
		ICardHolder? ownerHolder;
		public ICardHolder? OwnerHolder { get => ownerHolder; set => ownerHolder = value; }

		public Card(int _num, Vector2 _pos)
		{
			_id = Guid.NewGuid();
			Layer = 1;
			Size = DEFAULT_CARD_SIZE;

			number = _num;
			Position = _pos;

			image = Raylib.GenImageColor((int)Size.X, (int)Size.Y, Color.WHITE);

			Image fileImage = Raylib.LoadImage($"img/Card{number}.png");
			Raylib.ImageDraw(ref image,
					fileImage,
					new Rectangle(0, 0, fileImage.width, fileImage.height),
					new Rectangle(0, 0, Size.X, Size.Y),
					Color.WHITE);

			texture = Raylib.LoadTextureFromImage(image);

			Raylib.UnloadImage(fileImage);
		}


		public void Input(MouseInputEvent input)
		{
			if((ActiveCard != null && ActiveCard != this))
			{
				return;
			}
			
			if(input.Consumed)
			{
				ResetInputs();
				return;
			}

			var mousePos = input.MousePosition;
			mouseOver = Raylib.CheckCollisionPointRec(mousePos, Rectangle) || mouseDown;

			if(mouseOver)
			{
				// Only act if the Card recognized a Pressed Event
				if(!acted && input.MouseButtonLeft == MouseInputEvent.ButtonState.Pressed)
				{
					acted = true;
					Layer = 100;
					ownerHolder?.OnCardActed(this);
				}

				mouseDown = input.MouseButtonLeftDown;

				if(acted)
				{
					if(mouseDown)
					{
						if (input.MouseButtonLeft == MouseInputEvent.ButtonState.Pressed)
						{
							mouseDelta = Vector2.Zero;
						}
						else
						{
							mouseDelta = mousePos - prevMousePos;
						}
						prevMousePos = mousePos;

						if (ActiveCard != this)
							ActiveCard = this;

					}
					else if (input.MouseButtonLeft == MouseInputEvent.ButtonState.Released)
					{
						OnMouseReleased();
					}

				}
				
				input.ConsumeInput();
			}
			else 
			{
				ResetInputs();
			}
		}

		private void ResetInputs()
		{
			if (mouseDelta != Vector2.Zero)
			{
				mouseDelta = Vector2.Zero;
			}

			if(mouseOver)
			{
				mouseDown = false;
				mouseOver = false;
				acted = false;
			}
		}

		private void OnMouseReleased()
		{
			if (IsPlacementValid(out ICardHolder? newOwner))
			{
				UpdateAndPositionToNewOwner(newOwner!);
			}
			else
			{
				ownerHolder?.ReturnCard(this);

				foreach (var child in ChildCards)
				{
					child.SetToParentHolder(false);
				}
			}

			ResetInputs();

			ChildCards.Clear();
		}

		private bool IsPlacementValid(out ICardHolder? holder)
		{
			// Look for Free Spaces
			// We are moving just this card or a full stack
			if(ChildCards.Count >= 2 || (ChildCards.Count == 0 && ParentCard == null))
			{
				foreach (var space in Game.PlayArea.FreeSpaces)
				{
					if(space.Locked)
						continue;

					// Is Space Empty, or if the card in the space matches
					if (!space.PeekCard(out Card? heldCard) && (ChildCards.Count == 0 || ChildCards.Count == 3) || 
						(heldCard != null && heldCard.Number == Number && ChildCards.Count == 2))
					{
						if (Raylib.CheckCollisionRecs(space.Rectangle, Rectangle))
						{
							holder = space;
							return true;
						}
					}
					else
					{
						continue;
					}
				}
			}

			// Look for Card Stacks
			Card? topCard = null;
			Rectangle checkRect;
			foreach (var stack in Game.PlayArea.CardStacks)
			{
				if (!stack.Locked && !stack.PeekCard(out topCard))
				{
					checkRect = stack.Rectangle;
				}
				else
				{
					// Ignore the top card if it is this card or doesn't match in number
					if(topCard == this || topCard?.Number != number)
						continue;
					
					checkRect = topCard!.Rectangle;
				}

				if (Raylib.CheckCollisionRecs(checkRect, Rectangle))
				{
					// Skip if it is the same as the current Holder
					if(ownerHolder == stack)
						continue;

					holder = stack;
					return true;
				}
			}

			holder = ownerHolder;
			return false;
		}

		private bool CheckForFullStack(ICardHolder newOwner)
		{
			bool spaceFilled = newOwner.PeekCard(out Card peekCard);

			// If it is a FreeSpace, contains a card with a matching number, and is currently a 3 stack
			if (spaceFilled && newOwner.GetType() == typeof(FreeSpace) && peekCard!.Number == Number && ChildCards.Count == 2)
			{
				return true;
			}
			// If it is an empty card stack with 4 cards
			else if (ChildCards.Count == 3 && !spaceFilled)
			{
				return true;
			}

			return false;
		}

		private void UpdateAndPositionToNewOwner(ICardHolder newOwner)
		{
			if (CheckForFullStack(newOwner))
			{
				newOwner.LockHolder();
			}

			ownerHolder?.PopCard();

			newOwner?.AddCard(this);

			ownerHolder = newOwner;

			foreach (var child in ChildCards)
			{
				child.SetToParentHolder(true, newOwner);
			}
		}

		public void SetToParentHolder(bool add, ICardHolder? newOwner = null )
		{
			if(add)
			{
				if(newOwner != null)
				{
					UpdateAndPositionToNewOwner(newOwner);
				}
				else
				{
					Console.WriteLine("New Owner is null, but the Parent wants to add this card to one!");
					throw new Exception();
				}
			}
			else
			{
				ownerHolder?.ReturnCard(this);
			}
		}

		public void Update()
		{
			color = mouseOver ? (mouseDown ? clickColor : hoverColor) : Color.WHITE;

			if(ParentCard != null)
			{
				Position += ParentCard.MouseDelta;
			}
			if(mouseDown)
			{
				Position += mouseDelta;
			}
		}

		public override void Render()
		{
			if(texture.HasValue)
			{
				Raylib.DrawTexture(texture.Value, (int)Position.X, (int)Position.Y, color);
			}
		}

	}
}