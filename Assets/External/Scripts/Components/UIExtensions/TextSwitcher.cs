using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TextSwitcher : MonoBehaviour
{
    [Serializable]
    public class StringEvent : UnityEvent<string> { }

    public StringEvent OnSwitch;
    public List<string> Values;
    public Text Content;

    private int Selected;

    public void Switch(bool forward)
    {
        if (forward)
            Selected++;
        else
            Selected--;

        Selected =
            Selected < 0 ? (Values.Count - 1) :
            Selected > (Values.Count - 1) ? 0 :
            Selected;

        Content.text = Values[Selected];

        OnSwitch.Invoke(Content.text);
    }

    public void Select(int id)
    {
        Selected = id - 1;
        Switch(true);
    }
}
