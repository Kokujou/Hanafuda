﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Random = System.Random;

namespace Hanafuda
{
    public class AIArena : MonoBehaviour
    {
        public RectTransform Content;

        public NumericUpDown Times;
        public Dropdown Player1, Player2;

        private uint ExecutionTimes;
        private Settings.AIMode P1Mode, P2Mode;

        private Player P1KI, P2KI;

        public Color InfoColor, WarningColor, ErrorColor;

        public static class IterationOutput
        {
            public static int P1Wins;
            public static int P2Wins;
            public static int Draws;
            public static float avgDuration;
        }

        struct VirtualBoard : IHanafudaBoard
        {
            public List<Card> Deck { get; set; }
            public List<Card> Field { get; set; }
            public List<Player> Players { get; set; }
            public bool Turn { get; set; }

            public void ApplyMove(Move move)
            {
                Player activePlayer = Players[move.PlayerID];
                Card handSelection = activePlayer.Hand.Find(x => x.Title == move.HandSelection);
                activePlayer.Hand.Remove(handSelection);

                List<Card> handMatches = Field.FindAll(x => x.Monat == handSelection.Monat);
                if (handMatches.Count == 2)
                    handMatches = new List<Card>() { handMatches.First(x => x.Title == move.HandFieldSelection) };
                else if (handMatches.Count == 0)
                    Field.Add(handSelection);
                else
                    handMatches.Add(handSelection);
                activePlayer.CollectedCards.AddRange(handMatches);
                foreach (Card card in handMatches)
                    Field.Remove(card);

                Card deckSelection = Deck[0];
                Deck.RemoveAt(0);

                List<Card> deckMatches = Field.FindAll(x => x.Monat == deckSelection.Monat);
                if (deckMatches.Count == 2)
                    deckMatches = new List<Card>() { deckMatches.First(x => x.Title == move.DeckFieldSelection) };
                else if (deckMatches.Count == 0)
                    Field.Add(deckSelection);
                else
                    deckMatches.Add(deckSelection);
                activePlayer.CollectedCards.AddRange(deckMatches);
                foreach (Card card in deckMatches)
                    Field.Remove(card);

            }
        }
        public void StartSimulation()
        {
            ExecutionTimes = Times.Value;
            P1Mode = (Settings.AIMode)Player1.value;
            P2Mode = (Settings.AIMode)Player2.value;

            for (int round = 1; round <= ExecutionTimes; round++)
            {
                Log($"Starting round #{round}");
                PlayNewGame(true);
            }
            for (int round = 1; round <= ExecutionTimes; round++)
            {
                Log($"Starting round #{round}");
                PlayNewGame(false);
            }

            Log($"\n\nP1Wins: {(float)IterationOutput.P1Wins / (ExecutionTimes * 2)}, P2Wins: {(float)IterationOutput.P2Wins / (ExecutionTimes * 2)}, " +
                $"Draws: {(float)IterationOutput.Draws / (ExecutionTimes * 2)} Average Duration: {IterationOutput.avgDuration} ");

            IterationOutput.Draws = 0; 
            IterationOutput.P1Wins = 0;
            IterationOutput.P2Wins = 0;
            IterationOutput.avgDuration = 0;
        }

        private void PlayNewGame(bool P1Oya)
        {
            VirtualBoard newBoard = BuildRandomBoard(P1Oya);
            while (true)
            {
                Log("");
                for (int playerID = 0; playerID < newBoard.Players.Count; playerID++)
                {
                    if (newBoard.Players[playerID].Hand.Count == 0)
                    {
                        EndGame();
                        return;
                    }

                    Settings.PlayerID = 1 - playerID;
                    Move selectedMove = ((IArtificialIntelligence)newBoard.Players[playerID]).MakeTurn(newBoard);

                    selectedMove.PlayerID = playerID;
                    selectedMove.DeckSelection = newBoard.Deck[0].Title;

                    newBoard.ApplyMove(selectedMove);
                    Log(PlayerAction.FromMove(selectedMove, newBoard).ToString());
                    if (selectedMove.HadYaku && !selectedMove.Koikoi)
                    {
                        EndGame(newBoard.Players[selectedMove.PlayerID]);
                        return;
                    }
                }

            }
        }

        private void EndGame(Player player = null)
        {

            if (player == null)
            {
                IterationOutput.avgDuration += 8f / 40f;
                IterationOutput.Draws++;
                Log("Unentschieden. Keine Karten mehr auf der Hand.", LogType.Error);
                return;
            }
            IterationOutput.avgDuration += (8f - player.Hand.Count) / ExecutionTimes;
            if (player.Name == "Player 1")
                IterationOutput.P1Wins++;
            else
                IterationOutput.P2Wins++;
            List<Yaku> yakuList = Yaku.GetNewYakus(Enumerable.Range(0, Global.allYaku.Count).ToDictionary(x => x, x => 0), player.CollectedCards);
            Log($"{player.Name} sammelt {string.Join(",", yakuList.Select(x => x.Title))}", LogType.Warning);
            Log($"{player.Name} hat nicht Koi Koi gesagt. Die Runde ist beendet.", LogType.Warning);
        }

        private VirtualBoard BuildRandomBoard(bool P1Oya)
        {
            VirtualBoard newBoard = new VirtualBoard();

            if (P1Oya)
                newBoard.Players = new List<Player>() { KI.Init(P1Mode, "Player 1"), KI.Init(P2Mode, "Player 2") };
            else
                newBoard.Players = new List<Player>() { KI.Init(P2Mode, "Player 2"), KI.Init(P1Mode, "Player 1") };
            Settings.Players = newBoard.Players;
            Log($"{newBoard.Players[0].Name} ist der Oya");
            Log($"Player 1 Type: {(P1Oya ? P1Mode : P2Mode).ToString()}");
            Log($"Player 2 Type: {(P1Oya ? P2Mode : P1Mode).ToString()}");

            newBoard.Deck = BuildRandomDeck();
            newBoard.Field = newBoard.Deck.Take(8).ToList();
            newBoard.Deck.RemoveRange(0, 8);
            Log($"Field cards: {string.Join(",", newBoard.Field)}");

            newBoard.Players[0].Hand = newBoard.Deck.Take(8).ToList();
            newBoard.Deck.RemoveRange(0, 8);
            Log($"{newBoard.Players[0].Name} cards: {string.Join(",", newBoard.Players[0].Hand)}");

            newBoard.Players[1].Hand = newBoard.Deck.Take(8).ToList();
            newBoard.Deck.RemoveRange(0, 8);
            Log($"{newBoard.Players[1].Name} cards: {string.Join(",", newBoard.Players[1].Hand)}");

            return newBoard;
        }

        private List<Card> BuildRandomDeck()
        {
            List<Card> Deck = new List<Card>();
            var rnd = new Random();
            List<int> indices = Enumerable.Range(0, Global.allCards.Count).ToList();
            Deck.Clear();
            for (var i = indices.Count - 1; i >= 0; i--)
            {
                var rand = rnd.Next(0, indices.Count);
                Deck.Add(Global.allCards[indices[rand]]);
                indices.RemoveAt(rand);
            }

            Log($"Created new random deck");
            return Deck;
        }

        private enum LogType { Info, Warning, Error }
        private void Log(string text, LogType type = LogType.Info) => Global.Log(text, true);
        /*{
            return;
            Text newText = new GameObject().AddComponent<Text>();
            newText.text = text;
            newText.alignment = TextAnchor.MiddleLeft;
            newText.font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;

            switch (type)
            {
                case LogType.Info:
                    newText.color = InfoColor;
                    break;
                case LogType.Warning:
                    newText.color = WarningColor;
                    break;
                case LogType.Error:
                    newText.color = ErrorColor;
                    break;
            }

            newText.transform.SetParent(Content, false);

            Canvas.ForceUpdateCanvases();
            Content.transform.parent.GetComponent<ScrollRect>().normalizedPosition = new Vector2(0, 0);
        }*/
    }
}