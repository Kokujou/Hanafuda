using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Hanafuda
{
    public class Move : MessageBase
    {
        public string SingleSelection = "";
        public string HandSelection = "";
        public string HandFieldSelection = "";
        public string DeckSelection = "";
        public string DeckFieldSelection = "";
        public bool HadYaku = false;
        public bool Koikoi = false;
        public int PlayerID = -1;
        public int MoveID = -1;
        public Move() { }
        public Move(Move copy)
        {
            SingleSelection = copy.SingleSelection;
            HandSelection = copy.HandSelection;
            HandFieldSelection = copy.HandFieldSelection;
            DeckSelection = copy.DeckSelection;
            DeckFieldSelection = copy.DeckFieldSelection;
            HadYaku = copy.HadYaku;
            Koikoi = copy.Koikoi;
            PlayerID = copy.PlayerID;
            MoveID = copy.MoveID;
        }
    }
    public class Response : MessageBase
    {
        public int PlayerID;
        public int MessageID;
    }
    public class PlayerList : MessageBase
    {
        public string[] players;
    }
    public class Seed : MessageBase
    {
        public int seed;
    }
    public class Ping : MessageBase
    {
        bool value;
    }
}