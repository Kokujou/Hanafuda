
using Hanafuda.Base;
using Hanafuda.Base.Interfaces;
using System.Collections.Generic;
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
        private AIMode P1Mode, P2Mode;

        private Player P1KI, P2KI;

        public Color InfoColor, WarningColor, ErrorColor;

        public struct IterationOutput
        {
            public int P1Wins { get; set; }
            public int P2Wins { get; set; }
            public int Draws { get; set; }
            public float avgDuration { get; set; }
        }

        public void StartSimulation()
        {
            ExecutionTimes = Times.Value;
            P1Mode = (AIMode)Player1.value;
            P2Mode = (AIMode)Player2.value;

            object synchronizeOutput = new object();
            IterationOutput totalOutput = new IterationOutput();
            Random random = new Random();
            object synchronizeRandom = new object();

            Parallel.For<IterationOutput>(0, ExecutionTimes, () => new IterationOutput(), (round, state, output) =>
            {
                PlayNewGame(true, output);
                return output;
            }, finalOutput =>
            {
                lock (synchronizeOutput)
                {
                    totalOutput.P1Wins += finalOutput.P1Wins;
                    totalOutput.P2Wins += finalOutput.P2Wins;
                    totalOutput.Draws += finalOutput.Draws;
                }
            });

            Log($"\n\nP1Wins: {(float)totalOutput.P1Wins / (ExecutionTimes * 2)}, P2Wins: {(float)totalOutput.P2Wins / (ExecutionTimes * 2)}, " +
                $"Draws: {(float)totalOutput.Draws / (ExecutionTimes * 2)} Average Duration: {totalOutput.avgDuration} ");

            totalOutput.Draws = 0;
            totalOutput.P1Wins = 0;
            totalOutput.P2Wins = 0;
            totalOutput.avgDuration = 0;
        }

        private void PlayNewGame(bool P1Oya, IterationOutput output)
        {
            VirtualBoard newBoard = BuildRandomBoard(P1Oya);
            while (true)
            {
                Log("");
                for (int playerID = 0; playerID < newBoard.Players.Count; playerID++)
                {
                    if (newBoard.Players[playerID].Hand.Count == 0)
                    {
                        EndGame(output);
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
                        EndGame(output, newBoard.Players[selectedMove.PlayerID]);
                        return;
                    }
                }

            }
        }

        private void EndGame(IterationOutput output, Player player = null)
        {
            if (player == null)
            {
                output.avgDuration += 8f / 40f;
                output.Draws++;
                Log("Unentschieden. Keine Karten mehr auf der Hand.", LogType.Error);
                return;
            }
            output.avgDuration += (8f - player.Hand.Count) / ExecutionTimes;
            if (player.Name == "Player 1")
                output.P1Wins++;
            else
                output.P2Wins++;
            List<Yaku> yakuList = YakuMethods.GetNewYakus(Enumerable.Range(0, Global.allYaku.Count).ToDictionary(x => x, x => 0), player.CollectedCards);
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

        private List<ICard> BuildRandomDeck()
        {
            List<ICard> Deck = new List<ICard>();
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