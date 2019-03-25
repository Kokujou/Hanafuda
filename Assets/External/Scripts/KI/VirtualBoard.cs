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

        [Serializable]
        public struct Coords { public int x; public int y; }
        public Coords parentCoords;

        public void SayKoikoi(bool koikoi)
        {
            isFinal = !koikoi;
            LastMove.Koikoi = koikoi;
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
        public VirtualBoard(VirtualBoard parent, Move move, bool Turn, Coords parentCoords)
        {
            //WICHTIG! Einsammeln bei Kartenzug!
            this.parentCoords = parentCoords;
            Deck = new List<Card>(parent.Deck);
            Field = new List<Card>(parent.Field);
            Value = 0f;

            players = new List<Player>();
            for (int player = 0; player < parent.players.Count; player++)
                players.Add(new Player(parent.players[player]));
            Player activePlayer = players[Turn ? 1 - Settings.PlayerID : Settings.PlayerID];

            Card handSelection = activePlayer.Hand.Find(x => x.Title == move.HandSelection);
            List<Card> handMatches = new List<Card>();

            Card deckSelection = Deck.Find(x => x.Title == move.DeckSelection);
            List<Card> deckMatches = new List<Card>();

            for (int i = Field.Count - 1; i >= 0; i--)
            {
                if (move.HandFieldSelection.Length > 0)
                {
                    if (Field[i].Title == move.HandFieldSelection)
                    {
                        handMatches.Add(Field[i]);
                        Field.RemoveAt(i);
                        break;
                    }
                    continue;
                }
                else if (Field[i].Monat == handSelection.Monat)
                {
                    handMatches.Add(Field[i]);
                    Field.RemoveAt(i);
                    continue;
                }

                if (move.DeckFieldSelection.Length > 0)
                {
                    if (Field[i].Title == move.DeckFieldSelection)
                    {
                        deckMatches.Add(Field[i]);
                        Field.RemoveAt(i);
                        break;
                    }
                    continue;
                }
                else if (Field[i].Monat == deckSelection.Monat)
                {
                    deckMatches.Add(Field[i]);
                    Field.RemoveAt(i);
                    continue;
                }
            }

            activePlayer.Hand.Remove(handSelection);
            if (handMatches.Count > 0)
            {
                handMatches.Add(handSelection);
                activePlayer.CollectedCards.AddRange(handMatches);
            }
            else
            {
                Field.Add(handSelection);
            }

            Deck.Remove(deckSelection);
            if (deckMatches.Count > 0)
            {
                deckMatches.Add(deckSelection);
                activePlayer.CollectedCards.AddRange(deckMatches);
            }
            else
            {
                Field.Add(deckSelection);
            }
            /*
            var Yakus = new List<Yaku>();
            Yakus = Yaku.GetYaku(activePlayer.CollectedCards.ToList());
            var nPoints = 0;
            for (var i = 0; i < Yakus.Count; i++)
            {
                nPoints += Yakus[i].basePoints;
                if (Yakus[i].addPoints != 0)
                    nPoints += (activePlayer.CollectedCards.Count(x => x.Typ == Yakus[i].TypPref) -
                                Yakus[i].minSize) * Yakus[i].addPoints;
            }
            HasNewYaku = nPoints > activePlayer.tempPoints;
            */
            HasNewYaku = false;
            LastMove = move;
            LastMove.HadYaku = HasNewYaku;
        }
    }
}