using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hanafuda
{
    public class YakuManager : MonoBehaviour
    {
        private RectTransform oldYaku
        {
            get { return oldYaku; }
            set
            {
                if (oldYaku != null)
                    Destroy(oldYaku.gameObject);
                oldYaku = value;
            }
        }
        public RectTransform Yaku
        {
            get { return Yaku; }
            set
            {
                if (Yaku != null)
                    oldYaku = Yaku;
                Yaku = value;
            }
        }
        private float animLeft = 0;
        private readonly System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
        private List<Yaku> Queue;
        private List<Card> Collection;
        public void Init(List<Yaku> queue, List<Card> collection)
        {
            Queue = queue;
            Collection = collection;
        }
        public void AlignYaku()
        {
            if (Queue[0].TypPref == Card.Typen.Lichter) return;
            Yaku.localPosition = new Vector3(animLeft, 0, 0);
            if (oldYaku)
            {
                oldYaku.localPosition = new Vector3(animLeft + Yaku.rect.width, 0, 0);
            }
        }
        private void Update()
        {
            if (animLeft != 0 && Queue.Count != 0)
            {
                //allowInput = false;
                if (animLeft < 0)
                    animLeft += 10;
                else if (animLeft != 0)
                    animLeft = 0;
            }
            if (animLeft == 0 && !watch.IsRunning)
                watch.Start();
            if (watch.ElapsedMilliseconds > 3000)
            {
                YakuHandler handler;
                if (Queue[0].TypPref == Card.Typen.Lichter)
                {
                    handler = Instantiate(Global.prefabCollection.gAddYaku).GetComponent<YakuHandler>();
                    handler.KouYaku(Queue[0], Collection);
                }
                else if (Queue[0].addPoints == 0)
                {
                    handler = Instantiate(Global.prefabCollection.gAddYaku).GetComponent<YakuHandler>();
                    handler.FixedYaku(Queue[0], Collection);
                }
                else
                {
                    handler = Instantiate(Global.prefabCollection.gAddYaku).GetComponent<YakuHandler>();
                    handler.AddYaku(Queue[0]);
                }
                Yaku = handler.Main;
                watch.Reset();
                watch.Start();
                animLeft = -Yaku.rect.width;
            }
            AlignYaku();
        }
    }
}