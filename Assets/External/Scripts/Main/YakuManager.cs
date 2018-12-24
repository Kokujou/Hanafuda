using ExtensionMethods;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace Hanafuda
{
    public class YakuManager : MonoBehaviour
    {
        private RectTransform _oldYaku;
        private RectTransform oldYaku
        {
            get { return _oldYaku; }
            set
            {
                if (_oldYaku != null)
                    Destroy(_oldYaku.parent.gameObject);
                _oldYaku = value;
            }
        }
        private RectTransform _Yaku;
        public RectTransform Yaku
        {
            get { return _Yaku; }
            set
            {
                if (_Yaku?.name == "Kou")
                    Destroy(_Yaku.gameObject);
                else if (_Yaku != null)
                    oldYaku = _Yaku;
                _Yaku = value;
                Queue.RemoveAt(0);
            }
        }
        private float animLeft = -200;
        private readonly System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
        private List<Yaku> Queue = null;
        private Spielfeld Board;
        public GameObject Main;
        public RectTransform SlideIn;
        public EventTrigger YesButton, NoButton;
        public bool Finished=false;
        public void Init(List<Yaku> queue, Spielfeld board)
        {
            Queue = queue;
            Board = board;
        }
        public void AlignYaku()
        {
            if (Queue?.Count > 0)
            {
                Debug.Log(Queue[0].TypPref);
                if (Queue[0].TypPref == Card.Type.Lichter) return;
            }
            Yaku.localPosition = new Vector3(animLeft, 0, 0);
            if (oldYaku)
                oldYaku.localPosition = new Vector3(animLeft + 200, 0, 0);
        }
        private void Update()
        {
            if (Queue == null) return;
            if (Global.MovingCards > 0) return;
            if (animLeft != 0)
            {
                //allowInput = false;
                if (animLeft < 0)
                    animLeft += 3;
                else if (animLeft != 0)
                    animLeft = 0;
            }
            AlignYaku();
            if (watch.ElapsedMilliseconds > 3000 || !watch.IsRunning)
            {
                if (Queue.Count == 0)
                {
                    AskKoikoi();
                    return;
                }
                YakuHandler handler;
                if (Queue[0].TypPref == Card.Type.Lichter)
                {
                    handler = Instantiate(Global.prefabCollection.gKouYaku, transform).GetComponent<YakuHandler>();
                    handler.KouYaku(Queue[0], ((Player)Board.players[0]).CollectedCards);
                    handler.name = "Kou";
                }
                else if (Queue[0].addPoints == 0)
                {
                    handler = Instantiate(Global.prefabCollection.gFixedYaku, transform).GetComponent<YakuHandler>();
                    handler.FixedYaku(Queue[0], ((Player)Board.players[0]).CollectedCards);
                }
                else
                {
                    handler = Instantiate(Global.prefabCollection.gAddYaku, transform).GetComponent<YakuHandler>();
                    handler.AddYaku(Queue[0]);
                }
                Yaku = handler.Main;
                watch.Reset();
                watch.Start();
                animLeft = -200;
            }
        }
        public void AskKoikoi()
        {
            Queue = null;
            Destroy(Yaku.parent.gameObject);
            Main.SetActive(true);
            SlideIn.sizeDelta = new Vector2(1000, 500);
            EventTrigger.Entry entry = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerDown
            };
            entry.callback.AddListener((data) =>
            {
                ((Player)Board.players[0]).Koikoi++;
                StartCoroutine(Global.prefabCollection.KoikoiText.KoikoiAnimation(() =>
                {
                    Board.Turn = !Board.Turn;
                    Finished = true;
                }));
                Main.SetActive(false);
            });
            YesButton.triggers.Add(entry);
            entry = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerDown
            };
            entry.callback.AddListener((data) =>
            {
                Global.players.RemoveAll(x => x.Name == ((Player)Board.players[0]).Name);
                Global.players.Add(((Player)Board.players[0]));
                SceneManager.LoadScene("Finish");
            });
            NoButton.triggers.Add(entry);
        }
    }
}