﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Hanafuda
{
    public class UIYaku : MonoBehaviour
    {
        public GameObject YakuPrefab;
        public Transform[] YakuColumns;

        private List<KeyValuePair<Transform, Yaku>> YakuTransforms = new List<KeyValuePair<Transform, Yaku>>();

        public void AddCards(List<Card> cards)
        {
            for (int i = 0; i < YakuTransforms.Count; i++)
            {
                RawImage[] images = YakuTransforms[i].Key.GetComponentsInChildren<RawImage>();
                foreach (Card card in cards)
                {
                    if (YakuTransforms[i].Value.Contains(card))
                    {
                        foreach (RawImage image in images)
                        {
                            if (image.color.r < .9f)
                            {
                                image.texture = card.Image.mainTexture;
                                image.color = Color.white;
                                break;
                            }
                        }
                    }
                }
            }
        }
        public void BuildFromCards(List<Card> cards, List<KeyValuePair<Yaku, int>> yakus = null, float GridSpacingX = 10, float GridSpacingY = 10)
        {
            if (Settings.Mobile)
                BuildFromCardsMobile(cards, yakus, GridSpacingX, GridSpacingY);
            else
                BuildFromCardsPC(cards, yakus, GridSpacingX, GridSpacingY);
        }

        public void BuildFromCardsPC(List<Card> cards, List<KeyValuePair<Yaku, int>> yakus = null, float GridSpacingX = 10, float GridSpacingY = 10)
        {
            int Column = 0;
            int CardsPerRow = 16;
            int inColumn = 0;
            if (yakus == null) yakus = Global.allYaku.ToDictionary(x => x, x => 0).ToList();
            yakus = yakus.OrderBy(x => x.Key.minSize).ToList();
            foreach (KeyValuePair<Yaku, int> yaku in yakus)
            {
                inColumn += yaku.Key.minSize;
                if (inColumn > CardsPerRow)
                {
                    Column++;
                    inColumn = 0;
                }
                if (Column >= YakuColumns.Length) break;
                GameObject obj = Instantiate(YakuPrefab, YakuColumns[Column]);
                YakuTransforms.Add(new KeyValuePair<Transform, Yaku>(obj.transform, yaku.Key));
                RawImage card = obj.GetComponentInChildren<RawImage>();
                obj.GetComponentInChildren<Text>().text = yaku.Key.Title + (yaku.Value > 0 ? $" - {yaku.Value}P" : "");

                obj.GetComponentInChildren<GridLayoutGroup>().spacing = new Vector2(GridSpacingX, GridSpacingY);

                List<Card> cYakuCards = new List<Card>();
                List<Card> YakuCards = new List<Card>();
                List<Card> nYakucards = new List<Card>();

                cYakuCards.AddRange(cards.FindAll(x => yaku.Key.Contains(x)));
                YakuCards.AddRange(Global.allCards.FindAll(x => yaku.Key.Contains(x)));
                nYakucards = YakuCards.Where(x => !cYakuCards.Contains(x)).ToList();

                for (int i = 0; i < yaku.Key.minSize; i++)
                {
                    RawImage currentCard;
                    if (i == 0) currentCard = card;
                    else currentCard = Instantiate(card.gameObject, card.transform.parent).GetComponent<RawImage>();
                    if (i < cYakuCards.Count)
                        currentCard.texture = cYakuCards[i].Image.mainTexture;
                    else
                    {
                        currentCard.texture = nYakucards[i - cYakuCards.Count].Image.mainTexture;
                        currentCard.color = new Color(.5f, .5f, .5f, 1);
                    }
                }
            }
        }

        public void BuildFromCardsMobile(List<Card> cards, List<KeyValuePair<Yaku, int>> yakus = null, float GridSpacingX = 10, float GridSpacingY = 10)
        {
            int Column = 0;
            if (yakus == null) yakus = Global.allYaku.ToDictionary(x => x, x => 0).ToList();
            yakus = yakus.OrderBy(x=>x.Key.minSize).ToList();
            foreach (KeyValuePair<Yaku, int> yaku in yakus)
            {
                GameObject obj = Instantiate(YakuPrefab, YakuColumns[Column]);
                YakuTransforms.Add(new KeyValuePair<Transform, Yaku>(obj.transform, yaku.Key));
                RawImage card = obj.GetComponentInChildren<RawImage>();
                obj.GetComponentInChildren<Text>().text = yaku.Key.Title + $" - {yaku.Value}P";

                obj.GetComponentInChildren<GridLayoutGroup>().spacing = new Vector2(GridSpacingX, GridSpacingY);

                List<Card> cYakuCards = new List<Card>();
                List<Card> YakuCards = new List<Card>();
                List<Card> nYakucards = new List<Card>();

                cYakuCards.AddRange(cards.FindAll(x => yaku.Key.Contains(x)));
                YakuCards.AddRange(Global.allCards.FindAll(x => yaku.Key.Contains(x)));
                nYakucards = YakuCards.Where(x => !cYakuCards.Contains(x)).ToList();

                for (int i = 0; i < yaku.Key.minSize; i++)
                {
                    RawImage currentCard;
                    if (i == 0) currentCard = card;
                    else currentCard = Instantiate(card.gameObject, card.transform.parent).GetComponent<RawImage>();
                    if (i < cYakuCards.Count)
                        currentCard.texture = cYakuCards[i].Image.mainTexture;
                    else
                    {
                        currentCard.texture = nYakucards[i - cYakuCards.Count].Image.mainTexture;
                        currentCard.color = new Color(.5f, .5f, .5f, 1);
                    }
                }
                if (Column + 1 >= YakuColumns.Length) Column = 0;
                else Column++;
            }
        }
    }
}