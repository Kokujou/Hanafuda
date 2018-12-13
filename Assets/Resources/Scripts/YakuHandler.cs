using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

namespace Hanafuda
{
    /*
     * Todo:
     *  - Überlegung: Beim Lichter-Yaku verdunkeltes Ziel statt Hintergrund anzeigen
     */
    public class YakuHandler : MonoBehaviour
    {
        enum LightYaku
        {
            Sankou,
            Shikou,
            Ameshikou,
            Gokou
        }
        public RectTransform SlideIn, Cards, Main;
        public Text Title_Text, Title_Shadow, Subtitle_Text, Subtitle_Shadow;
        public List<Image> Lights;
        public List<Sprite> Captions;
        public Image Caption;
        public float cWidth;
        /*public void AskKoikoi()
        {
            initKoikoi = false;
            Destroy(GameObject.FindGameObjectWithTag("oldYaku"));
            GameObject Koikoi = Instantiate(Global.prefabCollection.Koikoi);
            Koikoi.name = "Koikoi";
            float cWidth = FindObjectOfType<Canvas>().GetComponent<RectTransform>().rect.width;
            GameObject.Find("Koikoi/SlideIn").GetComponent<RectTransform>().sizeDelta = new Vector2(cWidth / 0.2f, 500);
            EventTrigger.Entry entry = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerDown
            };
            entry.callback.AddListener((data) =>
            {
                initKoikoi = false;
                ((Player)Board.players[Board.Turn ? 0 : 1]).Koikoi++;
                StartCoroutine(Global.prefabCollection.KoikoiText.KoikoiAnimation(() =>
                {
                    allowInput = true;
                    Board.Turn = !Board.Turn;
                    PlayMode = 1;
                }));
                Destroy(Koikoi);
            });
            GameObject.Find("Koikoi/YesButton").GetComponent<EventTrigger>().triggers.Add(entry);
            entry = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerDown
            };
            entry.callback.AddListener((data) =>
            {
                initKoikoi = false;
                Global.players = Board.players.Cast<Player>().ToList();
                SceneManager.LoadScene("Finish");
            });
            GameObject.Find("Koikoi/NoButton").GetComponent<EventTrigger>().triggers.Add(entry);
        }*/

        public void AddYaku(Yaku Yaku)
        {
            GameObject yaku = Instantiate(Global.prefabCollection.gAddYaku);
            yaku.name = Yaku.Name;
            SetupText(Yaku);
        }

        private void SetupText(Yaku Yaku)
        {
            if (Yaku.Name == "Ino Shika Chou")
            {
                Title_Shadow.font = Global.prefabCollection.EdoFont;
                Title_Text.font = Global.prefabCollection.EdoFont;
            }
            SlideIn.sizeDelta = new Vector2(cWidth / 0.2f, 500);
            Title_Shadow.text = Yaku.JName;
            Title_Text.text = Yaku.JName;
            Subtitle_Shadow.text = Yaku.Name;
            Subtitle_Text.text = Yaku.Name;
        }

        public void FixedYaku(Yaku Yaku, List<Card> Collection)
        {
            GameObject yaku = Instantiate(Global.prefabCollection.gFixedYaku);
            yaku.name = Yaku.Name;
            SetupText(Yaku);
            List<Card> temp = new List<Card>();
            if (Yaku.Mask[1] == 1)
                temp.AddRange(Collection.FindAll(x => Yaku.Namen.Contains(x.Name)));
            if (Yaku.Mask[0] == 1)
                temp.AddRange(Collection.FindAll(x => x.Typ == Yaku.TypPref));
            Transform parent = Cards.transform;
            for (int yakuCard = 0; yakuCard < Yaku.minSize; yakuCard++)
            {
                GameObject card = new GameObject(yakuCard.ToString());
                RectTransform rect = card.AddComponent<RectTransform>();
                rect.SetParent(parent, false);
                rect.localPosition = new Vector3((Yaku.minSize / 2f) * -35 + 35 * yakuCard + 17.5f, 0);
                rect.sizeDelta = new Vector2(25, 40);
                GameObject shadow = new GameObject("Shadow");
                shadow.transform.SetParent(card.transform, false);
                shadow.AddComponent<Image>().color = new Color(0, 0, 0);
                rect = shadow.GetComponent<RectTransform>();
                rect.localPosition = new Vector3(2, -2);
                rect.sizeDelta = new Vector2(25, 40);
                GameObject Image = new GameObject("Image");
                Image.transform.SetParent(card.transform, false);
                Image img = Image.AddComponent<Image>();
                Texture2D tex = (Texture2D)temp[yakuCard].Image.mainTexture;
                img.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(.5f, .5f));
                rect = img.GetComponent<RectTransform>();
                rect.localPosition = new Vector3(0, 0);
                rect.sizeDelta = new Vector2(25, 40);
            }
        }

        public void KouYaku(Yaku Yaku, List<Card> Collection)
        {
            GameObject Kou = Instantiate(Global.prefabCollection.gKouYaku);
            Kou.name = Yaku.Name;
            Caption.sprite = Captions[(int)Enum.Parse(typeof(LightYaku), Yaku.name)];
            List<Card> Matches = Global.allCards.FindAll(y => y.Typ == Card.Typen.Lichter);
            for (int cardID = 0; cardID < 5; cardID++)
            {
                if (Collection.Exists(x => x.Name == Matches[cardID].Name))
                {
                    Texture2D tex = (Texture2D)Matches[cardID].Image.mainTexture;
                    Lights[cardID].sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(.5f, .5f));
                }
                else
                    Lights[cardID].sprite = Global.CardSkins[Global.Settings.CardSkin];
            }
        }
    }
}