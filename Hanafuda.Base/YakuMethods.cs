using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hanafuda.Base
{
    public static class YakuMethods
    {
        public static List<Yaku> GetNewYakus(Dictionary<int, int> currentYakus, List<Card> newCards, bool AllowWrite = false)
        {
            List<Yaku> NewYaku = new List<Yaku>();
            for (int yakuID = 0; yakuID < Global.allYaku.Count; yakuID++)
            {
                Yaku yaku = Global.allYaku[yakuID];
                int matchingCount = 0;
                foreach (Card card in newCards)
                {
                    if (!yaku.Contains(card)) continue;
                    int oldCount = currentYakus[yakuID];
                    matchingCount++;
                    if (AllowWrite)
                        currentYakus[yakuID]++;
                    if (yaku.GetPoints(oldCount + matchingCount) > yaku.GetPoints(oldCount))
                        NewYaku.Add(yaku);
                }
            }
            return NewYaku;
        }

        public static void DistinctYakus(List<KeyValuePair<Yaku, int>> list)
        {
            for (var i = 5; i > 2; i--)
                if (list.Exists(x => x.Key.Title.Contains((i == 5 ? "Go" : i == 4 ? "Shi" : "San") + "kou")))
                    list.RemoveAll(x =>
                        x.Key.Title.Contains("kou") &&
                        !x.Key.Title.Contains((i == 5 ? "Go" : i == 4 ? "Shi" : "San") + "kou"));
                else if (i == 4 && list.Exists(x => x.Key.Title.Contains("Ameshikou")))
                    list.RemoveAll(x => x.Key.Title.Contains("kou") && !x.Key.Title.Contains("Ameshikou"));
            if (list.Exists(x => x.Key.Title == "Aka Ao Kasane"))
                list.RemoveAll(x => x.Key.Title.Contains("tan") && x.Key.Title != "Aka Ao Kasane");
        }

        public static void DistinctYakus(List<Yaku> list)
        {
            for (var i = 5; i > 2; i--)
                if (list.Exists(x => x.Title.Contains((i == 5 ? "Go" : i == 4 ? "Shi" : "San") + "kou")))
                    list.RemoveAll(x =>
                        x.Title.Contains("kou") && !x.Title.Contains((i == 5 ? "Go" : i == 4 ? "Shi" : "San") + "kou"));
                else if (i == 4 && list.Exists(x => x.Title.Contains("Ameshikou")))
                    list.RemoveAll(x => x.Title.Contains("kou") && !x.Title.Contains("Ameshikou"));
            if (list.Exists(x => x.Title == "Aka Ao Kasane"))
                list.RemoveAll(x => x.Title.Contains("tan") && x.Title != "Aka Ao Kasane");
        }

        public static List<Yaku> GetYaku(List<Card> Hand)
        {
            var temp = new List<Yaku>();
            for (var i = 0; i < Global.allYaku.Count; i++)
                if (Hand == Global.allYaku[i])
                    temp.Add(Global.allYaku[i]);
            return temp;
        }

        public static List<int> GetYakuIDs(List<Card> Hand)
        {
            var temp = new List<int>();
            for (var i = 0; i < Global.allYaku.Count; i++)
                if (Hand == Global.allYaku[i])
                    temp.Add(i);
            return temp;
        }
    }
}
