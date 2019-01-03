using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Hanafuda
{
    public class Message : MessageBase
    {
        public string message;
    }
    public class Move : MessageBase
    {
        public string SingleSelection;
        public string HandSelection;
        public string HandFieldSelection;
        public string DeckSelection;
        public string DeckFieldSelection;
        public bool hadYaku;
        public bool Koikoi;
        public int PlayerID;
    }
}