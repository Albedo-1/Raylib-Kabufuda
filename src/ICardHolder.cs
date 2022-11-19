
namespace Kabufuda
{
	public interface ICardHolder
	{
		public void PositionCardOnHolder(Card card);

		public void OnCardActed(Card actedCard);

		public void AddCard(Card card);

		public bool PeekCard(out Card? card);

		public Card? PopCard();

		public void CleanHolder();

		public void ReturnCard(Card card);

		public void LockHolder();
	}
}