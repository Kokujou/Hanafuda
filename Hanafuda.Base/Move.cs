using System;


namespace Hanafuda.Base
{
    [Serializable]
    public class Move
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

        public override string ToString()
        {
            if (SingleSelection.Length > 0)
                return SingleSelection;
            else
            {
                string handFieldString = "";
                if (HandFieldSelection.Length > 0)
                    handFieldString = $"&& {HandFieldSelection}";
                string deckFieldString = "";
                if (DeckFieldSelection.Length > 0)
                    deckFieldString = $"&& {DeckFieldSelection}";
                string yakuString = HadYaku ? "Had Yaku" : "";
                string KoikoiString = Koikoi ? "Said Koi Koi" : "Didn't say Koi Koi";
                return $"Hand: {HandSelection} {handFieldString} >> Deck: {DeckSelection} {deckFieldString} >> {yakuString} >> {KoikoiString}";
            }
        }

    }
}