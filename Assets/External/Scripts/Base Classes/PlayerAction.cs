using System.Collections.Generic;

namespace Hanafuda
{
    public class PlayerAction
    {
        private Card HandSelection;
        private Card DeckSelection = null;
        private List<Card> HandMatches = new List<Card>();
        private List<Card> _DeckMatches = new List<Card>();
        private List<Card> DeckMatches
        {
            get { return _DeckMatches; }
            set
            {
                _DeckMatches = value;
                if (value.Count != 2)
                {
                    GetNewYaku();
                }
            }
        }
        public readonly List<Yaku> NewYaku = new List<Yaku>();
        private bool Koikoi = false;
        private Spielfeld Board;

        public bool isFinal()
        {
            return !Koikoi;
        }
        public void SayKoikoi()
        {
            Koikoi = true;
        }

        public void Init(Spielfeld board)
        {
            Board = board;
        }
        public void SelectFromHand(Card selection)
        {
            HandSelection = selection;
            HandMatches = Board.Field.FindAll(x => x.Monat == selection.Monat);
        }
        public void SelectHandMatch(Card selection)
        {
            HandMatches = new List<Card>() { selection };
        }
        public void DrawCard()
        {
            DeckSelection = Board.Deck[0];
            DeckMatches = Board.Field.FindAll(x => x.Monat == DeckSelection.Monat);
        }
        public void SelectDeckMatch(Card fieldSelection)
        {
            DeckMatches = new List<Card>() { fieldSelection };
        }

        public override string ToString()
        {
            string handAction;
            if (HandMatches.Count == 0)
                handAction = $"legt {HandSelection} aufs Feld.";
            else
            {
                string cards = string.Join(", ", HandMatches);
                handAction = $"sammelt {cards} und {HandSelection} ein.";
            }

            string deckAction;
            if (DeckMatches.Count == 0)
                deckAction = $"legt er {DeckSelection} aufs Feld.";
            else
            {
                string cards = string.Join(", ", DeckMatches);
                deckAction = $"sammelt er {cards} und {DeckSelection} ein.";
            }

            string yakuAction = "";
            if (NewYaku.Count > 0)
            {
                string yaku = string.Join(", ", NewYaku);

                string koikoiAction = "";
                if (Koikoi)
                    koikoiAction = $"{Settings.GetName()} hat 'Koi Koi' gesagt.";
                else
                    koikoiAction = $"{Settings.GetName()} hat nicht 'Koi Koi' gesagt. Das Spiel ist beendet.";

                yakuAction = $"Damit erreicht er die Yaku {yaku}.\n{koikoiAction}\n";
            }
            return $"{Settings.GetName()} {handAction}.\n " +
                $"Anschließend {deckAction}.\n" +
                $"{yakuAction}";
        }

        public void Apply()
        {
            if (DeckSelection)
                ApplyDeckSelection();
            else
                ApplyHandSelection();
        }
        private void ApplyHandSelection()
        {
            List<Card> Hand = ((Player)Board.players[Board.Turn ? 0 : 1]).Hand;
            Hand.RemoveAll(x => x.Title == HandSelection.Title);
            if (HandMatches.Count > 0)
            {
                List<Card> Collection = ((Player)Board.players[Board.Turn ? 0 : 1]).CollectedCards;
                Collection.AddRange(HandMatches);
                Collection.Add(HandSelection);
            }
            else
            {
                Board.Field.Add(HandSelection);
            }
        }
        private void ApplyDeckSelection()
        {
            Board.Deck.RemoveAll(x => x.Title == DeckSelection.Title);
            if (DeckMatches.Count > 0)
            {
                List<Card> Collection = ((Player)Board.players[Board.Turn ? 0 : 1]).CollectedCards;
                Collection.AddRange(DeckMatches);
                Collection.Add(DeckSelection);
            }
            else
            {
                Board.Field.Add(DeckSelection);
            }
        }

        public void Apply3D()
        {

        }

        public void GetNewYaku()
        {
            NewYaku.Clear();
            List<Card> oldCollection = ((Player)Board.players[Board.Turn ? 0 : 1]).CollectedCards;
            List<Card> newCollection = new List<Card>();
            if (HandMatches.Count > 0)
                newCollection.Add(HandSelection);
            newCollection.AddRange(HandMatches);
            if (DeckMatches.Count > 0)
                newCollection.Add(DeckSelection);
            newCollection.AddRange(DeckMatches);
            List<Yaku> oldYaku = Yaku.GetYaku(oldCollection);
            List<Yaku> newYaku = Yaku.GetYaku(newCollection);
            for (int i = newYaku.Count; i >= 0; i--)
            {
                if (oldYaku.Exists(x=>x.Title == newYaku[i].Title))
                    NewYaku.Add(newYaku[i]);
                else
                {
                    if(newYaku[i].addPoints > 0)
                    {
                        int oldPoints = oldCollection.FindAll(x => x.Typ == newYaku[i].TypPref).Count;
                        int newPoints = newCollection.FindAll(x => x.Typ == newYaku[i].TypPref).Count;
                        if (newPoints > oldPoints)
                            NewYaku.Add(NewYaku[i]);
                    }
                }
            }
        }
    }
}