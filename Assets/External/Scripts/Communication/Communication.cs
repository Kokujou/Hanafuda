using ExtensionMethods;
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

        private void OnDisconnect(byte[] msg)
        {
            Instantiate(Global.prefabCollection.PText).GetComponent<TextMesh>().text ="Verbindungsfehler";
        }
        private void ReceiveSeed(byte[] msg)
        {
            int seed = msg.Deserialize<int>();
            OnDeckSync(seed);
            OnDeckSync = x => Global.NoAction();
        }
        private void ReceiveMove(byte[] msg)
        {
            Move action = msg.Deserialize<Move>();
            OnMoveSync(action);
        }

        private async void BroadcastMove(byte[] msg)
        {
            await Settings.Server.SendToAll(MoveSyncMsg, msg.Deserialize<Move>());
        }

        public async void BroadcastSeed(int seed)
        {
            await Settings.Server.SendToAll(DeckSyncMsg, new Seed { seed = seed });
        }

        public async void SendAction(Move action)
        {
            await Settings.Client.Send(MoveSyncMsg, action);
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