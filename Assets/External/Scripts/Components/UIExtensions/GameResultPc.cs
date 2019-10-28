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
            P1Name.text = Settings.Players[Settings.PlayerID].Name;
            P2Name.text = Settings.Players[1 - Settings.PlayerID].Name;

            for (var i = 0; i < (Settings.Rounds6 ? 6 : 12); i++)
            {
                P1Points[i].text = $"{Settings.Players[Settings.PlayerID].pTotalPoints.ElementAtOrDefault(i)}";
                P2Points[i].text = $"{Settings.Players[1 - Settings.PlayerID].pTotalPoints.ElementAtOrDefault(i)}";
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
            if (WinnerId == Settings.PlayerID)
            {
                var winnerTexts = P1Points
                    .Append(P1Name).Append(WinnerName)
                    .ToArray();
                ColorTexts(WinnerColor, winnerTexts);

                var looserTexts = P2Points.Append(P2Name)
                    .ToArray();
                ColorTexts(LooserColor, looserTexts);
            }
            else if (WinnerId == (1 - Settings.PlayerID))
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
            YakuInfo.BuildFromCards(initialWin > 0 ? Settings.Players[WinnerId].Hand : winner.CollectedCards,
                    (initialWin > 0 ? new Dictionary<int, int>() { { -1, initialWin == 1 ? 4 : 8 } } :
                    winner.CollectedYaku.Where(x => Global.allYaku[x.Key].minSize <= x.Value).ToDictionary(x => x.Key, x => x.Value)));
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