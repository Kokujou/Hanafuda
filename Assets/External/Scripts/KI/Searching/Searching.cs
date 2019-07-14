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

        private Dictionary<int, float> StateValues = new Dictionary<int, float>();
        public override float RateState(SearchingBoard state) => 
            state.computerHand.Count <= 1 ? 0 : StateValues[Tree.GetLevel(1).IndexOf(state)];

        private int Faculty(uint number)
        {
            if (number <= 1) return 1;
            uint result = number;
            while (number > 1)
            {
                result *= number;
                number--;
            }
            return (int)result;
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
            float yakuProgressValue = yakuProgess.Average(x => 1f / Faculty((uint)(Global.allYaku[x.Key].minSize - x.Value)));

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
                    if (count == 0) continue;
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
                SearchingBoard state = Tree.GetState(1, stateID);
                Result[stateID] = RateSingleState(yakuDurations[stateID], yakuProgress[stateID], null);
            }
            return Result;
        }

        protected override void BuildStateTree(IHanafudaBoard cRoot, int playerID)
        {
            System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
            watch.Start();
            Global.Log("Searching AI Tree Building started");
            SearchingBoard root = new SearchingBoard(cRoot, playerID);
            root.Turn = true;
            Tree = new SearchingStateTree(new SearchingBoard(root));
            Tree.Build(skipOpponent: true);
            Global.Log($"State Tree Building Time: {watch.ElapsedMilliseconds}");
            Global.Log($"State Tree Last Count: {Tree.GetLevel(Tree.Size - 1).Count}");
            StateValues = RateFirstLevel();
        }

        public override Move RequestDeckSelection(Spielfeld board, Move baseMove, int playerID)
        {
            throw new NotImplementedException();
        }
    }
}