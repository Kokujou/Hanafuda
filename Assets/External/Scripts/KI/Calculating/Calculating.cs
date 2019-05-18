﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Hanafuda
{
    public class CalculatingAI : KI<UninformedBoard>
    {
        public CalculatingAI(string name) : base(name)
        {
            Tree = new UninformedStateTree();
        }

        protected override void BuildStateTree(Spielfeld cRoot)
        {
            throw new NotImplementedException();
        }

        public override Dictionary<string, float> GetWeights()
        {
            throw new NotImplementedException();
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
            float result = 0;
            return result;
        }

        public override void SetWeight(string name, float value)
        {
            throw new NotImplementedException();
        }
    }
}