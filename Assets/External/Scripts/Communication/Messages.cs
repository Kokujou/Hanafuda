using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Hanafuda
{
    public class Move : MessageBase
    {
        public string SingleSelection;
        public string HandSelection;
        public string HandFieldSelection;
        public string DeckSelection;
        public string DeckFieldSelection;
        public bool HadYaku;
        public bool Koikoi;
        public int PlayerID;
        public int MoveID;
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
}