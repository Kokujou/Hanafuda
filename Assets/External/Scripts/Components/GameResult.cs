using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Hanafuda
{
    public class GameResult : MonoBehaviour
    {
        public Text P1Name;
        public Text P2Name;
        public Text WinnerName;
        public RectTransform P1PointGrid;
        public RectTransform P2PointGrid;
        public RectTransform[] YakuColumns;
        public GameObject YakuPrefab;

        public Color WinnerColor;
        public Color LooserColor;

        private int _Column;
        private int Column { get => _Column; set { if (value >= YakuColumns.Length) _Column = 0; else _Column = value; } }

        // Start is called before the first frame update
        void Start()
        {
            Text[] P1Points = P1PointGrid.GetComponentsInChildren<Text>();
            Text[] P2Points = P2PointGrid.GetComponentsInChildren<Text>();

            P1Name.text = Settings.Players[0].Name;
            P2Name.text = Settings.Players[1].Name;

            Player winner;
            if (Settings.Players[0].tempPoints > Settings.Players[1].tempPoints)
            {
                winner = Settings.Players[0];
                P1Name.color = WinnerColor;
                P2Name.color = LooserColor;
                P1Points[Settings.Rounds].color = WinnerColor;
                P2Points[Settings.Rounds].color = LooserColor;
            }
            else
            {
                winner = Settings.Players[1];
                P2Name.color = WinnerColor;
                P1Name.color = LooserColor;
                P2Points[Settings.Rounds].color = WinnerColor;
                P1Points[Settings.Rounds].color = LooserColor;
            }
            WinnerName.text = $"Sieger - {winner.Name}";
            WinnerName.color = WinnerColor;
            Settings.Players[0].pTotalPoints.Add(Settings.Players[0].tempPoints);
            Settings.Players[1].pTotalPoints.Add(Settings.Players[1].tempPoints);

            SetupYakus(winner);

            for (int i = 0; i < 6; i++)
            {
                if (i >= Settings.Players[0].pTotalPoints.Count) P1Points[i].text = "";
                else P1Points[i].text = Settings.Players[0].pTotalPoints[i].ToString();
                if (i >= Settings.Players[1].pTotalPoints.Count) P2Points[i].text = "";
                else P2Points[i].text = Settings.Players[1].pTotalPoints[i].ToString();
            }

            Settings.Rounds++;
        }

        private void SetupYakus(Player winner)
        {
            Column = 0;
            foreach (KeyValuePair<Yaku, int> yaku in winner.CollectedYaku)
            {
                GameObject obj = Instantiate(YakuPrefab, YakuColumns[Column]);
                RawImage card = obj.GetComponentInChildren<RawImage>();
                obj.GetComponentInChildren<Text>().text = yaku.Key.Title + $" - {yaku.Value}P";

                List<Card> yakuCards = new List<Card>();
                if (yaku.Key.Mask[1] == 1)
                    yakuCards.AddRange(winner.CollectedCards.FindAll(x => yaku.Key.Namen.Contains(x.Title)));
                if (yaku.Key.Mask[0] == 1)
                    yakuCards.AddRange(winner.CollectedCards.FindAll(x => !yaku.Key.Namen.Contains(x.Title) && x.Typ == yaku.Key.TypePref));
                if (yakuCards.Count < yaku.Key.minSize) Debug.Log("Invalid Player Collection");

                RawImage secondRowCard = null;
                if (yaku.Key.minSize > 5)
                {
                    ((RectTransform)obj.transform).sizeDelta += Vector2.up * (60f / 1.6f);
                    GameObject secondRow = Instantiate(card.transform.parent.gameObject, card.transform.parent.parent);
                    secondRowCard = secondRow.GetComponentInChildren<RawImage>();
                }
                for (int i = 0; i < yaku.Key.minSize || i % 5 != 0; i++)
                {
                    RawImage currentCard;
                    if (i == 0) currentCard = card;
                    else if (i < 5) currentCard = Instantiate(card.gameObject, card.transform.parent).GetComponent<RawImage>();
                    else if (i == 5) currentCard = secondRowCard;
                    else currentCard = Instantiate(secondRowCard.gameObject, secondRowCard.transform.parent).GetComponent<RawImage>();
                    if (i < yaku.Key.minSize)
                        currentCard.texture = yakuCards[i].Image.mainTexture;
                    else
                        currentCard.color = new Color(0, 0, 0, 0);
                }

                Column++;
            }
        }

        public void Continue()
        {
            if (Settings.Rounds < (Settings.Rounds6 ? 6 : 12))
            {
                SceneManager.LoadScene("Main");
            }
        }
    }
}