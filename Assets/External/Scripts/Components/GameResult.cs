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
        public UIYaku YakuInfo;

        public Color WinnerColor;
        public Color LooserColor;
        public Color DrawnColor;

        private Player winner;
        private int initialWin;

        private int _Column;
        private int Column { get => _Column; set { if (value >= YakuColumns.Length) _Column = 0; else _Column = value; } }

        // Start is called before the first frame update
        void Start()
        {
            Text[] P1Points = P1PointGrid.GetComponentsInChildren<Text>();
            Text[] P2Points = P2PointGrid.GetComponentsInChildren<Text>();

            P1Name.text = Settings.Players[0].Name;
            P2Name.text = Settings.Players[1].Name;

            foreach (Player player in Settings.Players)
                player.tempPoints = player.CollectedYaku.Sum(x => Global.allYaku[x.Key].GetPoints(x.Value));

            if (Settings.Players[0].tempPoints > Settings.Players[1].tempPoints || (initialWin = Settings.Players[0].Hand.IsInitialWin()) > 0)
            {

                winner = Settings.Players[0];
                if (initialWin > 0) winner.tempPoints += 6;
                P1Name.color = WinnerColor;
                P2Name.color = LooserColor;
                P1Points[Settings.Rounds].color = WinnerColor;
                P2Points[Settings.Rounds].color = LooserColor;
            }
            else if (Settings.Players[1].tempPoints > Settings.Players[0].tempPoints || (initialWin = Settings.Players[1].Hand.IsInitialWin()) > 0)
            {
                winner = Settings.Players[1];
                if (initialWin > 0) winner.tempPoints += 6;
                P2Name.color = WinnerColor;
                P1Name.color = LooserColor;
                P2Points[Settings.Rounds].color = WinnerColor;
                P1Points[Settings.Rounds].color = LooserColor;
            }
            else
            {
                P1Name.color = DrawnColor;
                P2Name.color = DrawnColor;
                P2Points[Settings.Rounds].color = DrawnColor;
                P1Points[Settings.Rounds].color = DrawnColor;
                WinnerName.text = "Unentschieden";
                WinnerName.color = DrawnColor;
                Settings.Players[0].pTotalPoints.Add(0);
                Settings.Players[1].pTotalPoints.Add(0);
                for (int i = 0; i < 6; i++)
                {
                    if (i >= Settings.Players[0].pTotalPoints.Count) P1Points[i].text = "";
                    else P1Points[i].text = Settings.Players[0].pTotalPoints[i].ToString();
                    if (i >= Settings.Players[1].pTotalPoints.Count) P2Points[i].text = "";
                    else P2Points[i].text = Settings.Players[1].pTotalPoints[i].ToString();
                }
                Settings.Rounds++;
                return;
            }
            WinnerName.text = $"Sieger - {winner.Name}";
            WinnerName.color = WinnerColor;

            Settings.Players[0].pTotalPoints.Add(Settings.Players[0].tempPoints);
            Settings.Players[1].pTotalPoints.Add(Settings.Players[1].tempPoints);

            if (Settings.Mobile)
                YakuInfo.BuildFromCards(initialWin > 0 ? winner.Hand : winner.CollectedCards, 
                    (initialWin > 0 ? new Dictionary<int, int>() { { -1, initialWin == 1 ? 4 : 8 } } : winner.CollectedYaku), -45);
            else
                SetupYakus(winner, initialWin);

            for (int i = 0; i < 6; i++)
            {
                if (i >= Settings.Players[0].pTotalPoints.Count) P1Points[i].text = "";
                else P1Points[i].text = Settings.Players[0].pTotalPoints[i].ToString();
                if (i >= Settings.Players[1].pTotalPoints.Count) P2Points[i].text = "";
                else P2Points[i].text = Settings.Players[1].pTotalPoints[i].ToString();
            }

            Settings.Rounds++;
        }

        private void SetupYakus(Player winner, int initialWin)
        {
            Column = 0;
            foreach (var collectedYaku in (initialWin > 0 ? new Dictionary<int, int>() { { -1, initialWin == 1 ? 4 : 8 } } : winner.CollectedYaku))
            {
                Yaku yaku = null;
                if (initialWin == 0)
                    yaku = Global.allYaku[collectedYaku.Key];
                else if (initialWin == 1)
                    yaku = new Yaku() { Title = "Teshi", JName = "手四", basePoints = 6, minSize = 4, Mask = null };
                else if (initialWin == 2)
                    yaku = new Yaku() { Title = "Kuttsuki", JName = "くっつき", basePoints = 6, minSize = 8, Mask = null };
                GameObject obj = Instantiate(YakuPrefab, YakuColumns[Column]);
                RawImage card = obj.GetComponentInChildren<RawImage>();
                obj.GetComponentInChildren<Text>().text = yaku.Title + $" - {yaku.GetPoints(collectedYaku.Value)}P";

                List<Card> yakuCards = new List<Card>();
                if (initialWin > 0)
                    yakuCards = winner.Hand;
                else
                {
                    if (yaku.Mask[1] == 1)
                        yakuCards.AddRange(winner.CollectedCards.FindAll(x => yaku.Namen.Contains(x.Title)));
                    if (yaku.Mask[0] == 1)
                        yakuCards.AddRange(winner.CollectedCards.FindAll(x => !yaku.Namen.Contains(x.Title) && x.Typ == yaku.TypePref));
                    if (yaku.Mask[1] == -1)
                        yakuCards.RemoveAll(x => yaku.Namen.Contains(x.Title));
                    if (yakuCards.Count < yaku.minSize) Debug.Log("Invalid Player Collection");
                }

                RawImage secondRowCard = null;
                if (yaku.minSize > 5)
                {
                    ((RectTransform)obj.transform).sizeDelta += Vector2.up * (60f / 1.6f);
                    GameObject secondRow = Instantiate(card.transform.parent.gameObject, card.transform.parent.parent);
                    secondRowCard = secondRow.GetComponentInChildren<RawImage>();
                }
                for (int i = 0; i < yaku.minSize || i % 5 != 0; i++)
                {
                    RawImage currentCard;
                    if (i == 0) currentCard = card;
                    else if (i < 5) currentCard = Instantiate(card.gameObject, card.transform.parent).GetComponent<RawImage>();
                    else if (i == 5) currentCard = secondRowCard;
                    else currentCard = Instantiate(secondRowCard.gameObject, secondRowCard.transform.parent).GetComponent<RawImage>();
                    if (i < yaku.minSize)
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
                int winnerIndex = Settings.Players.IndexOf(winner);
                if (winnerIndex != 0)
                {
                    Settings.Players.RemoveAt(winnerIndex);
                    Settings.Players.Insert(0, winner);
                }
                SceneManager.LoadScene("Main");
            }
        }
    }
}