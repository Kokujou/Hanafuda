using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Hanafuda
{
    public partial class Spielfeld
    {
        List<Yaku> NewYaku;
        private void CheckNewYaku()
        {
            int oPoints = ((Player)(players[Turn ? 0 : 1])).tempPoints;
            List<KeyValuePair<Yaku, int>> oYaku = ((Player)(players[Turn ? 0 : 1])).CollectedYaku;
            ((Player)(players[Turn ? 0 : 1])).CollectedYaku = new List<KeyValuePair<Yaku, int>>(Yaku.GetYaku(((Player)(players[Turn ? 0 : 1])).CollectedCards).ToDictionary(x => x, x => 0));
            NewYaku.Clear();
            if (((Player)(players[Turn ? 0 : 1])).tempPoints > oPoints)
            {
                for (int i = 0; i < ((Player)(players[Turn ? 0 : 1])).CollectedYaku.Count; i++)
                {
                    if (!oYaku.Exists(x => x.Key.Title == ((Player)(players[Turn ? 0 : 1])).CollectedYaku[i].Key.Title && x.Value == ((Player)(players[Turn ? 0 : 1])).CollectedYaku[i].Value))
                    {
                        NewYaku.Add(((Player)(players[Turn ? 0 : 1])).CollectedYaku[i].Key);
                    }
                }
                Instantiate(Global.prefabCollection.YakuManager).GetComponent<YakuManager>().Init(NewYaku, this);
            }
        }
    }
}