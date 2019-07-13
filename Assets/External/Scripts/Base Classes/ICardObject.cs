
using UnityEngine;

namespace Hanafuda
{
    public interface ICardObject
    {
        Material Image { get; }
        GameObject Object { get; set; }
        void FadeCard(bool hide = true);
        void HoverCard(bool unhover = false);
    }
}
