using System;
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

        public UIYaku GetYakuList(int id)
        {
            if (id == 0) return P1Yakus;
            else if (id == 1) return P2Yakus;
            else return null;
        }

        public void ToggleView(GameObject view)
        {
            view.SetActive(!view.activeSelf);
        }

    }
}