using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Hanafuda {
    public class Drag : MonoBehaviour
    {
        public RectTransform Target;
        public float CanvasWidth;

        private int expand = 1;

        private const float _FlipBackTreshold = .25f;

        public void OnDrag(BaseEventData data)
        {
            PointerEventData pointer = (PointerEventData)data;
            Target.localPosition += (pointer.delta.x / GetComponentInParent<Canvas>().scaleFactor) * Vector3.right;
            if (Target.localPosition.x > 0) Target.localPosition = Vector3.zero;
            else if (Target.localPosition.x < -CanvasWidth) Target.localPosition = Vector3.right * -CanvasWidth;
        }

        public void OnDrop(BaseEventData data)
        {
            PointerEventData pointer = (PointerEventData)data;
            if ((expand > 0 && Target.localPosition.x > -CanvasWidth * (1 - _FlipBackTreshold)) ||
                (expand < 0 && Target.localPosition.x < -CanvasWidth * _FlipBackTreshold))
                expand *= -1;
            Target.localPosition = (expand + 1) * -0.5f * CanvasWidth * Vector3.right;
            transform.rotation = Quaternion.Euler(180 * -0.5f * (expand - 1) * Vector3.up);
            Color darkGray = new Color(.25f, .25f, .25f);
            Color lightGray = new Color(.75f, .75f, .75f);
            GetComponentInChildren<Text>().color = expand > 0 ? lightGray : darkGray;
            GetComponent<Image>().color = expand < 0 ? lightGray : darkGray;
        }
    }
}
