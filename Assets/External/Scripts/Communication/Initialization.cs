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
using Photon.Pun;

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

        private Action OnMasterConnection = () => { };


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
            connected = new List<Player>();
            PhotonNetwork.ConnectUsingSettings();
            OnMasterConnection = () => PhotonNetwork.CreateRoom(Settings.GetMatchName(), new Photon.Realtime.RoomOptions() { MaxPlayers = 2 });
        }

        public override void OnConnectedToMaster()
        {
            OnMasterConnection();
        }
        public override void OnJoinedRoom()
        {
            PhotonView view = PhotonView.Get(this);
            view.RPC("AddPlayer", RpcTarget.MasterClient, Settings.GetName());
        }
        public override void OnJoinRoomFailed(short returnCode, string message)
        {
            Debug.Log("Dem Raum konnte nicht beigetreten werden.");
        }
        public override void OnCreateRoomFailed(short returnCode, string message)
        {
            Debug.Log("Der Raum existiert bereits.");
        }

        [PunRPC]
        private void AddPlayer(string name, PhotonMessageInfo info)
        {
            connected.Add(new Player(name));
            Debug.Log($"Player Added:{name}");
            if (connected.Count == MaxPlayer)
            {
                Debug.Log("Alle Spieler verbunden");
                info.photonView.RPC("SyncAndStart", RpcTarget.AllBuffered, (object)connected.Select(x => x.Name).ToArray());
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
            Settings.Players = new List<Player>() { new Player(partner), new Player(player) };
            Settings.PlayerID = 1;
            Settings.Rounds6 = rounds6;
            Settings.Multiplayer = true;
            PhotonNetwork.ConnectUsingSettings();
            OnMasterConnection = () => PhotonNetwork.JoinRoom(Settings.GetMatchName());
        }

        [PunRPC]
        private void SyncAndStart(string[] names, PhotonMessageInfo info)
        {
            Debug.Log("SyncAndStart");
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