using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hanafuda
{
    public class SearchingStatePropsCalculator
    {
        private readonly int _roundsLeft;
        private readonly List<int> _yakuDurations;
        private readonly List<int> _yakuProgresses;
        private readonly Dictionary<int, int> _stateYakus;

        public SearchingStatePropsCalculator(SearchingBoard board)
        {
            _roundsLeft = board.computer.Hand.Count;
            _stateYakus = Enumerable.Range(0, Global.allYaku.Count).ToDictionary(x => x, x => 0);

            _yakuDurations = GetYakuDurations(board);
            _yakuProgresses = GetYakuProgresses();
        }

        public float GetYakuDurationValue()
        {
            float yakuDurationValue = _yakuDurations.Sum(x => 8 - (x > 8 ? 8 : x));
            return yakuDurationValue;
        }

        public float GetYakuProgressValue()
        {
            int index = 0;
            float yakuProgressValue = _yakuProgresses.Average(x => 1f / (Global.allYaku[index++].minSize - x).Faculty());
            return yakuProgressValue;
        }

        private List<int> GetYakuProgresses()
        {
            var yakuProgresses = new List<int>();

            foreach (var pair in _stateYakus)
            {
                if (pair.Value > yakuProgresses[pair.Key])
                    yakuProgresses[pair.Key] = pair.Value;
            }

            return yakuProgresses;
        }

        private List<int> GetYakuDurations(SearchingBoard board)
        {
            var yakuDurations = new List<int>();

            int lastIndex = 0;
            for (int partID = 0; partID < board.CardsCollected.Count; partID++)
            {
                int count = board.CardsCollected[partID];
                if (count == 0)
                    continue;
                List<Yaku> yakus = Yaku.GetNewYakus(_stateYakus, board.computerCollection.GetRange(lastIndex, count), true);
                lastIndex += count;

                /*
                 * Set Global Minimum for all new Yaku if they are earlier
                 */
                foreach (Yaku yaku in yakus)
                {
                    int yakuID = Global.allYaku.FindIndex(x => yaku.Title == x.Title);
                    if (partID < yakuDurations[yakuID])
                        yakuDurations[yakuID] = partID;
                }
            }

            return yakuDurations;
        }
    }
}
