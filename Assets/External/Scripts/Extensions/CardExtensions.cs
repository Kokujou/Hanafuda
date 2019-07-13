using Hanafuda.Base.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Hanafuda.Extensions
{
    public static class CardExtensions
    {
        public static GameObject GetObject(this ICard card) => ((Card3D)card).GetObject();
        public static void SetObject(this ICard card, GameObject obj) => ((Card3D)card).Object = obj;
        public static Material GetImage(this ICard card) => ((Card3D)card).GetImage();
        public static void FadeCard(this ICard card, bool hide = true) => ((Card3D)card).FadeCard(hide);
        public static void HoverCard(this ICard card, bool unhover = false) => ((Card3D)card).HoverCard(unhover);
    }
}
