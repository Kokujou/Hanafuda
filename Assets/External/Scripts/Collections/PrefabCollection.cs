using System;
using UnityEngine;

namespace Hanafuda
{
    public partial class Global : MonoBehaviour
    {
        public static PrefabCollection prefabCollection;

        [SerializeField] public PrefabCollection singleton;

        [Serializable]
        public class PrefabCollection
        {
            public Font EdoFont;
            public GUISkin MGUISkin, IngameSkin, FinishSkin;
            public GameObject PKarte, PSlide;
            public GameObject CherryBlossoms, YakuManager, KoikoiText, gAddYaku, gFixedYaku, gKouYaku;
            public GameObject Loading, PText, UIHide, UIMask, UIMatch, UIMessageBox, UIButton;
        }

        // Use this for initialization
    }
}