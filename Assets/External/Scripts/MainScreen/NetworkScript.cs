using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;
using UnityEngine.SceneManagement;


namespace Hanafuda
{
    public class NetworkScript : MonoBehaviour
    {
        private NetworkClient client;
        private readonly Global.GridLayout.SelectionGrid KIMode =
            new Global.GridLayout.SelectionGrid(1, 3, new[] { "Normal", "Schwer", "Alptraum" });
        private Global.GridLayout layout;
        private string matchName = "";
        private readonly Global.GridLayout MultiPlayer = new Global.GridLayout();
        private readonly Global.GridLayout.TextField P1 = new Global.GridLayout.TextField(2, 20);
        private readonly Global.GridLayout.TextField P2 = new Global.GridLayout.TextField(2, 20);
        private Global.GridLayout.SelectionGrid PlayMode;
        private readonly Vector2 resolution = new Vector2(900, 750);
        private readonly Global.GridLayout.Toggle rounds6 = new Global.GridLayout.Toggle(2, 0, 0, "6 Runden");
        private bool Running;
        private readonly Global.GridLayout SinglePlayer = new Global.GridLayout();
        private GameObject Loading;
        public GUISkin skin;

        private void Update()
        {
            layout.Width = resolution.x * 0.9f;
            layout.Height = resolution.y * 0.9f;
            layout.Top = resolution.x * 0.05f;
            layout.Left = resolution.y * 0.05f;
        }

        private void Start()
        {
            layout = new Global.GridLayout(Screen.currentResolution.width * 0.5f,
                Screen.currentResolution.height * 0.7f, Screen.currentResolution.width / 2,
                Screen.currentResolution.height / 2, 10, 10);
            PlayerPrefs.SetInt("Start", 0);
            NetworkManager.singleton.StartMatchMaker();
            PlayMode = new Global.GridLayout.SelectionGrid(1, 2, new[] { "Einzelspieler", "Mehrspieler" },
                selectionChanged: selected =>
                {
                    layout.Grid.RemoveAll(x => x[0].Label == (selected - 1 == 0 ? 1 : 2));
                    layout.AddRange(selected == 0 ? SinglePlayer.Grid : MultiPlayer.Grid, selected + 1);
                });
            layout.addLine(new Global.GridLayout.Empty(1), 1);
            layout.addToLine(rounds6);
            layout.addToLine(new Global.GridLayout.Toggle(2, 0, 1, "12 Runden"));
            layout.addToLine(new Global.GridLayout.Empty(1));
            layout.addLine(new Global.GridLayout.Label(1, "Spieler-Name", true), 1);
            layout.addToLine(P1);
            layout.addLine(new Global.GridLayout.Label(1, "Spielmodus", true), 1);
            layout.addLine(PlayMode, 1);
            SinglePlayer.addLine(new Global.GridLayout.Label(1, "Künstliche Intelligenz", true), 1);
            SinglePlayer.addLine(KIMode, 1);
            SinglePlayer.addLine(new Global.GridLayout.Empty(1), 1);
            SinglePlayer.addToLine(new Global.GridLayout.Button(2, () =>
            {
                Settings.Rounds6 = Global.GridLayout.Toggles[0][0];
                Settings.KIMode = KIMode.Selected;
                Settings.Players = new List<Player>() { new Player(P1.Text), new Player("Computer") };
                Settings.PlayerID = 0;
                SceneManager.LoadScene("OyaNegotiation");
            }, "Spiel Starten"));
            SinglePlayer.addToLine(new Global.GridLayout.Empty(1));
            MultiPlayer.addLine(new Global.GridLayout.Empty(1), 1);
            MultiPlayer.addToLine(new Global.GridLayout.Button(4, () =>
            {
                Global.instance.gameObject.GetComponent<Communication>().CreateMatch(P1.Text, Global.GridLayout.Toggles[0][0]);
                Running = true;
            }, "Auf Mitspieler warten"));
            MultiPlayer.addToLine(new Global.GridLayout.Empty(1));
            MultiPlayer.addLine(new Global.GridLayout.Label(1, "Name des Mitspielers", true), 1);
            MultiPlayer.addToLine(P2);
            MultiPlayer.addLine(new Global.GridLayout.Empty(1), 1);
            MultiPlayer.addToLine(new Global.GridLayout.Button(4,
                () =>
                {
                    Global.instance.gameObject.GetComponent<Communication>().SearchMatch(P1.Text, P2.Text, Global.GridLayout.Toggles[0][0]);
                    Running = true;
                },
                "Mitspieler suchen"));
            MultiPlayer.addToLine(new Global.GridLayout.Empty(1));
            PlayMode.Selected = 0;
            KIMode.Selected = 0;
        }
        /// <summary>
        ///     Graphische Benutzeroberfläche für Spieleinstellungen
        /// </summary>
        private void OnGUI()
        {
            if (!Running)
            {
                GUI.skin = skin;
                // 500:400
                GUI.matrix = Matrix4x4.TRS(new Vector3(0, 0, 0), Quaternion.identity,
                    new Vector3(Screen.width / resolution.x, Screen.height / resolution.y, 1));
                layout.DrawLayout(true, "Einstellungen");
            }
            else if (Loading == null)
            {
                Loading = Instantiate(Global.prefabCollection.Loading);
            }
        }
    }
}