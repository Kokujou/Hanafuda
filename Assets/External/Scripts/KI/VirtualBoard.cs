using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Hanafuda
{
    public class VirtualBoard
    {
        public List<Card> Deck, Field;
        public List<Player> players;
        public Move LastMove;
        public float Value;
        public bool isFinal;
        public bool HasNewYaku;
        public bool Turn;

        public void SayKoikoi(bool koikoi)
        {
            isFinal = true;
        }
        public VirtualBoard(Spielfeld root)
        {
            Deck = new List<Card>(root.Deck);
            Field = new List<Card>(root.Field);
            LastMove = null;
            players = new List<Player>();
            for (int player = 0; player < root.players.Count; player++)
                players.Add(new Player(root.players[player]));
            Value = 0f;
            isFinal = false;
        }
        /// <summary>
        /// KI-Konstruktor
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="move"></param>
        /// <param name="Turn"></param>
        public VirtualBoard(VirtualBoard parent, Move move, bool Turn)
        {
            //WICHTIG! Einsammeln bei Kartenzug!
            Deck = new List<Card>(parent.Deck);
            Field = new List<Card>(parent.Field);
            Value = 0f;
            players = new List<Player>();
            for (int player = 0; player < parent.players.Count; player++)
                players.Add(new Player(parent.players[player]));
            Player activePlayer = players[Turn ? 1 - Settings.PlayerID : Settings.PlayerID];
            Card handSelection = activePlayer.Hand.Find(x => x.Title == move.HandSelection);

            List<Card> handMatches = new List<Card>();
            try
            {
                if (move.HandFieldSelection.Length <= 0)
                    handMatches = Field.FindAll(x => x.Monat == handSelection.Monat);
                else
                    handMatches = new List<Card>() { Field.Find(x => x.Title == move.HandFieldSelection) };
            }
            catch(Exception e)
            {
                Debug.Log($"Hand: {handSelection.ToString()} Name: {move.HandSelection}");
                throw e;
            }

            Card deckSelection = Deck.Find(x => x.Title == move.DeckSelection);
            List<Card> deckMatches;
            if (move.DeckFieldSelection.Length <= 0)
                deckMatches = Field.FindAll(x => x.Monat == deckSelection.Monat);
            else
                deckMatches = new List<Card>() { Field.Find(x => x.Title == move.DeckFieldSelection) };

            activePlayer.Hand.Remove(handSelection);
            Field.Add(handSelection);
            if (handMatches.Count > 0)
            {
                handMatches.Add(handSelection);
                Field.RemoveAll(x => handMatches.Contains(x));
                activePlayer.CollectedCards.AddRange(handMatches);
            }

            Deck.Remove(deckSelection);
            Field.Add(deckSelection);
            if (deckMatches.Count > 0)
            {
                deckMatches.Add(deckSelection);
                Field.RemoveAll(x => handMatches.Contains(x));
                activePlayer.CollectedCards.AddRange(deckMatches);
            }

            var Yakus = new List<Yaku>();
            int ID = Turn ? Settings.PlayerID : 1 - Settings.PlayerID;
            Yakus = Yaku.GetYaku(((Player)players[ID]).CollectedCards.ToList());
            var nPoints = 0;
            for (var i = 0; i < Yakus.Count; i++)
            {
                nPoints += Yakus[i].basePoints;
                if (Yakus[i].addPoints != 0)
                    nPoints += (((Player)players[ID]).CollectedCards.Count(x => x.Typ == Yakus[i].TypPref) -
                                Yakus[i].minSize) * Yakus[i].addPoints;
            }
            HasNewYaku = nPoints > ((Player)players[ID]).tempPoints;
        }
    }
}