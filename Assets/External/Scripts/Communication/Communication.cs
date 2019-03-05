﻿using ExtensionMethods;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;
using UnityEngine.SceneManagement;

namespace Hanafuda
{
    public partial class Communication : MonoBehaviourPunCallbacks
    {
        public Action<int> OnDeckSync = x => Global.NoAction();
        public Action<Move> OnMoveSync = x => Global.NoAction();

        public bool DeckSyncSet = false;
        public bool MoveSyncSet = false;

        public const float _ConnectionTimeout = 10;

        private List<Action> Pending = new List<Action>();

        private void HandleDisconnect()
        {
            Destroy(Global.instance.gameObject);
            SceneManager.LoadScene("Startup");
        }

        private IEnumerator ReconnectLoop()
        {
            string caption = "";
            MessageBox box = Instantiate(Global.prefabCollection.UIMessageBox).GetComponentInChildren<MessageBox>();
            box.Setup("Verbindungsverlust", caption, destroyCallback: () => PhotonNetwork.IsConnectedAndReady);
            float start = Time.unscaledTime;
            int elapsed = 0;
            while (!PhotonNetwork.IsConnectedAndReady && elapsed < _ConnectionTimeout)
            {
                elapsed = (int)(Time.unscaledTime - start);
                caption = "Die Verbindung zum Mitspieler wurde getrennt. Es wird nun versucht sie wieder herzustellen. \n\n" +
                $"In {_ConnectionTimeout - elapsed} Sekunden wird das Spiel abgebrochen.";
                box.Content.text = caption;
                if (PhotonNetwork.NetworkingClient.LoadBalancingPeer.PeerState == ExitGames.Client.Photon.PeerStateValue.Disconnected)
                    PhotonNetwork.ReconnectAndRejoin();
                yield return null;
            }
            if (!PhotonNetwork.IsConnectedAndReady)
                HandleDisconnect();
            else
            {
                foreach (Action action in Pending)
                    action();
            }
        }

        public override void OnDisconnected(DisconnectCause cause)
        {
            if (cause == DisconnectCause.DisconnectByClientLogic) return;
            StartCoroutine(ReconnectLoop());
        }

        [PunRPC]
        private async Task ReceiveSeed(int seed, PhotonMessageInfo info)
        {
            if (info.SentServerTime > Settings.LastTime)
                Settings.LastTime = info.SentServerTime;
            else return;
            while (!DeckSyncSet) await Task.Yield();
            Debug.Log("Received Random Seed");
            OnDeckSync(seed);
            OnDeckSync = x => Global.NoAction();
            DeckSyncSet = false;
        }

        [PunRPC]
        private async Task ReceiveMove(byte[] message, PhotonMessageInfo info)
        {
            if (info.SentServerTime > Settings.LastTime)
                Settings.LastTime = info.SentServerTime;
            else return;
            while (!MoveSyncSet) await Task.Yield();
            Move action = message.Deserialize<Move>();
            OnMoveSync(action);
        }

        private void BroadcastMove(Move action)
        {
            Action sending = () => PhotonView.Get(this).RPC("ReceiveMove", RpcTarget.AllBuffered, (object)action.Serialize());
            if (PhotonNetwork.IsConnectedAndReady)
                sending();
            else
                Pending.Add(sending);

        }

        public void BroadcastSeed(int seed)
        {
            Action sending = () => PhotonView.Get(this).RPC("ReceiveSeed", RpcTarget.AllBuffered, seed);
            if (PhotonNetwork.IsConnectedAndReady)
                sending();
            else
                Pending.Add(sending);
        }

        public void SendAction(Move action)
        {
            BroadcastMove(action);
        }
    }
}