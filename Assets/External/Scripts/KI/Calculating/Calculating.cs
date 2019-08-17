using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace Hanafuda
{
    public partial class CalculatingAI : KI<UninformedBoard>
    {
        const string _LocalWeight = "_LocalWeight";
        const string _GlobalWeight = "_GlobalWeight";
        const string _DeckWeight = "_DeckWeight";
        const string _CollectionWeight = "_CollectionWeight";

        public CalculatingAI(string name) : base(name) { }

        protected override void BuildStateTree(IHanafudaBoard cRoot, int playerID)
        {
            var root = new UninformedBoard(cRoot, playerID);
            root.Turn = true;
            Tree = new UninformedStateTree(root);
            Tree.Build(1, skipOpponent: true);
        }

        private Dictionary<string, float> weights = new Dictionary<string, float>()
        {
            { _GlobalWeight, 20 },
            { _LocalWeight, 100 },
            { _CollectionWeight, 5 },
        };

        public override Dictionary<string, float> GetWeights() => weights;

        public override void SetWeight(string name, float value)
        {
            float temp;
            if (weights.TryGetValue(name, out temp))
                weights[name] = value;
        }

        public override Move RequestDeckSelection(IHanafudaBoard root, Move baseMove, int playerID)
        {
            var uninformedRoot = new UninformedBoard(root, playerID);
            var deckCard = uninformedRoot.UnknownCards.First(x => x.Key.Title == baseMove.DeckSelection).Key;
            List<Card> matches = root.Field.FindAll(x => x.Monat == deckCard.Monat);

            float maxValue = float.NegativeInfinity;
            Card selection = null;
            foreach (Card card in matches)
            {
                baseMove.DeckFieldSelection = card.Title;
                UninformedBoard child = uninformedRoot.Clone();
                UninformedBoard board = uninformedRoot.ApplyMove(child, baseMove, true);
                float value = RateState(board);
                if (value > maxValue)
                {
                    maxValue = value;
                    selection = card;
                }
            }

            baseMove.DeckFieldSelection = selection.Title;
            return baseMove;
        }

        public override Move MakeTurn(IHanafudaBoard cRoot, int playerID)
        {
            Move selectedMove = base.MakeTurn(cRoot, playerID);
            selectedMove.DeckSelection = cRoot.Deck[0].Title;

            if (cRoot.Field.Count(x => x.Monat == cRoot.Deck[0].Monat) == 2)
                RequestDeckSelection(cRoot, selectedMove, playerID);

            return selectedMove;
        }

        public override float RateState(UninformedBoard state)
        {
            float Result = 0f;
            Global.Log($"Zustand {state.GetHashCode()}: {state.LastMove.ToString().Replace("\n", "")}");
            if (state.isFinal)
                return Mathf.Infinity;
            if (state.computer.Hand.Count <= 1)
                return 0;

            float ComGlobalMaximum, ComLocalMaximum, ComCollectionValue;
            float PGlobalMaximum = float.NegativeInfinity;
            float PLocalMaximum = float.NegativeInfinity;
            float PCollectionValue = float.NegativeInfinity;

            var comUninformedStatePropsCalculator = new UninformedStatePropsCalculator(state, CardProps, true);
            ComGlobalMaximum = comUninformedStatePropsCalculator.GetGlobalMaximum();
            ComLocalMaximum = comUninformedStatePropsCalculator.GetLocalMaximum();
            ComCollectionValue = comUninformedStatePropsCalculator.GetCollectionValue();

            var nextPlayerStates = GetNextPlayerStates(state);
            foreach(var playerState in nextPlayerStates)
            {
                var pUninformedStatePropsCalculator = 
                    new UninformedStatePropsCalculator(playerState, CardProps, false);
                var currentGlobalMaximum = pUninformedStatePropsCalculator.GetGlobalMaximum();
                var currentLocalMaximum = pUninformedStatePropsCalculator.GetLocalMaximum();
                var currentCollectionValue = pUninformedStatePropsCalculator.GetCollectionValue();

                if (currentGlobalMaximum > PGlobalMaximum)
                    PGlobalMaximum = currentGlobalMaximum;
                if (currentLocalMaximum > PLocalMaximum)
                    PLocalMaximum = currentLocalMaximum;
                if (currentCollectionValue > PCollectionValue)
                    PCollectionValue = currentCollectionValue;
            }

            Result = (ComGlobalMaximum - PGlobalMaximum) * weights[_GlobalWeight]
                + (ComLocalMaximum - PLocalMaximum) * weights[_LocalWeight]
                + (ComCollectionValue - PCollectionValue) * weights[_CollectionWeight];

            return Result;
        }

        private List<UninformedBoard> GetNextPlayerStates(UninformedBoard state)
        {
            UninformedStateTree PlayerTree = new UninformedStateTree(state);
            PlayerTree.Build(1, false, true);
            return PlayerTree.GetLevel(1);
        }
    }
}