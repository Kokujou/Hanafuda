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

        private Dictionary<string, float> weights = new Dictionary<string, float>()
        {
            { _YakuDurationWeight, 1 },
            { _YakuProgressWeight, 1 },
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

            var correctedState = CorrectState(state);
            state.LastMove = correctedState.LastMove;

            if (correctedState.isFinal)
                return float.PositiveInfinity;

            var statePropsCalculator = new SearchingStatePropsCalculator(correctedState);
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

        private SearchingBoard CorrectState(SearchingBoard state)
        {
            var correctedMove = CorrectMove(state.LastMove, Tree.Root.Field);
            var correctedState = Tree.Root.ApplyMove(Tree.Root, correctedMove, true);

            return correctedState;
        }

        private Move CorrectMove(Move oldMove, List<Card> oldField)
        {
            var correctedMove = new Move(oldMove);

            var handSelection = Global.allCards.FirstOrDefault(x => x.Title == correctedMove.HandSelection);
            var deckSelection = Global.allCards.FirstOrDefault(x => x.Title == correctedMove.DeckSelection);

            var handMatches = oldField.FindAll(x => x.Monat == handSelection.Monat);
            var deckMatches = oldField.FindAll(x => x.Monat == deckSelection.Monat);
            if (handMatches.Count == 2)
                correctedMove.HandFieldSelection = ChooseBestCard(handMatches).Title;
            if (deckMatches.Count == 2 && handSelection.Monat != deckSelection.Monat)
                correctedMove.DeckFieldSelection = ChooseBestCard(deckMatches).Title;

            return correctedMove;
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