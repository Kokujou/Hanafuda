using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace Hanafuda
{
    public class Message : MessageBase
    {
        public string message;
    }
    public partial class Main
    {
        void GenerateDeck(int seed)
        {
            if (Global.players.Count == 0)
                Board.Init(new List<object>() { new Player(Global.Settings.P1Name), new Player(Global.Settings.P2Name) }, TurnCallback, seed);
            else
            {
                Board.Init(Global.players.Cast<object>().ToList(), TurnCallback, seed);
                for (int i = 0; i < Board.players.Count; i++)
                    ((Player)Board.players[0]).Reset();
            }
            Board._Turn = Global.Settings.Name == ((Player)Board.players[Global.Turn]).Name;
            //FieldSetup();
        }
        void SyncDeck(NetworkMessage msg)
        {
            int seed = Convert.ToInt32(msg.ReadMessage<Message>().message);
            GenerateDeck(seed);
        }
        void SyncMove(NetworkMessage msg)
        {
            string[] splitted = msg.ReadMessage<Message>().message.Split(',');
            int[] move = new int[3];
            for (int i = 0; i < splitted.Length && i < 3; i++)
                move[i] = Convert.ToInt32(splitted[i]);
            Debug.Log(move[0] + "," + move[1] + "," + move[2]);
            PlayerAction action = new PlayerAction();
            action.Init(Board);
            action.SelectFromHand(((Player)Board.players[1]).Hand[move[0]]);
            if (move[1] >= 0)
                action.SelectHandMatch(Board.Field[move[1]]);
            if (move[2] >= 0)
                action.SelectDeckMatch(Board.Field[move[2]]);
            action.Apply();
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
                NetworkServer.SendToAll(DeckSyncMsg, new Message() { message = seed.ToString() });
                GenerateDeck(seed);
            }
        }
    }
}