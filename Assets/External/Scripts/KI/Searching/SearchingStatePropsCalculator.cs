using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hanafuda
{
    public class SearchingStatePropsCalculator
    {
        private readonly SearchingBoard _board;

        private readonly int _roundsLeft;
        private readonly List<int> _yakuDurations;
        private readonly List<int> _yakuProgresses;
        private readonly List<float> _yakuOpponentDependencies;
        private readonly Dictionary<int, int> _stateYakus;

        public SearchingStatePropsCalculator(SearchingBoard board)
        {
            _board = board;
            _roundsLeft = _board.computerHand.Count;
            _stateYakus = Enumerable.Range(0, Global.allYaku.Count).ToDictionary(x => x, x => 0);
            _yakuOpponentDependencies = Enumerable.Repeat(0f, Global.allYaku.Count).ToList();

            _yakuDurations = GetYakuDurations(_board);
            _yakuProgresses = GetYakuProgresses();
        }

        public float GetYakuDurationValue(int aggregationSize = 1)
        {
            float yakuDurationValue = _yakuDurations
                .Select((x, y) => _roundsLeft - (x > _roundsLeft ? _roundsLeft : x) * _yakuOpponentDependencies[y])
                .OrderBy(x => x)
                .Take(aggregationSize)
                .Sum();

            return yakuDurationValue;
        }

        public float GetYakuProgressValue(int aggregationSize = 1)
        {
            var maxima = _yakuProgresses
                .Select((x, y) => (Global.allYaku[y].minSize - x).Faculty() * _yakuOpponentDependencies[y])
                .OrderBy(x => x)
                .Take(aggregationSize)
                .ToList();

            float yakuProgressValue = maxima.Average(
                x => 1f / x);

            return yakuProgressValue;
        }

        private List<int> GetYakuProgresses()
        {
            var yakuProgresses = Enumerable.Repeat(0, Global.allYaku.Count).ToList();

            foreach (var pair in _stateYakus)
            {
                if (pair.Value > yakuProgresses[pair.Key])
                    yakuProgresses[pair.Key] = pair.Value;
            }

            return yakuProgresses;
        }

        private List<int> GetYakuDurations(SearchingBoard board)
        {
            var yakuDurations = Enumerable.Repeat(9, Global.allYaku.Count).ToList();

            int lastIndex = 0;
            for (int partID = 0; partID < board.CardsCollected.Count; partID++)
            {
                int count = board.CardsCollected[partID];
                if (count == 0)
                    continue;
                var currentRange = board.computerCollection.GetRange(lastIndex, count);
                lastIndex += count;

                List<Yaku> yakus = new List<Yaku>();
                foreach (var card in currentRange)
                {
                    for (int yakuId = 0; yakuId < Global.allYaku.Count; yakuId++)
                    {
                        var yaku = Global.allYaku[yakuId];
                        if (card == yaku)
                        {
                            _stateYakus[yakuId]++;
                            _yakuOpponentDependencies[yakuId] += GetOpponentDependency(card);

                            if ((_stateYakus[yakuId] > yaku.minSize && yaku.addPoints > 0)
                                || _stateYakus[yakuId] == yaku.minSize)
                                yakus.Add(yaku);
                        }
                    }
                }

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

        private float GetOpponentDependency(Card card)
        {
            var result = 0f;
            var root = _board.parent;

            var playerMatches = root.playerHand.Count(x => x.Monat == card.Monat);

            for (int deckIndex = 0; deckIndex < root.playerHand.Count; deckIndex++)
            {
                if (root.Deck[deckIndex * 2 + 1].Monat == card.Monat)
                    playerMatches++;
            }

            result += playerMatches / 3f;

            return result;
        }
    }
}
