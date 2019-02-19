using ExtensionMethods;
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

        private void HandleDisconnect()
        {

        }

        private IEnumerator ReconnectLoop()
        {
            float start = Time.unscaledTime;
            while(!PhotonNetwork.IsConnected && Time.unscaledTime - start < _ConnectionTimeout)
            {
                PhotonNetwork.ReconnectAndRejoin();
                yield return null;
            }
            if(!PhotonNetwork.IsConnected)
            {
                HandleDisconnect();
            }
        }

        public override void OnDisconnected(DisconnectCause cause)
        {
            if (cause == DisconnectCause.DisconnectByClientLogic || 
                cause == DisconnectCause.DisconnectByServerLogic) return;
            Instantiate(Global.prefabCollection.PText).GetComponent<TextMesh>().text = cause.ToString();
            StartCoroutine(ReconnectLoop());
        }

        [PunRPC]
        private async Task ReceiveSeed(int seed, PhotonMessageInfo info)
        {
            while (!DeckSyncSet) await Task.Yield();
            Debug.Log("Received Random Seed");
            OnDeckSync(seed);
            OnDeckSync = x => Global.NoAction();
            DeckSyncSet = false;
        }

        [PunRPC]
        private async Task ReceiveMove(byte[] message, PhotonMessageInfo info)
        {
            
            while (!MoveSyncSet) await Task.Yield();
            Move action = message.Deserialize<Move>();
            OnMoveSync(action);
        }

        private void BroadcastMove(Move action)
        {
            PhotonView.Get(this).RPC("ReceiveMove", RpcTarget.AllBuffered, (object)action.Serialize());
        }

        public void BroadcastSeed(int seed)
        {
            PhotonView.Get(this).RPC("ReceiveSeed", RpcTarget.AllBuffered, seed);
        }

        public void SendAction(Move action)
        {
            BroadcastMove(action);
        }

    }
}