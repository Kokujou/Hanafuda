using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Hanafuda
{
    public abstract class GameResult : MonoBehaviour
    {
        public Text P1Name;
        public Text P2Name;
        public Text WinnerName;
        public Text ContinueText;
        public UIYaku YakuInfo;

        public Color WinnerColor;
        public Color LooserColor;
        public Color DrawnColor;

        protected int WinnerId;
        protected int P1InitialWin;
        protected int P2InitialWin;

        protected virtual void Start()
        {
            WinnerId = GetWinnerId();

            UpdatePoints();
            SetupTexts();
            MarkGameResult();

            if (WinnerId >= 0)
                SetupYakus();

            Settings.Rounds++;
        }

        public static void ColorTexts(Color targetColor, params Text[] texts)
        {
            foreach (var text in texts)
                text.color = targetColor;
        }

        private int GetWinnerId()
        {
            P1InitialWin = Settings.Players[Settings.PlayerID].Hand.IsInitialWin();
            P2InitialWin = Settings.Players[1 - Settings.PlayerID].Hand.IsInitialWin();
            if (P1InitialWin > 0)
                return Settings.PlayerID;
            if (P2InitialWin > 0)
                return (1 - Settings.PlayerID);
            if (Settings.Players[Settings.PlayerID].tempPoints > Settings.Players[1 - Settings.PlayerID].tempPoints)
                return Settings.PlayerID;
            if (Settings.Players[1 - Settings.PlayerID].tempPoints > Settings.Players[Settings.PlayerID].tempPoints)
                return (1 - Settings.PlayerID);
            else return -1;
        }

        public void Continue()
        {
            if (Settings.Rounds < (Settings.Rounds6 ? 6 : 12))
            {
                if (WinnerId > 0)
                {
                    var winner = Settings.Players[WinnerId];
                    Settings.Players.RemoveAt(WinnerId);
                    Settings.Players.Insert(0, winner);
                    Settings.PlayerID = 1 - Settings.PlayerID;
                }
                SceneManager.LoadScene("Main");
            }
            else
                SceneManager.LoadScene("Startup");
        }

        protected abstract void SetupTexts();
        protected abstract void MarkGameResult();
        protected abstract void UpdatePoints();
        protected abstract void SetupYakus();
    }
}