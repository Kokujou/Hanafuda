using System.Collections.Generic;
using System.Linq;

namespace Hanafuda
{
    public class CardProperties
    {
        public int ID { get; }
        public Card card { get; }
        public Dictionary<int, float> RelevanceForYaku { get; set; }
        public CardProperties(int cardID)
        {
            ID = cardID;
            card = Global.allCards[cardID];
            MinTurns = -1;
            Probability = 0;
            RelevanceForYaku = new Dictionary<int, float>();
            for (int i = 0; i < Global.allYaku.Count; i++)
            {
                Yaku yaku = Global.allYaku[i];
                if (yaku.Contains(card))
                {
                    RelevanceForYaku.Add(i, 1f / yaku.minSize);
                }
            }
        }        

        public float TotalValue { get; private set; }
        public void CalcValue(Dictionary<int, float> YakuValues)
        {
            float result = 0;
            for (int i = 0; i < YakuValues.Count; i++)
            {
                float value = 0;
                RelevanceForYaku.TryGetValue(i, out value);
                value *= YakuValues[i];
                result += value;
            }
            TotalValue = result;
        }

        //To be Initialized by Collection
        public int MinTurns { get; set; }
        public float Probability { get; set; }
    }
}