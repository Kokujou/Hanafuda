using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;
using UnityEngine.SceneManagement;

namespace Hanafuda
{
    public partial class Communication : MonoBehaviour
    {
        public Action<int> OnDeckSync = x => Global.NoAction();
        public Action<Move> OnMoveSync = x => Global.NoAction();

        private void OnDisconnect(NetworkMessage msg)
        {
            Instantiate(Global.prefabCollection.PText).GetComponent<TextMesh>().text ="Verbindungsfehler";
        }
        private void ReceiveSeed(NetworkMessage msg)
        {
            int seed = msg.ReadMessage<Seed>().seed;
            OnDeckSync(seed);
            OnDeckSync = x => Global.NoAction();
        }
        private void ReceiveMove(NetworkMessage msg)
        {
            Move action = msg.ReadMessage<Move>();
            OnMoveSync(action);
        }

        private void BroadcastMove(NetworkMessage msg)
        {
            StartCoroutine(DoTillSuccess(() => NetworkServer.SendToAll(MoveSyncMsg, msg.ReadMessage<Move>())));
        }

        public void BroadcastSeed(int seed)
        {
            StartCoroutine(DoTillSuccess(() => NetworkServer.SendToAll(DeckSyncMsg, new Seed { seed = seed })));
        }

        public void SendAction(Move action)
        {
            StartCoroutine(DoTillSuccess(() => Settings.Client.Send(MoveSyncMsg, action)));
        }

        private IEnumerator DoTillSuccess(Func<bool> action)
        {
            int i = 0;
            while (!action())
            {
                i++;
                yield return null;
            }
            Debug.Log(i);
        }
    }
}