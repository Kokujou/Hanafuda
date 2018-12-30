﻿using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;
using UnityEngine.SceneManagement;

namespace Hanafuda
{
    public partial class Communication
    {
        private const int MaxPlayer = 2;
        private const int MoveSyncMsg = 131;
        private const int DeckSyncMsg = 132;
        private const int AddPlayerMsg = 133;
        private const int PlayerSyncMsg = 134;

        private List<Player> connected;

        public void RegisterHandlers()
        {
            NetworkServer.RegisterHandler(MoveSyncMsg, SyncMove);
            if (!NetworkServer.active)
                Settings.Client.RegisterHandler(DeckSyncMsg, SyncDeck);
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
            NetworkManager.singleton.StartMatchMaker();
            NetworkManager.singleton.autoCreatePlayer = false;
            NetworkManager.singleton.matchMaker.CreateMatch(Settings.GetMatchName(), 2, true, "", "", "", 0, 0, WaitForPlayers);
        }
        private void WaitForPlayers(bool success, string extendedInfo, MatchInfo matchInfo)
        {
            if (success)
            {
                NetworkServer.Listen(matchInfo, 9000);
                NetworkServer.RegisterHandler(AddPlayerMsg, AddPlayer);
                connected = new List<Player>();
                Settings.Client = ClientScene.ConnectLocalServer();
                Settings.Client.RegisterHandler(MsgType.Connect, WaitForStart);
            }
            else
            {
                Debug.LogError("Create match failed");
            }
        }
        private void AddPlayer(NetworkMessage msg)
        {
            string name = msg.ReadMessage<Message>().message;
            connected.Add(new Player(name));
            if (connected.Count == MaxPlayer)
            {
                Debug.Log("Alle Spieler verbunden");
                NetworkServer.UnregisterHandler(AddPlayerMsg);
                NetworkServer.SendToAll(PlayerSyncMsg, new Message { message = string.Join("|", connected.Select(x => x.Name)) });
            }
            else
                Debug.Log(connected.Count);
        }

        /// <summary>
        /// Matchsuche nach Namen
        /// </summary>
        /// <param name="partner">Partnername (=Matchname)</param>
        public void SearchMatch(string partner, bool rounds6)
        {
            Settings.Players = new List<Player>() { new Player(partner) };
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
                client.RegisterHandler(MsgType.Connect, WaitForStart);
                client.Connect(matchInfo);
                Settings.Client = client;
            }
            else
            {
                Debug.LogError("Join/Create match failed");
            }
        }
        private void WaitForStart(NetworkMessage msg)
        {
            Settings.Client.UnregisterHandler(MsgType.Connect);
            Settings.Client.RegisterHandler(PlayerSyncMsg, SyncAndStart);
            Settings.Client.Send(AddPlayerMsg, new Message { message = Settings.Players[0].Name });
            Debug.Log("Send Add Player");
        }
        private void SyncAndStart(NetworkMessage msg)
        {
            Debug.Log("SyncAndStart");
            Settings.Client.UnregisterHandler(PlayerSyncMsg);
            string[] names = msg.ReadMessage<Message>().message.Split('|');
            Settings.PlayerID = names.ToList().IndexOf(Settings.Players[0].Name);
            Settings.Players.Clear();
            for (int name = 0; name < names.Length; name++)
                Settings.Players.Add(new Player(names[name]));
            SceneManager.LoadScene("OyaNegotiation");
        }
    }
}