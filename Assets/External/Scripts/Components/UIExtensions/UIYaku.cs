
using Hanafuda.Base;
using Hanafuda.Base.Interfaces;
using Hanafuda.Extensions;
using System.Collections;
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

        public void AddCards(List<ICard> cards)
        {
            for (int i = 0; i < YakuTransforms.Count; i++)
            {
                RawImage[] images = YakuTransforms[i].Key.GetComponentsInChildren<RawImage>();
                foreach (ICard card in cards)
                {
                    if (YakuTransforms[i].Value.Contains(card))
                    {
                        List<RawImage> existing = images.ToList().FindAll(x => x.texture == card.GetImage().mainTexture);
                        if (existing.Count > 0)
                        {
                            int firstMissing = images.ToList().FindIndex(x => x.color.r < .9f);
                            if (firstMissing == -1) break;
                            else
                            {
                                existing[0].color = images[firstMissing].color;
                                images[firstMissing].color = Color.white;
                                existing[0].texture = images[firstMissing].texture;
                                images[firstMissing].texture = card.GetImage().mainTexture;
                            }
                        }
                        else
                        {
                            foreach (RawImage image in images)
                            {
                                if (image.color.r < .9f)
                                {
                                    image.texture = card.GetImage().mainTexture;
                                    image.color = Color.white;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }
        public void BuildFromCards(List<ICard> cards, Dictionary<int, int> yakus, float GridSpacingX = 10, float GridSpacingY = 10)
        {
            if (Settings.Mobile)
                BuildFromCardsMobile(cards, yakus, GridSpacingX, GridSpacingY);
            else
                BuildFromCardsPC(cards, yakus, GridSpacingX, GridSpacingY);
        }

        public void BuildFromCardsPC(List<ICard> cards, Dictionary<int, int> yakus, float GridSpacingX = 10, float GridSpacingY = 10)
        {
            int Column = 0;
            int CardsPerRow = 16;
            int inColumn = 0;
            yakus = yakus.OrderBy(x => Global.allYaku[x.Key].minSize).ToDictionary(x => x.Key, x => x.Value);
            foreach (var collectedYaku in yakus)
            {
                Yaku yaku = Global.allYaku[collectedYaku.Key];
                inColumn += yaku.minSize;
                if (inColumn > CardsPerRow)
                {
                    Column++;
                    inColumn = 0;
                }
                if (Column >= YakuColumns.Length) break;
                GameObject obj = Instantiate(YakuPrefab, YakuColumns[Column]);
                YakuTransforms.Add(new KeyValuePair<Transform, Yaku>(obj.transform, yaku));
                RawImage card = obj.GetComponentInChildren<RawImage>();
                obj.GetComponentInChildren<Text>().text = yaku.Title + (yaku.GetPoints(collectedYaku.Value) > 0 ? $" - {yaku.GetPoints(collectedYaku.Value)}P" : "");

                obj.GetComponentInChildren<GridLayoutGroup>().spacing = new Vector2(GridSpacingX, GridSpacingY);

                List<ICard> cYakuCards = new List<ICard>();
                List<ICard> YakuCards = new List<ICard>();
                List<ICard> nYakucards = new List<ICard>();

                cYakuCards.AddRange(cards.FindAll(x => yaku.Contains(x)));
                YakuCards.AddRange(Global.allCards.FindAll(x => yaku.Contains(x)));
                nYakucards = YakuCards.Where(x => !cYakuCards.Contains(x)).ToList();

                for (int i = 0; i < yaku.minSize; i++)
                {
                    RawImage currentCard;
                    if (i == 0) currentCard = card;
                    else currentCard = Instantiate(card.gameObject, card.transform.parent).GetComponent<RawImage>();
                    if (i < cYakuCards.Count)
                        currentCard.texture = cYakuCards[i].GetImage().mainTexture;
                    else
                    {
                        currentCard.texture = nYakucards[i - cYakuCards.Count].GetImage().mainTexture;
                        currentCard.color = new Color(.5f, .5f, .5f, 1);
                    }
                }
            }
        }

        public void BuildFromCardsMobile(List<ICard> cards, Dictionary<int, int> yakus, float GridSpacingX = 10, float GridSpacingY = 10)
        {
            int Column = 0;
            if (!yakus.Keys.Contains(-1))
                yakus = yakus.OrderBy(x => Global.allYaku[x.Key].minSize).ToDictionary(x => x.Key, x => x.Value);
            foreach (var collectedYaku in yakus)
            {
                Yaku yaku = null;
                if (collectedYaku.Key >= 0)
                    yaku = Global.allYaku[collectedYaku.Key];
                else if (collectedYaku.Value == 4)
                    yaku = new Yaku() { Title = "Teshi", JName = "手四", basePoints = 6, minSize = 4, Mask = null };
                else if (collectedYaku.Value == 8)
                    yaku = new Yaku() { Title = "Kuttsuki", JName = "くっつき", basePoints = 6, minSize = 8, Mask = null };

                GameObject obj = Instantiate(YakuPrefab, YakuColumns[Column]);
                YakuTransforms.Add(new KeyValuePair<Transform, Yaku>(obj.transform, yaku));
                RawImage card = obj.GetComponentInChildren<RawImage>();
                obj.GetComponentInChildren<Text>().text = yaku.Title + (yaku.GetPoints(collectedYaku.Value) > 0 ? $" - {yaku.GetPoints(collectedYaku.Value)}P" : "");

                obj.GetComponentInChildren<GridLayoutGroup>().spacing = new Vector2(GridSpacingX, GridSpacingY);

                List<ICard> cYakuCards = new List<ICard>();
                List<ICard> YakuCards = new List<ICard>();
                List<ICard> nYakucards = new List<ICard>();

                if (collectedYaku.Key < 0)
                    cYakuCards = cards;
                else
                {
                    cYakuCards.AddRange(cards.FindAll(x => yaku.Contains(x)));
                    YakuCards.AddRange(Global.allCards.FindAll(x => yaku.Contains(x)));
                    nYakucards = YakuCards.Where(x => !cYakuCards.Contains(x)).ToList();
                }

                for (int i = 0; i < yaku.minSize; i++)
                {
                    RawImage currentCard;
                    if (i == 0) currentCard = card;
                    else currentCard = Instantiate(card.gameObject, card.transform.parent).GetComponent<RawImage>();
                    if (i < cYakuCards.Count)
                        currentCard.texture = cYakuCards[i].GetImage().mainTexture;
                    else
                    {
                        currentCard.texture = nYakucards[i - cYakuCards.Count].GetImage().mainTexture;
                        currentCard.color = new Color(.5f, .5f, .5f, 1);
                    }
                }
                if (Column + 1 >= YakuColumns.Length) Column = 0;
                else Column++;
            }
        }
    }
}