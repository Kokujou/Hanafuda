using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Hanafuda
{
    public partial class CalculatingAI : KI<UninformedBoard>
    {
        const string _LocalWeight = "_LocalWeight";
        const string _GlobalWeight = "_GlobalWeight";
        const string _DeckWeight = "_DeckWeight";

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
            { _LocalWeight, 5 },
            { _GlobalWeight, 100 },
            { _DeckWeight, 0 },
        };

        public override Dictionary<string, float> GetWeights() => weights;

        public override void SetWeight(string name, float value)
        {
            float temp;
            if (weights.TryGetValue(name, out temp))
                weights[name] = value;
        }

        public override Move RequestDeckSelection(Spielfeld root, Move baseMove, int playerID)
        {
            var uninformedRoot = new UninformedBoard(root, playerID);
            var deckCard = uninformedRoot.UnknownCards.First(x => x.Key.Title == baseMove.DeckSelection).Key;
            List<Card> matches = root.Field.FindAll(x => x.Monat == deckCard.Monat);
            if (matches.Count != 2)
                return baseMove;
            float maxValue = -100f;
            Card selection = null;
            foreach (Card card in matches)
            {
                baseMove.DeckFieldSelection = card.Title;
                UninformedBoard child = uninformedRoot.Clone();
                UninformedBoard board = uninformedRoot.ApplyMove(child, baseMove, true);
                float value = RateState(board);
                if (value > maxValue || selection == null)
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
            List<Card> matches = cRoot.Field.FindAll(x => x.Monat == cRoot.Deck[0].Monat);
            if (matches.Count == 2)
            {
                float maxValue = -100f;
                Card selection = null;
                foreach (Card card in matches)
                {
                    selectedMove.DeckFieldSelection = card.Title;
                    UninformedBoard root = new UninformedBoard(cRoot, playerID);
                    UninformedBoard board = root.ApplyMove(root, selectedMove, true);
                    float value = RateState(board);
                    if (value > maxValue || selection == null)
                    {
                        maxValue = value;
                        selection = card;
                    }
                }
                selectedMove.DeckFieldSelection = selection.Title;
            }
            return selectedMove;
        }
        public override float RateState(UninformedBoard State)
        {
            Global.Log($"Zustand {State.GetHashCode()}: {State.LastMove.ToString().Replace("\n", "")}");
            if (State.isFinal)
                return Mathf.Infinity;
            if (State.computer.Hand.Count <= 1)
                return 0;

            float Result = 0f;

            UninformedStateProps ComStateProps = RateSingleState(State, true);

            UninformedStateTree PlayerTree = new UninformedStateTree(State);
            PlayerTree.Build(1, false, true);
            List<UninformedBoard> PStates = PlayerTree.GetLevel(1);
            List<UninformedStateProps> PStateProps = new List<UninformedStateProps>();

            foreach (UninformedBoard PState in PStates)
                PStateProps.Add(RateSingleState(PState, false));

            float PLocalMinimum = 0;
            float PGlobalMinimum = 0;
            float PDeckValue = 0;
            if (PStateProps.Count > 0)
            {
                PLocalMinimum = PStateProps.Max(x => x.LocalMinimum);
                PGlobalMinimum = PStateProps.Max(x => x.GlobalMinimum.Probability);
            }

            Result = (((8 - ComStateProps.GlobalMinimum.MinTurns) * ComStateProps.GlobalMinimum.Probability) - PGlobalMinimum) * weights[_GlobalWeight]
                + (ComStateProps.LocalMinimum - PLocalMinimum) * weights[_LocalWeight]
                + (ComStateProps.DeckValue - PDeckValue) * weights[_DeckWeight];

            return Result;
        }

        private struct UninformedStateProps
        {
            public YakuProperties GlobalMinimum;
            public float LocalMinimum;
            public float DeckValue;
            public float SelectionProbability;
        }
        private UninformedStateProps RateSingleState(UninformedBoard State, bool Turn)
        {
            UninformedStateProps Result = new UninformedStateProps();

            List<Card> activeCollection = Turn ? State.computer.CollectedCards : State.OpponentCollection;
            Dictionary<Card, float> activeHand = Turn ? State.computer.Hand.ToDictionary(x => x, x => 1f) : State.UnknownCards;
            int activeHandSize = Turn ? State.computer.Hand.Count : State.OpponentHandSize;

            List<Card> NewCards = new List<Card>();
            if (State.LastMove != null)
            {
                UninformedBoard state = State.parent;
                NewCards = activeCollection.Except(Turn ? state.computer.CollectedCards : state.OpponentCollection).ToList();
            }

            if (Turn)
                Result.SelectionProbability = 1;
            else
            {
                var handSelection = State.parent
                    .UnknownCards.First(x => x.Key.Title == State.LastMove.HandSelection);
                Result.SelectionProbability = handSelection.Value;
                State.UnknownCards.Remove(handSelection.Key);
            }

            UninformedCards cardProps = new UninformedCards(CardProps, State, Turn);
            YakuCollection uninformedYakuProps = new YakuCollection(cardProps, NewCards, activeCollection, activeHandSize);

            Result.GlobalMinimum = uninformedYakuProps[0];
            foreach (YakuProperties yakuProp in uninformedYakuProps)
            {
                float value = (activeHandSize - yakuProp.MinTurns) * yakuProp.Probability;
                if (value > (activeHandSize - Result.GlobalMinimum.MinTurns) * Result.GlobalMinimum.Probability)
                    Result.GlobalMinimum = yakuProp;
            }
            float TotalCardValue = 0f;
            try
            {
                TotalCardValue = uninformedYakuProps
                    .Where(x => x.Targeted)
                    .Sum(x => (8 - x.MinTurns) * x.Probability);
            }
            catch (Exception) { }
            Result.LocalMinimum = TotalCardValue;

            /*
             * Berechnung der Yaku-Qualität
             */

            foreach (var pair in State.UnknownCards)
            {
                float isDeckProb = 1f - pair.Value;
                float MoveValue = 0f;

                Result.DeckValue = MoveValue * isDeckProb;
            }

            return Result;
        }
    }
}