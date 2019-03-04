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
        public RectTransform Cards, Main;
        public Text Title_Text, Title_Shadow, Subtitle_Text, Subtitle_Shadow;
        public List<Image> Lights;
        public List<Sprite> Captions;
        public Image Caption;

        public void AddYaku(Yaku Yaku)
        {
            SetupText(Yaku);
        }

        private void SetupText(Yaku Yaku)
        {
            if (Yaku.Title == "Ino Shika Chou")
            {
                Title_Shadow.font = Global.prefabCollection.EdoFont;
                Title_Text.font = Global.prefabCollection.EdoFont;
            }
            Title_Shadow.text = Yaku.JName;
            Title_Text.text = Yaku.JName;
            Subtitle_Shadow.text = Yaku.Title;
            Subtitle_Text.text = Yaku.Title;
        }

        public void FixedYaku(Yaku Yaku, List<Card> Collection)
        {
            const int CardWidth = 25;
            const int CardOffset = 10;
            int CardSpace = CardWidth + CardOffset;
            SetupText(Yaku);
            List<Card> yakuCards = new List<Card>();
            if (Yaku.Mask[1] == 1)
                yakuCards.AddRange(Collection.FindAll(x => Yaku.Namen.Contains(x.Title)));
            if (Yaku.Mask[0] == 1)
                yakuCards.AddRange(Collection.FindAll(x => x.Typ == Yaku.TypePref));
            Transform parent = Cards.transform;
            for (int yakuCard = 0; yakuCard < Yaku.minSize; yakuCard++)
            {
                GameObject card = new GameObject(yakuCard.ToString());
                RectTransform rect = card.AddComponent<RectTransform>();
                rect.SetParent(parent, false);
                if (Yaku.minSize > 5)
                    CardSpace = CardWidth + 4;
                rect.localPosition = new Vector3((Yaku.minSize / 2f) * -CardSpace + CardSpace * yakuCard + CardSpace / 2f, 0);
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
                Texture2D tex = (Texture2D)yakuCards[yakuCard].Image.mainTexture;
                img.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(.5f, .5f));
                rect = img.GetComponent<RectTransform>();
                rect.localPosition = new Vector3(0, 0);
                rect.sizeDelta = new Vector2(25, 40);
            }
        }

        public void KouYaku(Yaku Yaku, List<Card> Collection)
        {
            transform.localScale = (1f / transform.parent.GetComponent<Canvas>().scaleFactor) * Vector3.one;
            Caption.sprite = Captions[(int)Enum.Parse(typeof(LightYaku), Yaku.Title)];
            List<Card> Matches = Global.allCards.FindAll(y => y.Typ == Card.Type.Lichter);
            for (int cardID = 0; cardID < 5; cardID++)
            {
                if (Collection.Exists(x => x.Title == Matches[cardID].Title))
                {
                    Texture2D tex = (Texture2D)Matches[cardID].Image.mainTexture;
                    Lights[cardID].sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(.5f, .5f));
                }
                else
                    Lights[cardID].sprite = Global.CardSkins[Settings.CardSkin];
            }
        }
    }
}