using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace Hanafuda
{
    public partial class MainScreen : MonoBehaviour
    {
        private string Name;

        private GameObject Loading;

        public List<GameObject> ModeContainer = new List<GameObject>();

        public void ChangeRounds6(bool value)
        {
            Settings.Rounds6 = value;
        }

        public void OnAIModeChanged(int value)
        {
            Settings.AiMode = (Settings.AIMode)value;
        }

        public void OnGameModeChanged(int mode)
        {
            for (int i = 0; i < ModeContainer.Count; i++)
            {
                if (i == mode)
                    ModeContainer[i].SetActive(true);
                else
                    ModeContainer[i].SetActive(false);
            }
            Settings.Multiplayer = mode == 1;
        }

        public void OnNameChanged(string name)
        {
            Name = name.Replace("|", "").ToLower();
        }

        public void SingleplayerStart()
        {
            Settings.Players = new List<Player>() { new Player(Name), new Player("Computer") };
            Settings.PlayerID = 0;
            Settings.Multiplayer = false;
            SceneManager.LoadScene("OyaNegotiation");
        }

        public void ConsultingStart()
        {
            Settings.Players = new List<Player>() { new Player(Name), new Player("Computer") };
            Settings.PlayerID = 0;
            Settings.Multiplayer = false;
            Settings.Consulting = true;
            SceneManager.LoadScene("Consulting");
        }

        public void CreateMatch()
        {
            Global.instance.gameObject.GetComponent<Communication>().CreateMatch(Name, Settings.Rounds6);
            Loading = Instantiate(Global.prefabCollection.Loading);
        }

        public void SearchMatch()
        {
            Global.instance.gameObject.GetComponent<Communication>().JoinLobby(Name, Settings.Rounds6);
        }

        private void Start()
        {
        }
    }
}