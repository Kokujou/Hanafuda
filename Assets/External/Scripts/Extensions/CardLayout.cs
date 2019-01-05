using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hanafuda
{
    public class CardLayout
    {
        public bool RowWise;
        public float Delay;
        public bool IsMobileHand;
        public int MaxSize;
        public CardLayout(bool isHand, float delay = 0f)
        {
            IsMobileHand = Settings.Mobile && isHand;
            RowWise = false;
            Delay = delay;
            if (Settings.Mobile && !isHand)
                MaxSize = 3;
            else
            {
                if (isHand)
                    MaxSize = 1;
                else
                    MaxSize = 2;
            }
        }
    }
}