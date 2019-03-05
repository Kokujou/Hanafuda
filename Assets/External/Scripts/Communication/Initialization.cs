using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

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

        public Transform PlayerListParent;

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
            OnMasterConnection = () => PhotonNetwork.CreateRoom(Settings.GetMatchName(), new RoomOptions() { MaxPlayers = 2, EmptyRoomTtl = 60, PlayerTtl = 60 });
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
            Debug.Log("Dem Raum konnte nicht beigetreten werden." + message + returnCode);
        }
        public override void OnCreateRoomFailed(short returnCode, string message)
        {
            Debug.Log("Der Raum existiert bereits.");
        }

        [PunRPC]
        private void AddPlayer(string name, PhotonMessageInfo info)
        {
            if (info.SentServerTime > Settings.LastTime)
                Settings.LastTime = info.SentServerTime;
            else return;
            connected.Add(new Player(name));
            Debug.Log($"Player Added:{name}");
            if (connected.Count == MaxPlayer)
            {
                Debug.Log("Alle Spieler verbunden");
                info.photonView.RPC("SyncAndStart", RpcTarget.AllBuffered, (object)connected.Select(x => x.Name).ToArray());
            }
        }

        /// <summary>
        /// Matchsuche nach Namen
        /// </summary>
        /// <param name="partner">Partnername (=Matchname)</param>
        public void JoinLobby(string player, bool rounds6)
        {
            Settings.Players = new List<Player>() { new Player(player) };
            PhotonNetwork.ConnectUsingSettings();
            OnMasterConnection = () => PhotonNetwork.JoinLobby();
        }

        public void JoinMatch(string matchName)
        {
            string[] matchSplit = matchName.Split('|');
            Settings.Players.Insert(0, new Player(matchSplit[1].Trim()));
            Settings.PlayerID = 1;
            Settings.Rounds6 = matchSplit[0].StartsWith("6") ? true : false;
            Settings.Multiplayer = true;
            PhotonNetwork.JoinRoom(Settings.GetMatchName());
        }

        [PunRPC]
        private void SyncAndStart(string[] names, PhotonMessageInfo info)
        {
            if (info.SentServerTime > Settings.LastTime)
                Settings.LastTime = info.SentServerTime;
            else return;
            Debug.Log("Players Completed, Starting Game");
            Settings.PlayerID = names.ToList().IndexOf(Settings.GetName());
            Settings.Players.Clear();
            for (int name = 0; name < names.Length; name++)
            {
                Settings.Players.Add(new Player(names[name]));
            }
            SceneManager.LoadScene("OyaNegotiation");
        }

        public override void OnRoomListUpdate(List<RoomInfo> roomList)
        {
            if (!PlayerListParent) return;
            foreach (RoomInfo room in roomList)
            {
                GameObject match;
                if (!room.RemovedFromList && room.IsOpen && room.PlayerCount < room.MaxPlayers)
                {
                    match = Instantiate(Global.prefabCollection.UIMatch, PlayerListParent);
                    match.name = room.Name;
                    match.GetComponent<Button>().onClick.AddListener(() => JoinMatch(match.name));
                    Text[] captions = match.GetComponentsInChildren<Text>();
                    string[] roomSplit = room.Name.Split('|');
                    captions[0].text = roomSplit[1].Trim();
                    captions[1].text = roomSplit[0].Trim();
                }
                else
                {
                    Destroy(PlayerListParent.GetComponentsInChildren<Button>().ToList().Find(x => x.name == room.Name).gameObject);
                }
            }
        }
    }
}