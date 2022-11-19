using System;
using System.Linq;
using System.Numerics;

using Raylib_cs;

namespace Kabufuda
{
	public class Game
	{
		public static Color PRIMARY_BG_COLOR { get; private set; } = new Color(200, 90, 0, 255);
		public static Color SECONDARY_BG_COLOR { get; private set; } = new Color(225, 161, 0, 255);
		public static Color TERTIARY_BG_COLOR { get; private set; }  = new Color(150, 42, 0, 255);

		private Rectangle backgroundTopRect;
		private Rectangle backgroundBorderRect;

		int screenWidth = 800;
		int screenHeight = 600;

		int targetFps = 30;

		private static PlayArea _playArea;
		public static PlayArea PlayArea { get => _playArea; }

		// UI
		private GUIButton newGameButton;
		private GUIButton howToPlayButton;

		private GUIButton htpNextButton;
		private GUIButton htpBackButton;

		// How To Play
		private bool howToPlayActive = false;
		private int htpCurrentSlide = 0;
		private Image[] htpImages = new Image[5];
		private Texture2D[] htpTextures = new Texture2D[5];


		public Game()
		{
			_playArea = new PlayArea();

			backgroundTopRect = new Rectangle(0, 0, screenWidth, screenHeight * 2/5);
			backgroundBorderRect = new Rectangle(0, screenHeight * 2 / 5 - 10, screenWidth, 20);
		}

		public int Run()
		{

			// Initial Setup
			Raylib.InitWindow(screenWidth, screenHeight, "Kabufuda");
			Raylib.SetTargetFPS(targetFps);

			// UI setup
			newGameButton = new GUIButton(new Vector2(15, 75), new Vector2(75, 50), $"img/Button_NewGame.png");
			newGameButton.MouseClickEvent += InitCards;
			howToPlayButton = new GUIButton(new Vector2(15, 15), new Vector2(75, 50), $"img/Button_HowToPlay.png");
			howToPlayButton.MouseClickEvent += () => SetHowToPlayActive(!howToPlayActive);

			htpNextButton = new GUIButton(new Vector2(screenWidth - 75 - 15, screenHeight - 50 - 15), new Vector2(75, 50), $"img/Button_Next.png");
			htpNextButton.MouseClickEvent += () => GoToHowToPlaySlide(htpCurrentSlide + 1);
			htpBackButton = new GUIButton(new Vector2(15, screenHeight - 50 - 15), new Vector2(75, 50), $"img/Button_Back.png");
			htpBackButton.MouseClickEvent += () => GoToHowToPlaySlide(htpCurrentSlide - 1);

			// Play Area Setup
			InitFreeSpaces();
			InitCards();

			// How To Play Setup
			InitHowToPlay();

			while (!Raylib.WindowShouldClose())
			{
				Input();

				Update();

				Render();
			}

			// Unload How To Play textures
			for (int i = 0; i < htpTextures.Length; i++)
			{
				Raylib.UnloadTexture(htpTextures[i]);
			}
			for (int i = 0; i < htpImages.Length; i++)
			{
				Raylib.UnloadImage(htpImages[i]);
			}

			Raylib.CloseWindow();

			return 0;
		}

		private void InitFreeSpaces()
		{
			// Free Space Creation
			int freeMargin = 150;
			int freePosStep = (screenWidth - freeMargin * 2) / _playArea.FreeSpaces.Length;
			for (int i = 0; i < _playArea.FreeSpaces.Length; i++)
			{
				int temp = i;
				_playArea.FreeSpaces[i] = new FreeSpace(new Vector2(freeMargin + freePosStep * temp, screenHeight / 16));
			}

			// Card Stack Creation
			int stackMargin = 4;
			int stackPosStep = (screenWidth - stackMargin * 2) / _playArea.CardStacks.Length;
			for (int i = 0; i < _playArea.CardStacks.Length; i++)
			{
				int temp = i;
				_playArea.CardStacks[i] = new CardStack(new Vector2(stackMargin + stackPosStep * temp, screenHeight * 4 / 9));
			}
		}

		private void InitCards()
		{
			_playArea.ClearCardList();

			// Card Creation
			List<int> numberList = new List<int>();

			for (int i = 0; i < PlayArea.UNIQUE_CARDS; i++)
			{
				int currentNumber = i;
				for (int j = 0; j < PlayArea.DUPLICATE_CARDS; j++)
				{
					numberList.Add(currentNumber);
				}
			}
			numberList.Randomize();

			int cardNumber = 0;
			int cardsPerStack = (numberList.Count / _playArea.CardStacks.Length);
			int cardPosStep = (screenWidth - 100) / cardsPerStack;
			for (int i = 0; i < _playArea.CardStacks.Length; i++)
			{
				for (int j = 0; j < cardsPerStack; j++)
				{
					Card newCard = new Card(numberList[cardNumber++] + 1, new Vector2(0, 0));

					_playArea.AddCard(newCard);

					_playArea.CardStacks[i].AddCard(newCard);
				}
			}
		}


		private void InitHowToPlay()
		{
			for (int i = 0; i < htpImages.Length; i++)
			{
				int number = i;
				htpImages[i] = Raylib.GenImageColor(screenWidth, screenHeight, Color.WHITE);

				Image fileImage = Raylib.LoadImage($"img/HowTo_{number}.png");
				Raylib.ImageDraw(ref htpImages[i],
						fileImage,
						new Rectangle(0, 0, fileImage.width, fileImage.height),
						new Rectangle(0, 0, screenWidth, screenHeight),
						Color.WHITE);

				htpTextures[i] = Raylib.LoadTextureFromImage(htpImages[i]);
			}
		}

		private void SetHowToPlayActive(bool value)
		{
			howToPlayActive = value;
			htpCurrentSlide = 0;
		}

		private void GoToHowToPlaySlide(int number)
		{
			htpCurrentSlide = Math.Clamp(number, 0, htpTextures.Length-1);
		}

		private void Input()
		{
			MouseInputEvent mouseEvent = new MouseInputEvent();

			howToPlayButton.Input(mouseEvent);

			if (!howToPlayActive)
			{
				// Draw UI
				newGameButton.Input(mouseEvent);
				

				if(Card.ActiveCard != null)
				{
					Card.ActiveCard.Input(mouseEvent);
				}

				if(mouseEvent.Consumed)
				{
					return;
				}

				// Try to send Inputs to Free Spaces and Card Stacks
				foreach (var space in _playArea.FreeSpaces)
				{
					if (space.PeekCard(out Card? card))
					{
						card?.Input(mouseEvent);
					}
				}

				foreach (var stack in _playArea.CardStacks)
				{
					if(stack.PeekCard(out Card? peekCard))
					{
						foreach (var card in stack.Stack)
						{
							if(card.Number == peekCard?.Number)
							{
								card.Input(mouseEvent);
							}
							else
							{
								break;
							}
						}
					}
				}

				if(!mouseEvent.Consumed)
				{
					Card.ActiveCard = null;
				}
			}
			else
			{
				htpNextButton.Input(mouseEvent);
				htpBackButton.Input(mouseEvent);
			}
		}

		private void Update()
		{
			howToPlayButton.Update();

			if(!howToPlayActive)
			{
				// UI
				newGameButton.Update();

				// Try to send Inputs to Free Spaces and Card Stacks
				foreach (var space in _playArea.FreeSpaces)
				{
					if (space.PeekCard(out Card? card))
					{
						card?.Update();
					}
				}

				foreach (var stack in _playArea.CardStacks)
				{
					stack.Stack.ForEach(card => card.Update());
				}
			}
			else
			{
				htpNextButton.Update();
				htpBackButton.Update();
			}
			
		}

		private List<Card> cardList = new List<Card>();
		private void Render()
		{
			Raylib.BeginDrawing();

			if(!howToPlayActive)
			{

				Raylib.ClearBackground(PRIMARY_BG_COLOR);
				Raylib.DrawRectangleRec(backgroundTopRect, SECONDARY_BG_COLOR);
				Raylib.DrawRectangleRec(backgroundBorderRect, TERTIARY_BG_COLOR);

				cardList.Clear();

				foreach (var space in _playArea.FreeSpaces)
				{
					space.Render();

					if (space.PeekCard(out Card? card))
					{
						cardList.Add(card!);
					}
				}

				foreach (var stack in _playArea.CardStacks)
				{
					stack.Render();

					if (!stack.Locked)
					{
						foreach (var card in stack.Stack)
						{
							cardList.Add(card);
						}
					}
				}

				//cards.Reverse();
				cardList = cardList.OrderBy(card => card.Layer).ToList();
				foreach (var card in cardList)
				{
					card.Render();
				}

				// Draw UI
				newGameButton.Render();


				if (_playArea.GameComplete)
				{
					Raylib.DrawTextPro(Raylib.GetFontDefault(),
							$"You Win!",
							new Vector2(screenWidth / 2, screenHeight * 3 / 4),
							new Vector2(125, 0),
							0,
							64,
							4,
							TERTIARY_BG_COLOR);
				}
			}
			else
			{
				Raylib.DrawTexture(htpTextures[htpCurrentSlide], 0, 0, Color.WHITE);

				htpNextButton.Render();
				htpBackButton.Render();
			}

			howToPlayButton.Render();

			Raylib.EndDrawing();
		}
	}
}