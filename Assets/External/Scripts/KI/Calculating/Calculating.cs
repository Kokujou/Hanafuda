using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Hanafuda
{
    public partial class CalculatingAI : KI<CalculatingAI.UninformedBoard>
    {
        const string _LocalWeight = "_LocalWeight";
        const string _GlobalWeight = "_GlobalWeight";
        const string _DeckMoveWeight = "_DeckMoveWeight";
        const string _OpponentUncertaintyWeight = "_OpponentUncertaintyWeight";

        public CalculatingAI(string name) : base(name)
        {
            Tree = new UninformedStateTree();
        }

        protected override void BuildStateTree(Spielfeld cRoot)
        {
            cRoot.Turn = true;
            Tree = new UninformedStateTree(new UninformedBoard(cRoot));
            Tree.Build(1);
        }

        private Dictionary<string, float> weights = new Dictionary<string, float>()
        {
            { _LocalWeight, 1f },
            { _GlobalWeight, 1f },
            { _DeckMoveWeight, 1f },
            { _OpponentUncertaintyWeight, 1f }
        };

        public override Dictionary<string, float> GetWeights() => weights;

        public override void SetWeight(string name, float value)
        {
            float temp;
            if (weights.TryGetValue(name, out temp))
                weights[name] = value;
        }

        public override Move MakeTurn(Spielfeld cRoot)
        {
            cRoot.Turn = true;
            Tree = new UninformedStateTree(new UninformedBoard(cRoot));
            Tree.Build();
            //Bewertung möglicherweise in Threads?
            var maxValue = -100f;
            Move selectedMove = null;
            List<List<UninformedBoard>> stateTree = new List<List<UninformedBoard>>();
            for (var i = 0; i < stateTree[1].Count; i++)
            {
                stateTree[1][i].Value = RateState(stateTree[1][i]);
                if (stateTree[1][i].Value > maxValue)
                {
                    maxValue = stateTree[1][i].Value;
                    selectedMove = stateTree[1][i].LastMove;
                }
            }
            return selectedMove;
        }
        public override float RateState(UninformedBoard State)
        {
            lock (UninformedStateTree.thisLock)
                Global.Log($"Zustand {State.GetHashCode()}: {PlayerAction.FromMove(State.LastMove, MainSceneVariables.boardTransforms.Main).ToString().Replace("\n", "")}");
            if (State.isFinal) return Mathf.Infinity;

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
            if (PStateProps.Count > 0)
            {
                PLocalMinimum = PStateProps.Max(x => x.LocalMinimum);
                PGlobalMinimum = PStateProps.Max(x => x.GlobalMinimum.Probability);
            }

            Result = (((State.computer.Hand.Count - ComStateProps.GlobalMinimum.MinTurns) * ComStateProps.GlobalMinimum.Probability) - PGlobalMinimum) * weights[_GlobalWeight]
                + (ComStateProps.LocalMinimum - PLocalMinimum) * weights[_LocalWeight];

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

            List<Card> NewCards = new List<Card>();
            if (State.LastMove != null)
            {
                UninformedBoard state = Tree.GetState(State.parentCoords.x, State.parentCoords.y);
                NewCards = activeCollection.Except(Turn ? state.computer.CollectedCards : state.OpponentCollection).ToList();
            }

            if (Turn) Result.SelectionProbability = 1;
            else
            {
                var handSelection = State.UnknownCards.First(x => x.Key.Title == State.LastMove.HandSelection);
                Result.SelectionProbability = handSelection.Value;
                State.UnknownCards.Remove(handSelection.Key);
            }

            /*
             * Berechnung der Yaku-Qualität
             */

            foreach (var pair in State.UnknownCards)
            {
                float isDeckProb = 1f - pair.Value;
                float MoveValue;



                Result.DeckValue = MoveValue * isDeckProb;
            }

            return Result;
        }
    }
}