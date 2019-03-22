using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Hanafuda
{
    public class SearchingAI : KI
    {
        public SearchingAI(string name) : base(name)
        {
        }
        public override Move MakeTurn(VirtualBoard cRoot)
        {
            cRoot.Turn = true;
            if (Tree == null)
            {
                Tree = new StateTree(cRoot);
                Tree.Build();
            }
            //Bewertung möglicherweise in Threads?
            Move selectedMove = null;
            return selectedMove;
        }
        public override float RateState(VirtualBoard State)
        {
            float result = 0;
            return result;
        }

    }
}