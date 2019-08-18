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

        public SearchingAI(string name) : base(name) { }

        public override float RateState(SearchingBoard state)
        {
            var result = 0f;

            CorrectMove(state.LastMove, state);

            if (state.isFinal)
                return float.PositiveInfinity;

            var statePropsCalculator = new SearchingStatePropsCalculator(state);
            var yakuDurationValue = statePropsCalculator.GetYakuDurationValue();
            var yakuProgressValue = statePropsCalculator.GetYakuProgressValue();

            result = yakuDurationValue * weights[_YakuDurationWeight]
                + yakuProgressValue * weights[_YakuProgressWeight];

            return result;
        }

        protected override void BuildStateTree(IHanafudaBoard cRoot, int playerID)
        {
            SearchingBoard root = new SearchingBoard(cRoot, playerID);
            root.Turn = true;
            Tree = new SearchingStateTree(new SearchingBoard(root));
            Tree.Build(skipOpponent: true);
        }

        public override Move RequestDeckSelection(IHanafudaBoard board, Move baseMove, int playerID)
            => throw new NotImplementedException();

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
    }
}