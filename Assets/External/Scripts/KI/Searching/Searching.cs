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
            { _YakuDurationWeight, 1 },
            { _YakuProgressWeight, 1 },
            { _OpponentDependenceWeight, 1 },
            { _YakuQualityWeight, 1 }
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

        private void CorrectMove(Move move, SearchingBoard state)
        {
            var handSelection = Global.allCards.FirstOrDefault(x => x.Title == move.HandSelection);
            var deckSelection = Global.allCards.FirstOrDefault(x => x.Title == move.DeckSelection);
            var handMatches = state.Field.FindAll(x => x.Monat == handSelection.Monat);
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
            var deckMatches = state.Field.FindAll(x => x.Monat == deckSelection.Monat);
            if (deckMatches.Count == 2)
                move.DeckFieldSelection = ChooseBestCard(deckMatches).Title;

            if (Yaku.GetNewYakus(Enumerable.Range(0, Global.allYaku.Count).ToDictionary(x => x, x => 0), state.computerCollection).Count > 0)
            {
                state.LastMove.HadYaku = true;
                state.LastMove.Koikoi = false;
            }
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

        private List<float> StateValues = new List<float>();
        public override float RateState(SearchingBoard state)
        {
            var result = 0f;

            CorrectMove(state.LastMove, state);

            var statePropsCalculator = new SearchingStatePropsCalculator();
            var yakuDurationValue = statePropsCalculator.GetYakuDurationValue();
            var yakuProgressValue = statePropsCalculator.GetYakuProgressValue();

            result = yakuDurationValue * weights[_YakuDurationWeight]
                + yakuProgressValue * weights[_YakuProgressWeight];

            return result;
        }

        /// <summary>
        /// Errechne einen Zustandswert aus untenstehenden Werten
        /// </summary>
        /// <param name="yakuDurations">Zeit für jeden Yaku, bis er eingesammelt wird. (Zuweisung durch Index)</param>
        /// <param name="yakuProgess">Karten, die im letzten Zustand zur Erreichung des Yaku Fehlen. (Zuweisung durch Index)</param>
        /// <param name="yakuOppDependencies">Gibt an, wie abhängig die Yaku von gegnerischen Karten sind (Zuweisung durch Index)</param>
        /// <returns></returns>
        private float RateSingleState(List<int> yakuDurations, List<int> yakuProgess, List<float> yakuOppDependencies)
        {
            float Result = 0f;

            int turnsLeft;
            float yakuDurationValue = yakuDurations.Sum(x => 8 - (x > 8 ? 8 : x));

            int index = 0;
            float yakuProgressValue = yakuProgess.Average(x => 1f / (Global.allYaku[index++].minSize - x).Faculty());

            Result = yakuDurationValue * weights[_YakuDurationWeight]
                + yakuProgressValue * weights[_YakuProgressWeight];
            return Result;
        }



        private List<float> RateFirstLevel()
        {
            List<float> Result = Enumerable.Repeat(0f, Tree.GetLevel(1).Count).ToList();

            List<List<int>> yakuProgress = Enumerable.Repeat(
                Enumerable.Repeat(0, Global.allYaku.Count).ToList(),
                Tree.GetLevel(1).Count)
                .ToList();

            List<List<int>> yakuDurations = Enumerable.Repeat(
                Enumerable.Repeat(9, Global.allYaku.Count).ToList(),
                Tree.GetLevel(1).Count)
                .ToList();

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

        public override Move RequestDeckSelection(IHanafudaBoard board, Move baseMove, int playerID)
        {
            throw new NotImplementedException();
        }
    }
}