using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hanafuda
{
    public class SearchingStatePropsCalculator
    {
        private readonly List<int> _yakuDurations;
        private readonly List<int> _yakuProgresses;

        public SearchingStatePropsCalculator(SearchingBoard board)
        {
            List<int> stateYakus = Enumerable.Repeat(0, Global.allYaku.Count).ToList();

            /*
             * Get Yaku Fullfillment in every Partition of Collected Cards
             */
            int lastIndex = 0;
            for (int partID = 0; partID < board.CardsCollected.Count; partID++)
            {
                int count = board.CardsCollected[partID];
                if (count == 0)
                    continue;
                List<Yaku> yakus = Yaku.GetNewYakus(, board.computerCollection.GetRange(lastIndex, count), true);
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

        public float GetYakuDurationValue()
        {

        }

        public float GetYakuProgressValue()
        {

        }
    }
}
