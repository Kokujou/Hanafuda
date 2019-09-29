using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Hanafuda
{
    public class GraphicSettings : MonoBehaviour
    {
        public float AspectRatio;
        public Vector2 FullscreenResolution;
        public FullScreenMode FullscreenMode;
        public Texture CardMotive;

        private const int MinScreenWidth = 0;
        private const int MinScreenHeight = 0;
    }
}
