using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using System.Net.Sockets;
using System.Net;
using System.Text;
using ExtensionMethods;

namespace Hanafuda
{
    public partial class Communication
    {
        private const int MaxPlayer = 2;
        private const short PingMsg = 130;
        private const short MoveSyncMsg = 131;
        private const short DeckSyncMsg = 132;
        private const short AddPlayerMsg = 133;
        private const short PlayerSyncMsg = 134;

        private List<Player> connected;

        public void RegisterHandlers()
        {
            Settings.Client.RegisterHandler(DeckSyncMsg, ReceiveSeed);
            Settings.Client.RegisterHandler(MoveSyncMsg, ReceiveMove);
            Settings.Client.RegisterHandler(MsgType.Disconnect, OnDisconnect);
            Settings.Client.RegisterHandler(PingMsg, x => { });
            if (TCPServer.Active)
            {
                Settings.Server.RegisterHandler(MoveSyncMsg, BroadcastMove);
                Settings.Server.RegisterHandler(PingMsg, x => { });
            }
        }

        /// <summary>
        /// Erstellt ein Match auf den Namen des Hosts
        /// </summary>
        /// <param name="player">Name des Hosts</param>
        public void CreateMatch(string player, bool rounds6)
        {
            Settings.Multiplayer = true;
            Settings.PlayerID = 0;
            Settings.Players = new List<Player>() { new Player(player) };
            Settings.Rounds6 = rounds6;
            Settings.Server = new TCPServer(2);
            Settings.Server.RegisterHandler(AddPlayerMsg, AddPlayer);
            connected = new List<Player>();
            Settings.Client = new TCPClient("127.0.0.1", 9000);
            NetworkManager.singleton.StartMatchMaker();
            NetworkManager.singleton.autoCreatePlayer = false;
            NetworkManager.singleton.matchMaker.CreateMatch(Settings.GetMatchName(), 2, true, "", new WebClient().DownloadString("http://icanhazip.com").Trim(), "", 0, 0, WaitForPlayers);
        }
        private void WaitForPlayers(bool success, string extendedInfo, MatchInfo matchInfo)
        {
            if (success)
            {
                WaitForStart();
            }
            else
            {
                Debug.LogError("Create match failed");
            }
        }
        private async void AddPlayer(byte[] msg)
        {
            string name = msg.Deserialize<string>();
            connected.Add(new Player(name));
            Debug.Log($"Player Added:{name}");
            if (connected.Count == MaxPlayer)
            {
                Debug.Log("Alle Spieler verbunden");
                Settings.Server.UnregisterHandler(AddPlayerMsg);
                await Settings.Server.SendToAll(PlayerSyncMsg, new PlayerList { players = connected.Select(x => x.Name).ToArray() });
            }
            else
                Debug.Log(connected.Count);
        }

        /// <summary>
        /// Matchsuche nach Namen
        /// </summary>
        /// <param name="partner">Partnername (=Matchname)</param>
        public void SearchMatch(string player, string partner, bool rounds6)
        {
            TcpListener listener = new TcpListener(IPAddress.Any, 9000);
            Settings.Players = new List<Player>() { new Player(partner), new Player(player) };
            Settings.PlayerID = 1;
            Settings.Rounds6 = rounds6;
            Settings.Multiplayer = true;
            NetworkManager.singleton.matchMaker.ListMatches(0, 20, Settings.GetMatchName(), false, 0, 0, JoinFirstMatch);
        }
        private void JoinFirstMatch(bool success, string extendedInfo, List<MatchInfoSnapshot> matches)
        {
            if (success)
            {
                if (matches.Count != 0)
                {
                    NetworkManager.singleton.matchName = matches[0].name;
                    NetworkManager.singleton.matchSize = (uint)matches[0].currentSize;
                    NetworkManager.singleton.matchMaker.JoinMatch(matches[0].networkId, "", "", "", 0, 0, WaitForConnection);
                    Debug.Log(matches[0].directConnectInfos[0].publicAddress);
                    TCPClient client = new TCPClient(matches[0].directConnectInfos[0].publicAddress, 9000);
                    Settings.Client = client;
                    Debug.Log("Client Created");
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
                WaitForStart();
            }
            else
            {
                Debug.LogError("Join/Create match failed");
            }
        }
        private async void WaitForStart()
        {
            Settings.Client.RegisterHandler(PlayerSyncMsg, SyncAndStart);
            await Settings.Client.Send(AddPlayerMsg, Settings.GetName());
            Debug.Log("Send Add Player");
        }
        private void SyncAndStart(byte[] msg)
        {
            Debug.Log("SyncAndStart");
            Settings.Client.UnregisterHandler(PlayerSyncMsg);
            string[] names = msg.Deserialize<string[]>();
            Settings.PlayerID = names.ToList().IndexOf(Settings.GetName());
            Settings.Players.Clear();
            for (int name = 0; name < names.Length; name++)
            {
                Settings.Players.Add(new Player(names[name]));
                Debug.Log(Settings.Players[name].Name);
            }
            SceneManager.LoadScene("OyaNegotiation");
        }
    }
}