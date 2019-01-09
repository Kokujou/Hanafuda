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
            int ID = Turn ? Settings.PlayerID : 1 - Settings.PlayerID;
            int oPoints = ((Player)(players[ID])).tempPoints;
            List<KeyValuePair<Yaku, int>> oYaku = ((Player)(players[ID])).CollectedYaku;
            ((Player)(players[ID])).CollectedYaku = new List<KeyValuePair<Yaku, int>>(Yaku.GetYaku(((Player)(players[ID])).CollectedCards).ToDictionary(x => x, x => 0));
            NewYaku.Clear();
            if (((Player)(players[ID])).tempPoints > oPoints)
            {
                for (int i = 0; i < ((Player)(players[ID])).CollectedYaku.Count; i++)
                {
                    if (!oYaku.Exists(x => x.Key.Title == ((Player)(players[ID])).CollectedYaku[i].Key.Title && x.Value == ((Player)(players[ID])).CollectedYaku[i].Value))
                    {
                        NewYaku.Add(((Player)(players[ID])).CollectedYaku[i].Key);
                    }
                }
                Instantiate(Global.prefabCollection.YakuManager).GetComponent<YakuManager>().Init(NewYaku, this);
            }
            else
                OpponentTurn();
        }
    }
}