using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Hanafuda
{
    public class GameResultPc : GameResult
    {
        public RectTransform P1PointGrid;
        public RectTransform P2PointGrid;

        public Text[] P1Points;
        public Text[] P2Points;

        private int _Column;
        private int Column { get => _Column; set { if (value >= YakuColumns.Length) _Column = 0; else _Column = value; } }

        protected override void Start()
        {
            if (!Settings.Rounds6)
                ClonePointsGrids();

            P1Points = P1PointGrid.GetComponentsInChildren<Text>();
            P2Points = P2PointGrid.GetComponentsInChildren<Text>();

            base.Start();
        }

        protected override void SetupTexts()
        {
            P1Name.text = Settings.Players[0].Name;
            P2Name.text = Settings.Players[1].Name;

            for (var i = 0; i < (Settings.Rounds6 ? 6 : 12); i++)
            {
                P1Points[i].text = $"{Settings.Players[0].pTotalPoints.ElementAtOrDefault(i)}";
                P2Points[i].text = $"{Settings.Players[1].pTotalPoints.ElementAtOrDefault(i)}";
            }

            if (Settings.Rounds + 1 >= (Settings.Rounds6 ? 6 : 12))
                ContinueText.text = "Spiel Beenden";
            else
                ContinueText.text = "Weiter";

            if (WinnerId < 0)
                WinnerName.text = "Unentschieden";
            else
                WinnerName.text = $"Sieger - {Settings.Players[WinnerId].Name}";
        }

        protected override void MarkGameResult()
        {
            if (WinnerId == 0)
            {
                var winnerTexts = P1Points
                    .Append(P1Name).Append(WinnerName)
                    .ToArray();
                ColorTexts(WinnerColor, winnerTexts);

                var looserTexts = P2Points.Append(P2Name)
                    .ToArray();
                ColorTexts(LooserColor, looserTexts);
            }
            else if (WinnerId == 1)
            {
                var winnerTexts = P2Points.Append(P2Name).ToArray();
                ColorTexts(WinnerColor, winnerTexts);

                var looserTexts = P1Points.Append(P1Name).Append(WinnerName)
                    .ToArray();
                ColorTexts(LooserColor, looserTexts);
            }
            else
            {
                var drawnTexts = P1Points.Union(P2Points)
                    .Append(P1Name).Append(P2Name).Append(WinnerName)
                    .ToArray();
                ColorTexts(DrawnColor, drawnTexts);
            }
        }

        protected override void UpdatePoints()
        {
            if (P1InitialWin > 0)
                Settings.Players[0].tempPoints += 6;
            else if (P2InitialWin > 0)
                Settings.Players[1].tempPoints += 6;

            Settings.Players[0].pTotalPoints.Add(Settings.Players[0].tempPoints);
            Settings.Players[1].pTotalPoints.Add(Settings.Players[1].tempPoints);
            Settings.Players[0].TotalPoints += Settings.Players[0].tempPoints;
            Settings.Players[1].TotalPoints += Settings.Players[1].tempPoints;
        }

        protected override void SetupYakus()
        {
            var winner = Settings.Players[WinnerId];
            var initialWin = P1InitialWin + P2InitialWin;
            Column = 0;
            foreach (var collectedYaku in (initialWin > 0 ? new Dictionary<int, int>() { { -1, initialWin == 1 ? 4 : 8 } } :
                winner.CollectedYaku.Where(x => x.Value >= Global.allYaku[x.Key].minSize)))
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

                List<Card> yakuCards;
                if (initialWin > 0)
                    yakuCards = winner.Hand;
                else
                    yakuCards = winner.CollectedCards.Where(x => yaku.Contains(x)).ToList();

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

        private void ClonePointsGrids()
        {
            for (var i = 0; i < 6; i++)
            {
                var child = P1PointGrid.GetChild(0);

                var p1Clone = Instantiate(child.gameObject);
                p1Clone.transform.SetParent(P1PointGrid);

                var p2Clone = Instantiate(child.gameObject);
                p2Clone.transform.SetParent(P2PointGrid);
            }
        }
    }
}