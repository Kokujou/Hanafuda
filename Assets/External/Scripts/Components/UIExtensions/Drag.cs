using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Hanafuda
{
    public class Drag : MonoBehaviour
    {
        public RectTransform Target;
        public float MinPositionX;
        public float MaxPositionX;
        public float FlipBackTreshold;

        private float BorderPositionX
            => MinPositionX + ((MaxPositionX - MinPositionX) * FlipBackTreshold);

        private static readonly Color _darkGray = new Color(.25f, .25f, .25f);
        private static readonly Color _lightGray = new Color(.75f, .75f, .75f);

        public void OnDrag(BaseEventData data)
        {
            Debug.Log(Target.localPosition.x);
            PointerEventData pointer = (PointerEventData)data;
            Target.localPosition += (pointer.delta.x / GetComponentInParent<Canvas>().scaleFactor) * Vector3.right;
            if (Target.localPosition.x > MaxPositionX) Target.localPosition = new Vector2(MaxPositionX, Target.localPosition.y);
            else if (Target.localPosition.x < MinPositionX) Target.localPosition = Vector3.right * MinPositionX;
        }

        public void OnDrop(BaseEventData data)
        {
            Debug.Log(BorderPositionX);
            if (Target.localPosition.x < BorderPositionX)
            {
                Target.localPosition = Vector3.right * MinPositionX;
                transform.rotation = Quaternion.Euler(Vector3.zero);
                GetComponentInChildren<Text>().color = _darkGray;
                GetComponent<Image>().color = _lightGray;
            }
            else
            {
                Target.localPosition = Vector3.right * MaxPositionX;
                transform.rotation = Quaternion.Euler(Vector3.up * 180);
                GetComponentInChildren<Text>().color = _lightGray;
                GetComponent<Image>().color = _darkGray;
            }
        }
    }
}
