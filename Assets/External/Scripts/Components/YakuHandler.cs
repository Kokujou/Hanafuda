using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using Hanafuda.Base;
using Hanafuda.Base.Interfaces;
using Hanafuda.Extensions;

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

        public void FixedYaku(Yaku Yaku, List<ICard> Collection)
        {
            const int CardWidth = 25;
            const int CardOffset = 10;
            int CardSpace = CardWidth + CardOffset;
            SetupText(Yaku);
            List<ICard> yakuCards = Collection.Where(x=>Yaku.Contains(x)).ToList();
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
                Texture2D tex = (Texture2D)yakuCards[yakuCard].GetImage().mainTexture;
                img.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(.5f, .5f));
                rect = img.GetComponent<RectTransform>();
                rect.localPosition = new Vector3(0, 0);
                rect.sizeDelta = new Vector2(25, 40);
            }
        }

        public void KouYaku(Yaku Yaku, List<ICard> Collection)
        {
            if(Settings.Mobile)
                transform.localScale = (transform.parent.GetComponent<CanvasScaler>().referenceResolution.x / Screen.width) * Vector3.one;
            transform.localScale = (1f / transform.parent.GetComponent<Canvas>().scaleFactor) * Vector3.one;
            Caption.sprite = Captions[(int)Enum.Parse(typeof(LightYaku), Yaku.Title)];
            List<ICard> Matches = Global.allCards.FindAll(y => y.Motive == CardMotive.Lichter).Cast<ICard>().ToList();
            for (int cardID = 0; cardID < 5; cardID++)
            {
                if (Collection.Exists(x => x.Title == Matches[cardID].Title))
                {
                    Texture2D tex = (Texture2D)Matches[cardID].GetImage().mainTexture;
                    Lights[cardID].sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(.5f, .5f));
                }
                else
                    Lights[cardID].sprite = Global.CardSkins[Settings.CardSkin];
            }
        }
    }
}