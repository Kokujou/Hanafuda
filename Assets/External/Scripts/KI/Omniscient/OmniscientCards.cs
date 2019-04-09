using System.Collections.Generic;
using System.Linq;

namespace Hanafuda
{
    public partial class OmniscientAI
    {
        public class OmniscientCards : CardCollection
        {
            Dictionary<Card.Months, uint> PPlayableMonths;
            Dictionary<Card.Months, uint> PCollectableMonths;
            Dictionary<Card.Months, uint> OppPlayableMonths;
            Dictionary<Card.Months, uint> OppCollectableMonths;

            protected override void Preparations() => CalcMonths(State, Turn);

            public OmniscientCards(IEnumerable<CardProperties> list, VirtualBoard State, bool Turn) : base(list, State, Turn) { }

            private void CalcMonths(VirtualBoard State, bool Turn)
            {
                PPlayableMonths = new Dictionary<Card.Months, uint>(
                    Enumerable.Range(0, 12).ToDictionary(x => (Card.Months)x, x => (uint)0));
                OppPlayableMonths = new Dictionary<Card.Months, uint>(PPlayableMonths);
                PCollectableMonths = new Dictionary<Card.Months, uint>(PPlayableMonths);
                OppCollectableMonths = new Dictionary<Card.Months, uint>(PPlayableMonths);
                for (int cardID = 0; cardID < player.Hand.Count; cardID++)
                {
                    Card deckCard = State.Deck[cardID * 2];
                    Card handCard = player.Hand[cardID];
                    PPlayableMonths[handCard.Monat]++;
                    PPlayableMonths[deckCard.Monat]++;
                }

                for (int cardID = 0; cardID < opponent.Hand.Count; cardID++)
                {
                    Card deckCard = State.Deck[cardID * 2 + 1];
                    Card handCard = opponent.Hand[cardID];
                    PPlayableMonths[handCard.Monat]++;
                    PPlayableMonths[deckCard.Monat]++;
                }

                PCollectableMonths = PPlayableMonths.ToDictionary(x => x.Key, x => x.Value);
                foreach (Card card in State.Field)
                    PCollectableMonths[card.Monat]++;

                OppCollectableMonths = OppPlayableMonths.ToDictionary(x => x.Key, x => x.Value);
                foreach (Card card in State.Field)
                    OppCollectableMonths[card.Monat]++;

                PCollectableMonths = PCollectableMonths.ToDictionary(x => x.Key, x => (x.Value / 2) * 2);
                OppCollectableMonths = OppCollectableMonths.ToDictionary(x => x.Key, x => (x.Value / 2) * 2);

            }

            protected override void CalcMinTurns(VirtualBoard State, bool Turn)
            {
                /*
                 * Verbesserung: Bessere Approximation für Einsammelnbare Karten
                 */
                foreach (Card card in player.CollectedCards)
                    this[card.ID].MinTurns = 0;

                for (int cardID = 0; cardID < player.Hand.Count; cardID++)
                {
                    Card deckCard = State.Deck[cardID * 2];
                    Card handCard = player.Hand[cardID];
                    this[deckCard.ID].MinTurns = cardID + 1;
                    this[handCard.ID].MinTurns = 1;
                }

                foreach (Card card in State.Field)
                {
                    if (PPlayableMonths[card.Monat] > 0)
                        this[card.ID].MinTurns = 1;
                }

                //Gegnerische Karten werden aufs Feld gelegt, wenn kein Match existiert
                for (int cardID = 0; cardID < opponent.Hand.Count; cardID++)
                {
                    Card handCard = State.Deck[cardID * 2 + 1];
                    Card deckCard = opponent.Hand[cardID];
                    if (OppCollectableMonths[handCard.Monat] == 0)
                        this[handCard.ID].MinTurns = 1;
                    if (OppCollectableMonths[deckCard.Monat] == 0)
                        this[deckCard.ID].MinTurns = cardID * 2 + 1;
                }
            }

            protected override void CalcProbs(VirtualBoard State, bool Turn)
            {
                foreach (CardProperties card in this)
                {
                    if (card.MinTurns == 0)
                        card.Probability = 1;
                }
            }
        }
    }
}