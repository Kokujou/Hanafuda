using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Hanafuda
{
    public class CalculatingAI : KI
    {
        public CalculatingAI(string name) : base(name) { }
        public override Move MakeTurn(VirtualBoard cRoot)
        {
            cRoot.Turn = true;
            Tree = new StateTree(cRoot);
            Tree.Build();
            //Bewertung möglicherweise in Threads?
            var maxValue = -100f;
            Move selectedMove = null;
            List<List<VirtualBoard>> stateTree = new List<List<VirtualBoard>>();
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
        public override float RateState(VirtualBoard State)
        {
            float result = 0;
            return result;
        }

    }
}