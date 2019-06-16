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
        public bool HadYaku;
        public Card HandSelection = null;
        public Card DeckSelection = null;
        public Card HandFieldSelection = null;
        public Card DeckFieldSelection = null;
        public List<Card> HandMatches = new List<Card>();
        public List<Card> DeckMatches = new List<Card>();
        public bool Koikoi = false;
        private IHanafudaBoard Board;

        public bool isFinal()
        {
            return !Koikoi;
        }
        public void SayKoikoi(bool koikoi)
        {
            HadYaku = true;
            Koikoi = koikoi;
        }

        public void Init(IHanafudaBoard board)
        {
            Board = board;
            PlayerID = Settings.PlayerID;
        }
        public void SelectFromHand(Card selection)
        {
            HandSelection = selection;
            HandMatches = Board.Field.FindAll(x => x.Monat == selection.Monat);
        }
        public void SelectHandMatch(Card selection)
        {
            HandFieldSelection = selection;
            HandMatches.Clear();
            HandMatches = new List<Card>() { HandSelection, HandFieldSelection };
        }
        public void DrawCard(Card selection = null)
        {
            if (selection)
                DeckSelection = selection;
            else
                DeckSelection = Board.Deck[0];
            DeckMatches = Board.Field.FindAll(x => x.Monat == DeckSelection.Monat);
            DeckMatches.RemoveAll(x => HandMatches.Contains(x));
        }
        public void SelectDeckMatch(Card fieldSelection)
        {
            DeckFieldSelection = fieldSelection;
            DeckMatches.Clear();
            DeckMatches = new List<Card>() { DeckSelection, DeckFieldSelection };
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
                    koikoiAction = $"{Settings.Players[PlayerID]} hat 'Koi Koi' gesagt.";
                else
                    koikoiAction = $"{Settings.Players[PlayerID]} hat nicht 'Koi Koi' gesagt. Das Spiel ist beendet.";

                yakuAction = $"Damit konnte er neue Yaku formen.\n{koikoiAction}\n";
            }
            return $"{Settings.Players[PlayerID]} {handAction}.\n " +
                $"Anschließend {deckAction}.\n" +
                $"{yakuAction}";
        }

        public static implicit operator Move(PlayerAction action)
        {
            Move move = new Move();
            move.PlayerID = action.PlayerID;
            if (action.SingleSelection)
            {
                move.SingleSelection = action.SingleSelection.name;
            }
            else
            {
                move.HandSelection = action.HandSelection.Title;
                move.DeckSelection = action.DeckSelection?.Title;
                if (action.HandFieldSelection)
                    move.HandFieldSelection = action.HandMatches[0].Title;
                if (action.DeckFieldSelection)
                    move.DeckFieldSelection = action.DeckMatches[0].Title;
                move.HadYaku = action.HadYaku;
                move.Koikoi = action.Koikoi;
            }
            return move;
        }

        public static PlayerAction FromMove(Move move, IHanafudaBoard board)
        {
            PlayerAction action = new PlayerAction();
            action.Init(board);
            if (move.HandSelection.Length > 0)
                action.SelectFromHand(Global.allCards.Find(x => x.Title == move.HandSelection));
            if (move.SingleSelection.Length > 0)
                action.SingleSelection = Global.allCards.Find(x => x.Title == move.SingleSelection);
            if (move.HandFieldSelection.Length > 0)
                action.SelectHandMatch(Global.allCards.Find(x => x.Title == move.HandFieldSelection));
            if (move.DeckSelection?.Length > 0)
                action.DrawCard(Global.allCards.Find(x => x.Title == move.DeckSelection));
            if (move.DeckFieldSelection?.Length > 0)
                action.SelectDeckMatch(Global.allCards.Find(x => x.Title == move.DeckFieldSelection));
            action.HadYaku = move.HadYaku;
            action.Koikoi = move.Koikoi;
            action.PlayerID = move.PlayerID;
            return action;
        }
    }
}