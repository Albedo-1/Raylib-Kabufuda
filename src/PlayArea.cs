namespace Kabufuda
{
	public class PlayArea
	{
		public const int UNIQUE_CARDS = 10;
		public const int DUPLICATE_CARDS = 4;

		public FreeSpace[] FreeSpaces = new FreeSpace[4];
		public CardStack[] CardStacks = new CardStack[8];
		List<Card> cards = new List<Card>();
		public Card[] Cards { get => cards.ToArray(); }

		public bool GameSetup => cards.Count() == UNIQUE_CARDS * DUPLICATE_CARDS;
		public bool GameComplete 
		{
			get 
			{
				int lockedSpaces = 0;

				foreach (var item in FreeSpaces)
				{
					if(item.Locked) 
						lockedSpaces++;
				}
				foreach (var item in CardStacks)
				{
					if (item.Locked)
						lockedSpaces++;
				}

				return lockedSpaces >= 10;
			}
		}

		public void AddCard(Card card)
		{
			cards.Add(card);
		}

		public void ClearCardList()
		{
			foreach (var item in FreeSpaces)
			{
				item.CleanHolder();
			}
			foreach (var item in CardStacks)
			{
				item.CleanHolder();
			}

			for (int i = cards.Count -1; i >= 0; i--)
			{
				cards[i].Dispose();
			}
			cards.Clear();

		}
	}
}