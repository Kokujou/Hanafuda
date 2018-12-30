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
        public Action<PlayerAction> OnMoveSync = x => Global.NoAction();

        private void ReceiveSeed(NetworkMessage msg)
        {
            int seed = Convert.ToInt32(msg.ReadMessage<Message>().message);
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
            NetworkServer.SendToAll(MoveSyncMsg, msg.ReadMessage<Move>());
        }

        public void SendSeed(int seed)
        {
            NetworkServer.SendToAll(DeckSyncMsg, new Message { message = seed.ToString() });
        }

        public void SendAction(Move action)
        {
            Settings.Client.Send(MoveSyncMsg, action);
        }
    }
}