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

        public override void BuildStateTree(VirtualBoard cRoot)
        {
            throw new NotImplementedException();
        }

        public override Dictionary<string, float> GetWeights()
        {
            throw new NotImplementedException();
        }

        public override Move MakeTurn(VirtualBoard cRoot)
        {
            cRoot.Turn = true;
            if (Tree == null)
            {
                Tree = new OmniscientStateTree(cRoot);
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

        public override void SetWeight(string name, float value)
        {
            throw new NotImplementedException();
        }
    }
}