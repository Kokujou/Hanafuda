using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Hanafuda
{
    public partial class SearchingAI : KI<SearchingBoard>
    {
        const string _YakuDurationWeight = "_YakuDurationWeight";
        const string _YakuProgressWeight = "_YakuProgressWeight";
        const string _OpponentDependenceWeight = "_OpponentDependenceWeight";
        const string _YakuQualityWeight = "_YakuQualityWeight";

        private Dictionary<string, float> weights = new Dictionary<string, float>()
        {
            { _YakuDurationWeight, 0 },
            { _YakuProgressWeight, 0 },
            { _OpponentDependenceWeight, 0 },
            { _YakuQualityWeight, 0 }
        };

        public override Dictionary<string, float> GetWeights() => weights;

        public override void SetWeight(string name, float value)
        {
            float temp;
            if (weights.TryGetValue(name, out temp))
                weights[name] = value;
        }

        public SearchingAI(string name) : base(name)
        {
        }

        public override Move MakeTurn(IHanafudaBoard board, int playerID)
        {
            Debug.Log("KI Turn Decision started");
            BuildStateTree(board, playerID);
            //Bewertung möglicherweise in Threads?
            float maxValue = float.NegativeInfinity;
            SearchingBoard selectedBoard = null;

            List<SearchingBoard> firstLevel = Tree.GetLevel(1);
            foreach (SearchingBoard state in firstLevel)
                state.Value = (float)new System.Random().NextDouble();

            //Parallel.ForEach(Tree.GetLevel(1), state => state.Value = RateState(state));

            foreach (SearchingBoard state in firstLevel)
            {
                if (state.Value > maxValue)
                {
                    maxValue = state.Value;
                    selectedBoard = state;
                }
            }
            CorrectMove(selectedBoard.LastMove, board);

            if (Yaku.GetNewYakus(Enumerable.Range(0, Global.allYaku.Count).ToDictionary(x => x, x => 0), selectedBoard.computerCollection).Count > 0)
            {
                selectedBoard.LastMove.HadYaku = true;
                selectedBoard.LastMove.Koikoi = false;
            }

            Global.Log($"{selectedBoard}", true);
            return selectedBoard.LastMove;
        }

        private void CorrectMove(Move move, IHanafudaBoard parent)
        {
            var handSelection = Global.allCards.FirstOrDefault(x => x.Title == move.HandSelection);
            var deckSelection = Global.allCards.FirstOrDefault(x => x.Title == move.DeckSelection);
            var handMatches = parent.Field.FindAll(x => x.Monat == handSelection.Monat);
            if (handMatches.Count == 2)
            {
                if (handSelection.Monat == deckSelection.Monat)
                {
                    move.HandFieldSelection = handMatches[0].Title;
                    move.DeckFieldSelection = handMatches[1].Title;
                    return;
                }
                else
                    move.HandFieldSelection = ChooseBestCard(handMatches).Title;
            }
            var deckMatches = parent.Field.FindAll(x => x.Monat == deckSelection.Monat);
            if (deckMatches.Count == 2)
                move.DeckFieldSelection = ChooseBestCard(deckMatches).Title;
        }

        private Card ChooseBestCard(List<Card> cards)
        {
            CardProperties bestProperty = null;
            foreach (var card in cards)
            {
                var cardProperty = CardProps.FirstOrDefault(x => x.card.Title == card.Title);
                if (bestProperty == null ||
                    cardProperty.RelevanceForYaku.Sum(x => x.Value) > bestProperty.RelevanceForYaku.Sum(x => x.Value))
                    bestProperty = cardProperty;
            }
            return bestProperty.card;
        }

        private Dictionary<int, float> StateValues = new Dictionary<int, float>();
        public override float RateState(SearchingBoard state) =>
            state.computerHand.Count <= 1 ? 0 : StateValues[Tree.GetLevel(1).IndexOf(state)];

        private long Faculty(long number)
        {
            if (number <= 1)
                return 1;
            long result = number;
            while (number > 1)
            {
                result *= number;
                number--;
            }
            return result;
        }

        /// <summary>
        /// Errechne einen Zustandswert aus untenstehenden Werten
        /// </summary>
        /// <param name="yakuDurations">Zeit für jeden Yaku, bis er eingesammelt wird. (Zuweisung durch Index)</param>
        /// <param name="yakuProgess">Karten, die im letzten Zustand zur Erreichung des Yaku Fehlen. (Zuweisung durch Index)</param>
        /// <param name="yakuOppDependencies">Gibt an, wie abhängig die Yaku von gegnerischen Karten sind (Zuweisung durch Index)</param>
        /// <returns></returns>
        private float RateSingleState(Dictionary<int, int> yakuDurations, Dictionary<int, int> yakuProgess, Dictionary<int, float> yakuOppDependencies)
        {
            float Result = 0f;

            float yakuDurationValue = yakuDurations.Sum(x => 8 - (x.Value > 8 ? 8 : x.Value));

            int maxMinSize = Global.allYaku.Max(x => x.minSize);
            float yakuProgressValue = yakuProgess.Average(x => 1f / Faculty((Global.allYaku[x.Key].minSize - x.Value)));

            Result = yakuDurationValue * weights[_YakuDurationWeight]
                + yakuProgressValue * weights[_YakuProgressWeight];

            return Result;
        }



        private Dictionary<int, float> RateFirstLevel()
        {
            Dictionary<int, float> Result = Enumerable.Range(0, Tree.GetLevel(1).Count).ToDictionary(x => x, x => 0f);

            Dictionary<int, Dictionary<int, int>> yakuProgress = Enumerable.Range(0, Tree.GetLevel(1).Count).ToDictionary(x => x,
                x => Enumerable.Range(0, Global.allYaku.Count).ToDictionary(y => y, y => 0));
            Dictionary<int, Dictionary<int, int>> yakuDurations = Enumerable.Range(0, Tree.GetLevel(1).Count).ToDictionary(x => x,
                 x => Enumerable.Range(0, Global.allYaku.Count).ToDictionary(y => y, y => 9));
            foreach (SearchingBoard board in Tree.GetLevel(Tree.Size - 1))
            {
                Dictionary<int, int> stateYakus = Enumerable.Range(0, Global.allYaku.Count).ToDictionary(x => x, x => 0);

                /*
                 * Get Yaku Fullfillment in every Partition of Collected Cards
                 */
                int lastIndex = 0;
                for (int partID = 0; partID < board.CardsCollected.Count; partID++)
                {
                    int count = board.CardsCollected[partID];
                    if (count == 0)
                        continue;
                    List<Yaku> yakus = Yaku.GetNewYakus(stateYakus, board.computerCollection.GetRange(lastIndex, count), true);
                    lastIndex += count;

                    /*
                     * Set Global Minimum for all new Yaku if they are earlier
                     */
                    if (yakus.Count > 0)
                    {
                        foreach (Yaku yaku in yakus)
                        {
                            int yakuID = Global.allYaku.FindIndex(x => yaku.Title == x.Title);
                            if (partID < yakuDurations[board.Root][yakuID])
                                yakuDurations[board.Root][yakuID] = partID;
                        }
                    }
                }

                /*
                 * Set Global Minimum for all Yaku if they need less cards
                 */
                foreach (var pair in stateYakus)
                {
                    if (pair.Value > yakuProgress[board.Root][pair.Key])
                        yakuProgress[board.Root][pair.Key] = pair.Value;
                }
            }

            /*
             * Calculate the final Values
             */
            for (int stateID = 0; stateID < Tree.GetLevel(1).Count; stateID++)
            {
                if (Tree.GetLevel(1)[stateID].isFinal)
                    Result[stateID] = Mathf.Infinity;
                else
                    Result[stateID] = RateSingleState(yakuDurations[stateID], yakuProgress[stateID], null);
            }
            return Result;
        }

        protected override void BuildStateTree(IHanafudaBoard cRoot, int playerID)
        {
            SearchingBoard root = new SearchingBoard(cRoot, playerID);
            root.Turn = true;
            Tree = new SearchingStateTree(new SearchingBoard(root));
            Tree.Build(skipOpponent: true);
            StateValues.Clear();
            StateValues = RateFirstLevel();
        }

        public override Move RequestDeckSelection(Spielfeld board, Move baseMove, int playerID)
        {
            throw new NotImplementedException();
        }
    }
}