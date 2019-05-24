using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Hanafuda
{
    public partial class SearchingAI : KI<SearchingBoard>
    {
        private Dictionary<string, float> weights = new Dictionary<string, float>()
        {
        };

        public override Dictionary<string, float> GetWeights() => weights;

        public override void SetWeight(string name, float value)
        {
            float temp;
            if (weights.TryGetValue(name, out temp))
                weights[name] = value;
        }

        public SearchingAI(string name) : base(name)
        {
        }

        public override float RateState(SearchingBoard state)
        {
            throw new NotImplementedException();
        }

        protected override void BuildStateTree(Spielfeld cRoot)
        {
            System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
            watch.Start();
            Global.Log("Searching AI Tree Building started");
            cRoot.Turn = true;
            Tree = new SearchingStateTree(new SearchingBoard(cRoot));
            Tree.Build(skipOpponent: true);
            Global.Log($"State Tree Building Time: {watch.ElapsedMilliseconds}");
            Global.Log($"State Tree Last Count: {Tree.GetLevel(Tree.Size - 1).Count}");
            watch.Restart();
            List<Yaku> yakuTree = new List<Yaku>();
            foreach (SearchingBoard board in Tree.GetLevel(Tree.Size - 1))
            {
                List<Yaku> newYakus = Yaku.GetNewYakus(Enumerable.Range(0, 13).ToDictionary(x => x, x => 0), board.computerCollection);
                if (newYakus.Count > 0)
                    yakuTree.AddRange(newYakus);
            }
            Global.Log($"Synced time for gettings yakus: {watch.ElapsedMilliseconds}");
            Global.Log($"Different Yakus: {string.Join(";", yakuTree.Select(x => x.Title).Distinct())}");
            Global.Log($"Max Collection Size: {string.Join(";", Tree.GetLevel(Tree.Size - 1).First(x => x.computerCollection.Count > 14).computerCollection)}");
        }
    }
}