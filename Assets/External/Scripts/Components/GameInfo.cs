using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Hanafuda {
    public class GameInfo : MonoBehaviour
    {
        public UIYaku P1Yakus;
        public UIYaku P2Yakus;

        public Text P1Text;
        public Text P2Text;

        public void Start()
        {
            P1Text.text = Settings.Players[0].Name;
            P2Text.text = Settings.Players[1].Name;
        }

        public void AddCards(int id, List<Card> cards)
        {
            if (id == 0)
                P1Yakus.AddCards(cards);
            else if (id == 1)
                P2Yakus.AddCards(cards);
        }
    }
}