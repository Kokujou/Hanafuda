﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Hanafuda
{
    public class GameResultMobile : GameResult
    {
        public Text P1Points;
        public Text P2Points;

        protected override void MarkGameResult()
        {
            if (WinnerId == Settings.PlayerID)
            {
                ColorTexts(WinnerColor, P1Name, P1Points, WinnerName);
                ColorTexts(LooserColor, P2Name, P2Points);
            }
            else if (WinnerId == 1- Settings.PlayerID)
            {
                ColorTexts(WinnerColor, P2Name, P2Points);
                ColorTexts(LooserColor, P1Name, P1Points, WinnerName);
            }
            else
                ColorTexts(DrawnColor, P1Name, P2Name, P1Points, P2Points, WinnerName);
        }

        protected override void SetupTexts()
        {
            P1Name.text = Settings.Players[Settings.PlayerID].Name;
            P2Name.text = Settings.Players[1 - Settings.PlayerID].Name;
            P1Points.text = Settings.Players[Settings.PlayerID].TotalPoints.ToString();
            P2Points.text = Settings.Players[1 - Settings.PlayerID].TotalPoints.ToString();

            if (Settings.Rounds + 1 >= (Settings.Rounds6 ? 6 : 12))
                ContinueText.text = "Spiel Beenden";
            else
                ContinueText.text = "Weiter";

            if (WinnerId < 0)
                WinnerName.text = "Unentschieden";
            else
                WinnerName.text = $"Sieger - {Settings.Players[WinnerId].Name}";
        }

        protected override void SetupYakus()
        {
            var winner = Settings.Players[WinnerId];
            var initialWin = P1InitialWin + P2InitialWin;
            YakuInfo.BuildFromCards(initialWin > 0 ? Settings.Players[WinnerId].Hand : winner.CollectedCards,
                    (initialWin > 0 ? new Dictionary<int, int>() { { -1, initialWin == 1 ? 4 : 8 } } :
                    winner.CollectedYaku.Where(x => Global.allYaku[x.Key].minSize <= x.Value).ToDictionary(x => x.Key, x => x.Value)));
        }

        protected override void UpdatePoints()
        {
            if (P1InitialWin > 0)
                Settings.Players[Settings.PlayerID].tempPoints += 6;
            else if (P2InitialWin > 0)
                Settings.Players[1 - Settings.PlayerID].tempPoints += 6;

            Settings.Players[Settings.PlayerID].pTotalPoints.Add(Settings.Players[Settings.PlayerID].tempPoints);
            Settings.Players[1 - Settings.PlayerID].pTotalPoints.Add(Settings.Players[1 - Settings.PlayerID].tempPoints);
            Settings.Players[Settings.PlayerID].TotalPoints += Settings.Players[Settings.PlayerID].tempPoints;
            Settings.Players[1 - Settings.PlayerID].TotalPoints += Settings.Players[1 - Settings.PlayerID].tempPoints;
        }
    }
}