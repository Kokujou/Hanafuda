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
            public GameObject PKarte, PSlide, CherryBlossoms, YakuManager, KoikoiText, gAddYaku, gFixedYaku, gKouYaku, PText, Loading, UIHide, UIMask, UIMatch;
        }

        // Use this for initialization
    }
}