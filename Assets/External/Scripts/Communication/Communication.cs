using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Hanafuda
{
    public partial class Main
    {
        void SyncDeck(NetworkMessage msg)
        {
            int seed = Convert.ToInt32(msg.ReadMessage<Global.Message>().message);
            GenerateDeck(seed);
        }
        void SyncMove(NetworkMessage msg)
        {
            string[] splitted = msg.ReadMessage<Global.Message>().message.Split(',');
            int[] move = new int[3];
            for (int i = 0; i < splitted.Length && i < 3; i++)
                move[i] = Convert.ToInt32(splitted[i]);
            Debug.Log(move[0] + "," + move[1] + "," + move[2]);
            Board.DrawTurn(move);
        }
        private void RegisterHandlers()
        {
            NetworkServer.RegisterHandler(MoveSyncMsg, SyncMove);
            for (int i = 0; i < Global.Settings.playerClients.Count; i++)
            {
                Global.Settings.playerClients[i].RegisterHandler(MoveSyncMsg, SyncMove);
                Global.Settings.playerClients[i].RegisterHandler(DeckSyncMsg, SyncDeck);
            }
            if (NetworkServer.active)
            {
                int seed = UnityEngine.Random.Range(0, 1000);
                NetworkServer.SendToAll(DeckSyncMsg, new Global.Message() { message = seed.ToString() });
                GenerateDeck(seed);
            }
        }
    }
}