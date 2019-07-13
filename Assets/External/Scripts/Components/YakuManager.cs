using Hanafuda.Base;
using Hanafuda.Extensions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
                if (_Yaku?.parent.name == "Kou")
                {
                    Destroy(_Yaku.parent.gameObject);
                    SlideIn.gameObject.SetActive(true);
                }
                else if (_Yaku != null)
                    oldYaku = _Yaku;
                _Yaku = value;
                Queue.RemoveAt(0);
            }
        }

        private float totalWidth;
        private float animLeft = -200;
        private readonly System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
        private List<Yaku> Queue = null;
        private Spielfeld Board;
        public GameObject Main, SkipButton;
        public RectTransform SlideIn;
        public EventTrigger YesButton, NoButton;
        public bool Finished = false;
        private GameObject _CherryBlossoms;
        public void Init(List<Yaku> queue, Spielfeld board)
        {
            Queue = queue;
            Board = board;
            board.gameObject.SetActive(false);
            _CherryBlossoms = Instantiate(Global.prefabCollection.CherryBlossoms);
            totalWidth = GetComponent<RectTransform>().sizeDelta.x / GetComponent<Canvas>().scaleFactor;
            if (Settings.Mobile)
            {
                GetComponent<CanvasScaler>().matchWidthOrHeight = 0;
                totalWidth = GetComponent<CanvasScaler>().referenceResolution.x;
            }
            animLeft = -totalWidth;
            SlideIn.sizeDelta = new Vector2(totalWidth * 2, 100);
            SlideIn.anchoredPosition = new Vector3(-totalWidth * 2, 0, 0);
            SlideIn.GetComponent<Image>().material.mainTextureScale = new Vector2(totalWidth / 50f, 1);
            SlideIn.gameObject.SetActive(Queue[0].TypePref != CardMotive.Lichter);
            
        }
        public void AlignYaku()
        {
            if (!Yaku || Queue == null || Yaku.parent.name == "Kou")
                return;
            Yaku.localPosition = new Vector3(animLeft, 0, 0);
            if (oldYaku)
            {
                oldYaku.localPosition = new Vector3(animLeft + totalWidth, 0, 0);
                SlideIn.GetComponent<RectTransform>().anchoredPosition = new Vector3(animLeft, 0, 0);
            }
            else
                SlideIn.GetComponent<RectTransform>().anchoredPosition = new Vector3(animLeft - totalWidth, 0, 0);

        }
        private void Update()
        {
            if (Queue == null) return;
            if (animLeft != 0)
            {
                //allowInput = false;
                if (animLeft < 0)
                    animLeft += totalWidth / 50f;
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
                if (Queue[0].TypePref == CardMotive.Lichter)
                {
                    handler = Instantiate(Global.prefabCollection.gKouYaku, transform).GetComponent<YakuHandler>();
                    handler.KouYaku(Queue[0], ((Player)Board.Players[Settings.PlayerID]).CollectedCards);
                    handler.name = "Kou";
                }
                else if (Queue[0].addPoints == 0)
                {
                    handler = Instantiate(Global.prefabCollection.gFixedYaku, transform).GetComponent<YakuHandler>();
                    handler.FixedYaku(Queue[0], ((Player)Board.Players[Settings.PlayerID]).CollectedCards);
                }
                else
                {
                    handler = Instantiate(Global.prefabCollection.gAddYaku, transform).GetComponent<YakuHandler>();
                    handler.AddYaku(Queue[0]);
                }
                Yaku = handler.Main;
                watch.Reset();
                watch.Start();
                animLeft = -totalWidth;
            }
        }
        public void Skip()
        {
            Destroy(oldYaku?.gameObject);
            _oldYaku = null;
            AskKoikoi();
        }
        public void AskKoikoi()
        {
            Queue = null;
            Destroy(Yaku.parent.gameObject);
            Destroy(SkipButton);
            SlideIn.gameObject.SetActive(true);
            SlideIn.anchoredPosition = Vector3.zero;
            Main.SetActive(true);
            EventTrigger.Entry entry = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerDown
            };
            entry.callback.AddListener((data) =>
            {
                Board.Players[Settings.PlayerID].Koikoi++;
                Destroy(SlideIn.gameObject);
                StartCoroutine(Global.prefabCollection.KoikoiText.KoikoiAnimation(() =>
                {
                    Destroy(_CherryBlossoms);
                    Destroy(gameObject);
                    Board.gameObject.SetActive(true);
                    Board.SayKoiKoi(true);
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
                Destroy(_CherryBlossoms);
                Destroy(gameObject);
                Board.gameObject.SetActive(true);
                Board.SayKoiKoi(false);
            });
            NoButton.triggers.Add(entry);
        }
    }
}