using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hanafuda
{
    [Serializable]
    public class UninformedBoard : IBoard<UninformedBoard>
    {
        public Dictionary<int, int> CollectedYaku;

        /// <summary>
        /// Unbekannte Karten mit Wahrscheinlichkeit, zur Hand des Gegners zu gehören.
        /// </summary>
        public Dictionary<Card, float> UnknownCards;

        public List<Card> OpponentCollection;

        public int OpponentHandSize;

        public override UninformedBoard Clone() => new UninformedBoard(this);

        /// <summary>
        /// Echte Kopie der Listen im Spielfeld und Anpassen des Informationsstandes eines echten Spielfeldes
        /// auf den einer unwissenden KI.
        /// </summary>
        /// <param name="root">Referenz-Spielfeld</param>
        public UninformedBoard(IHanafudaBoard root) : base(root)
        {
            CollectedYaku = root.Players[Settings.PlayerID].CollectedYaku;
            OpponentCollection = root.Players[Settings.PlayerID].CollectedCards;
            OpponentHandSize = root.Players[Settings.PlayerID].Hand.Count;

            UnknownCards = Global.allCards
                .Except(OpponentCollection)
                .Except(computer.Hand)
                .Except(computer.CollectedCards)
                .Except(Field)
                .ToDictionary(x => x, x => 0f);
            float divisor = UnknownCards.Count;
            float divident = UnknownCards.Count - OpponentHandSize;
            UnknownCards = UnknownCards.ToDictionary(x => x.Key, x => 1f - divident / divisor);

        }

        /// <summary>
        /// Echte Kopie der Klasse
        /// </summary>
        /// <param name="target">Referenz-Instanz</param>
        protected UninformedBoard(UninformedBoard target) : base(target)
        {
            CollectedYaku = new Dictionary<int, int>(target.CollectedYaku);
            UnknownCards = new Dictionary<Card, float>(target.UnknownCards);
            OpponentCollection = new List<Card>(target.OpponentCollection);
            OpponentHandSize = target.OpponentHandSize;
        }

        /// <summary>
        /// Uninformierter Spielzug der KI, Hand bekannt, Deckzug unbekannt
        /// </summary>
        /// <param name="boardCoords">Eltern-Koordinaten des neuen Spielfelds</param>
        /// <param name="move">getätigter Spielzug</param>
        /// <param name="turn">Immer wahr in dieser Überladung</param>
        /// <returns></returns>
        protected override void ApplyMove(string selection, string secondSelection, bool fromHand, bool turn)
        {
            List<Card> activeCollection = turn ? computer.CollectedCards : OpponentCollection;
            Dictionary<Card, float> target = (fromHand && turn) ? computer.Hand.ToDictionary(x => x, x => 1f) : UnknownCards;

            Card selectedCard = target.First(x => x.Key.Title == selection).Key;
            List<Card> matches = new List<Card>();

            //Build Matches and Remove from Field
            for (int i = Field.Count - 1; i >= 0; i--)
            {
                if (secondSelection.Length > 0)
                {
                    if (Field[i].Title == secondSelection)
                    {
                        matches.Add(Field[i]);
                        Field.RemoveAt(i);
                        break;
                    }
                    continue;
                }
                else if (Field[i].Monat == selectedCard.Monat)
                {
                    matches.Add(Field[i]);
                    Field.RemoveAt(i);
                }
            }

            //Collect Cards or add to Field
            target.Remove(selectedCard);
            if (matches.Count > 0)
            {
                matches.Add(selectedCard);
                activeCollection.AddRange(matches);
            }
            else
            {
                Field.Add(selectedCard);
            }
        }

        protected override bool CheckYaku(bool turn)
        {
            List<Card> activeCollection = turn ? computer.CollectedCards : OpponentCollection;
            return Yaku.GetNewYakus(Enumerable.Range(0, Global.allYaku.Count).ToDictionary(x => x, x => 0), activeCollection).Count > 0;
        }
    }
}