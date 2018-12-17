using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;
using UnityEngine.SceneManagement;

/*
 * Todo:
 *  - Fehlerbehebung: Doppelte Spielernamen bei Eintragung
 *  - Zusätzlicher Filter beim Suchen: Rundenzahl
 *  - Offene Match-Liste zur Verfügung stellen
 *  - Ladeanimation beim Warten/Suchen/Erstellen
 */
/// <summary>
/// Hauptmenü beim Spielstart
/// </summary>
/// 
namespace Hanafuda
{
    public class NetworkScript : MonoBehaviour
    {
        private NetworkClient client;

        private readonly Global.GridLayout.SelectionGrid KIMode =
            new Global.GridLayout.SelectionGrid(1, 3, new[] {"Normal", "Schwer", "Alptraum"});

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
        public GUISkin skin;
        public ObservableCollection<Card> test = new ObservableCollection<Card>();

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
            PlayMode = new Global.GridLayout.SelectionGrid(1, 2, new[] {"Einzelspieler", "Mehrspieler"},
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
            SinglePlayer.addToLine(new Global.GridLayout.Button(2, delegate
            {
                Global.Settings.Rounds6 = Global.GridLayout.Toggles[0][0];
                Global.Settings.P1Name = P1.Text;
                Global.Settings.KIMode = KIMode.Selected;
                SceneManager.LoadScene("OyaNegotiation");
            }, "Spiel Starten"));
            SinglePlayer.addToLine(new Global.GridLayout.Empty(1));
            MultiPlayer.addLine(new Global.GridLayout.Empty(1), 1);
            MultiPlayer.addToLine(new Global.GridLayout.Button(4, () =>
            {
                NetworkManager.singleton.StartMatchMaker();
                NetworkManager.singleton.autoCreatePlayer = false;
                NetworkManager.singleton.matchMaker.CreateMatch(P1.Text, 2, true, "", "", "", 0, 0, OnMatchCreate);
                matchName = P1.Text;
                Running = true;
                Camera.main.name = P1.Text;
                Global.Settings.Multiplayer = true;
                Global.Settings.Name = P1.Text;
                Global.Settings.P1Name = P1.Text;
                Global.Settings.Rounds6 = Global.GridLayout.Toggles[0][0];
            }, "Auf Mitspieler warten"));
            MultiPlayer.addToLine(new Global.GridLayout.Empty(1));
            MultiPlayer.addLine(new Global.GridLayout.Label(1, "Name des Mitspielers", true), 1);
            MultiPlayer.addToLine(P2);
            MultiPlayer.addLine(new Global.GridLayout.Empty(1), 1);
            MultiPlayer.addToLine(new Global.GridLayout.Button(4,
                () => { NetworkManager.singleton.matchMaker.ListMatches(0, 20, "", false, 0, 0, OnMatchList_Join); },
                "Mitspieler suchen"));
            MultiPlayer.addToLine(new Global.GridLayout.Empty(1));
            PlayMode.Selected = 0;
            KIMode.Selected = 0;
        }

        private void OnConnected(NetworkMessage msg)
        {
            Debug.Log("Test");
            if (Global.Settings.P2Name != "")
            {
                client.Send(131, new Global.Message {message = Global.Settings.Name});
                client.UnregisterHandler(131);
                client.UnregisterHandler(MsgType.Connect);
                SceneManager.LoadScene("OyaNegotiation");
            }
        }

        private void AddPlayer(NetworkMessage msg)
        {
            Global.Settings.P2Name = msg.ReadMessage<Global.Message>().message;
            NetworkServer.UnregisterHandler(131);
            NetworkServer.UnregisterHandler(MsgType.Connect);
            SceneManager.LoadScene("OyaNegotiation");
        }

        private void OnMatchCreate(bool success, string extendedInfo, MatchInfo matchInfo)
        {
            if (success)
            {
                var hostInfo = matchInfo;
                NetworkServer.Listen(hostInfo, 9000);
                NetworkServer.RegisterHandler(MsgType.Connect, OnConnected);
                NetworkServer.RegisterHandler(131, AddPlayer);
            }
            else
            {
                Debug.LogError("Create match failed");
            }
        }

        private void OnMatchJoined(bool success, string extendedInfo, MatchInfo matchInfo)
        {
            if (success)
            {
                client = new NetworkClient();
                client.Connect(matchInfo);
                client.RegisterHandler(MsgType.Connect, OnConnected);
                Global.Settings.playerClients.Add(client);
                Global.Settings.Multiplayer = true;
                Global.Settings.Name = P1.Text;
                Global.Settings.P1Name = P2.Text;
                Global.Settings.P2Name = P1.Text;
                Global.Settings.Rounds6 = Global.GridLayout.Toggles[0][0];
            }
            else
            {
                Debug.LogError("Join/Create match failed");
            }
        }

        /// <summary>
        ///     Auflistung der aller aktuellen Räume
        /// </summary>
        /// <param name="success">Erfolg/Miserfolg</param>
        /// <param name="extendedInfo"></param>
        /// <param name="matches">Liste von passenden Einträgen</param>
        private void OnMatchList_Join(bool success, string extendedInfo, List<MatchInfoSnapshot> matches)
        {
            if (success)
                if (matches.Count != 0)
                {
                    MatchInfoSnapshot result = null;
                    foreach (var match in matches)
                        if (match.name == P2.Text)
                            result = match;
                    if (result != null)
                    {
                        NetworkManager.singleton.matchName = result.name;
                        NetworkManager.singleton.matchSize = (uint) result.currentSize;
                        NetworkManager.singleton.matchMaker.JoinMatch(result.networkId, "", "", "", 0, 0,
                            OnMatchJoined);
                        Running = true;
                    }
                    else
                    {
                        Debug.Log("No matches with requested name in requested room!");
                    }
                }
                else
                {
                    Debug.Log("No matches in requested room!");
                }
            else
                Debug.LogError("Couldn't connect to match maker");
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
        }
    }
}