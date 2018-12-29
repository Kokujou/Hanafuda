using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;
using UnityEngine.SceneManagement;

namespace Hanafuda
{
    public partial class Communication
    {
        public const int MaxPlayer = 2;
        /// <summary>
        /// Erstellt ein Match auf den Namen des Hosts
        /// </summary>
        /// <param name="player">Name des Hosts</param>
        public void CreateMatch(string player)
        {
            NetworkManager.singleton.StartMatchMaker();
            NetworkManager.singleton.autoCreatePlayer = false;
            NetworkManager.singleton.matchMaker.CreateMatch(player.ToLower(), 2, true, "", "", "", 0, 0, WaitForPlayers);
            Settings.Multiplayer = true;
            Settings.PlayerID = 0;
            Settings.Players = new List<Player>() { new Player(player) };
            Settings.Rounds6 = Global.GridLayout.Toggles[0][0];
        }
        private void WaitForPlayers(bool success, string extendedInfo, MatchInfo matchInfo)
        {
            if (success)
            {
                var hostInfo = matchInfo;
                NetworkServer.Listen(hostInfo, 9000);
                NetworkServer.RegisterHandler(131, AddPlayer);
            }
            else
            {
                Debug.LogError("Create match failed");
            }
        }
        private void AddPlayer(NetworkMessage msg)
        {
            string name = msg.ReadMessage<Message>().message;
            NetworkServer.UnregisterHandler(131);
            NetworkServer.UnregisterHandler(MsgType.Connect);
            Settings.Players.Add(new Player(name));
            if (Settings.Players.Count == MaxPlayer)
            {
                NetworkServer.SendToAll(132, new Message { message = string.Join("|", Settings.Players.Cast<string>()) });
                SceneManager.LoadScene("OyaNegotiation");
            }
        }

        /// <summary>
        /// Matchsuche nach Namen
        /// </summary>
        /// <param name="partner">Partnername (=Matchname)</param>
        public void SearchMatch(string partner)
        {
            Settings.Players = new List<Player>() { new Player(partner) };
            NetworkManager.singleton.matchMaker.ListMatches(0, 20, partner.ToLower(), false, 0, 0, JoinFirstMatch);
        }
        private void JoinFirstMatch(bool success, string extendedInfo, List<MatchInfoSnapshot> matches)
        {
            if (success)
            {
                Debug.Log(matches.Count);
                if (matches.Count != 0)
                {
                    NetworkManager.singleton.matchName = matches[0].name;
                    NetworkManager.singleton.matchSize = (uint)matches[0].currentSize;
                    NetworkManager.singleton.matchMaker.JoinMatch(matches[0].networkId, "", "", "", 0, 0, WaitForConnection);
                }
                else
                {
                    Debug.Log("No matches in requested room!");
                }
            }
            else
                Debug.LogError("Couldn't connect to match maker");
        }
        private void WaitForConnection(bool success, string extendedInfo, MatchInfo matchInfo)
        {
            if (success)
            {
                NetworkClient client = new NetworkClient();
                client.Connect(matchInfo);
                client.RegisterHandler(MsgType.Connect, WaitForStart);
                Settings.Multiplayer = true;
                Settings.Rounds6 = Global.GridLayout.Toggles[0][0];
                Settings.Client = client;
            }
            else
            {
                Debug.LogError("Join/Create match failed");
            }
        }
        private void WaitForStart(NetworkMessage msg)
        {
            Settings.Client.Send(131, new Message { message = Settings.Players[0].Name });
            Settings.Client.UnregisterHandler(131);
            Settings.Client.UnregisterHandler(MsgType.Connect);
            Settings.Client.RegisterHandler(132, SyncAndStart);
        }
        private void SyncAndStart(NetworkMessage msg)
        {
            string[] names = msg.ReadMessage<Message>().message.Split('|');
            Settings.PlayerID = names.ToList().IndexOf(Settings.Players[0].Name);
            Settings.Players.Clear();
            for (int name = 0; name < names.Length; name++)
                Settings.Players.Add(new Player(names[name]));
            SceneManager.LoadScene("OyaNegotiation");
        }
    }
}