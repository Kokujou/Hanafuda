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


        private void Start()
        {
            BuildFromCards(new List<Card>());
        }
        public void BuildFromCards(List<Card> cards, List<KeyValuePair<Yaku, int>> yakus = null)
        {
            int Column = 0;
            if (yakus == null) yakus = Global.allYaku.ToDictionary(x => x, x => 0).ToList();
            foreach (KeyValuePair<Yaku, int> yaku in yakus)
            {
                GameObject obj = Instantiate(YakuPrefab, YakuColumns[Column]);
                RawImage card = obj.GetComponentInChildren<RawImage>();
                obj.GetComponentInChildren<Text>().text = yaku.Key.Title + $" - {yaku.Value}P";

                List<Card> cYakuCards = new List<Card>();
                List<Card> YakuCards = new List<Card>();
                List<Card> nYakucards = new List<Card>();

                if (yaku.Key.Mask[1] == 1)
                {
                    cYakuCards.AddRange(cards.FindAll(x => yaku.Key.Namen.Contains(x.Title)));
                    YakuCards.AddRange(Global.allCards.FindAll(x => yaku.Key.Namen.Contains(x.Title)));
                }
                if (yaku.Key.Mask[0] == 1)
                {
                    cYakuCards.AddRange(cards.FindAll(x => !yaku.Key.Namen.Contains(x.Title) && x.Typ == yaku.Key.TypePref));
                    YakuCards.AddRange(Global.allCards.FindAll(x => !yaku.Key.Namen.Contains(x.Title) && x.Typ == yaku.Key.TypePref));
                }
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