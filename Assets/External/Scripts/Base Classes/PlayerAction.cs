using ExtensionMethods;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Hanafuda
{
    public class PlayerAction
    {
        public Card SingleSelection;
        public int PlayerID;
        public int ActionID;
        public bool HadYaku;
        public Card HandSelection;
        public Card DeckSelection = null;
        public List<Card> HandMatches = new List<Card>();
        public List<Card> _DeckMatches = new List<Card>();
        public List<Card> DeckMatches
        {
            get { return _DeckMatches; }
            set
            {
                _DeckMatches = value;
                if (value.Count != 2)
                {
                    //GetNewYaku();
                }
            }
        }
        public bool Koikoi = false;
        private Spielfeld Board;

        public bool isFinal()
        {
            return !Koikoi;
        }
        public void SayKoikoi(bool koikoi)
        {
            HadYaku = true;
            Koikoi = koikoi;
        }

        public void Init(Spielfeld board)
        {
            Board = board;
            PlayerID = Settings.PlayerID;
            ActionID = (int)(Time.timeSinceLevelLoad * 100);
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
            if (HadYaku)
            {
                string koikoiAction = "";
                if (Koikoi)
                    koikoiAction = $"{Settings.GetName()} hat 'Koi Koi' gesagt.";
                else
                    koikoiAction = $"{Settings.GetName()} hat nicht 'Koi Koi' gesagt. Das Spiel ist beendet.";

                yakuAction = $"Damit konnte er neue Yaku formen.\n{koikoiAction}\n";
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

        public static implicit operator Move(PlayerAction action)
        {
            Move move = new Move();
            move.PlayerID = action.PlayerID;
            move.MoveID = action.ActionID;
            if (action.SingleSelection)
            {
                move.SingleSelection = action.SingleSelection.name;
            }
            else
            {
                move.HandSelection = action.HandSelection.Title;
                move.DeckSelection = action.DeckSelection.Title;
                if (action.HandMatches.Count == 1)
                    move.HandFieldSelection = action.HandMatches[0].Title;
                if (action.DeckMatches.Count == 1)
                    move.DeckFieldSelection = action.DeckMatches[0].Title;
                move.HadYaku = action.HadYaku;
                move.Koikoi = action.Koikoi;
            }
            return move;
        }

        public static implicit operator PlayerAction(Move move)
        {
            PlayerAction action = new PlayerAction();
            if (move.HandSelection != "")
                action.HandSelection = Global.allCards.Find(x => x.Title == move.HandSelection);
            if (move.SingleSelection != "")
                action.SingleSelection = Global.allCards.Find(x => x.Title == move.SingleSelection);
            if (move.HandFieldSelection != "")
                action.HandMatches = new List<Card>() { Global.allCards.Find(x => x.Title == move.HandFieldSelection) };
            if (move.DeckSelection != "")
                action.DeckSelection = Global.allCards.Find(x => x.Title == move.DeckSelection);
            if (move.DeckFieldSelection != "")
                action.DeckMatches = new List<Card>() { Global.allCards.Find(x => x.Title == move.DeckFieldSelection) };
            action.HadYaku = move.HadYaku;
            action.Koikoi = move.Koikoi;
            action.PlayerID = move.PlayerID;
            action.ActionID = move.MoveID;
            return action;
        }
    }
}