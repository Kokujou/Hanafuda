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
        /*void GenerateDeck(int seed)
        {
            if (Settings.Players.Count == 0)
                Board.Init(Settings.Players, seed);
            else
            {
                Board.Init(Settings.Players, seed);
                for (int i = 0; i < Board.players.Count; i++)
                    ((Player)Board.players[0]).Reset();
            }
            Board._Turn = Settings.GetName() == ((Player)Board.players[Global.Turn]).Name;
            //FieldSetup();
        }*/
        private void SyncDeck(NetworkMessage msg)
        {
            int seed = Convert.ToInt32(msg.ReadMessage<Message>().message);
            OnDeckSync(seed);
            OnDeckSync = x => Global.NoAction();
        }
        private void SyncMove(NetworkMessage msg)
        {
            Move action = msg.ReadMessage<Move>();
            OnMoveSync(action);
        }

        public void SendSeed(int seed)
        {
            NetworkServer.SendToAll(DeckSyncMsg, new Message { message = seed.ToString() });
        }

        public void SendAction(Move action)
        {
            NetworkServer.SendToAll(MoveSyncMsg, action);
        }
    }
}