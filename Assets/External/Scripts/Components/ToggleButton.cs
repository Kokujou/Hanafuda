using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ToggleButton : MonoBehaviour
{
    public List<Button> Buttons = new List<Button>();
    public Color SelectedNormal;
    public Color SelectedPushed;
    public Color StandardNormal;
    public Color StandardPushed;
    public bool AllowMultiselect = false;

    [Serializable]
    public class OnSelected : UnityEvent<int> { }
    [SerializeField]
    public OnSelected onSelected;

    // Start is called before the first frame update
    void Start()
    {
        foreach (Button button in Buttons)
        {
            button.onClick.AddListener(OnClick);
            button.name = "unselected";
            button.colors = new ColorBlock()
            {
                normalColor = StandardNormal,
                pressedColor = StandardPushed,
                highlightedColor = StandardNormal,
                fadeDuration = 0.1f,
                colorMultiplier = 1f
            };
        }
        Buttons[0].name = "selected";
        Buttons[0].colors = new ColorBlock()
        {
            normalColor = SelectedNormal,
            pressedColor = SelectedPushed,
            highlightedColor = SelectedNormal,
            fadeDuration = 0.1f,
            colorMultiplier = 1f
        };
        onSelected.Invoke(0);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnClick()
    {
        GameObject GO = EventSystem.current.currentSelectedGameObject;
        Button current = GO.GetComponent<Button>();
        if (current.name == "selected") return;
        if (!AllowMultiselect)
        {
            foreach (Button button in Buttons)
            {
                button.name = "unselected";
                button.colors = new ColorBlock()
                {
                    normalColor = StandardNormal,
                    pressedColor = StandardPushed,
                    highlightedColor = StandardNormal,
                    fadeDuration = 0.1f,
                    colorMultiplier = 1f
                };
            }
        }
        current.name = "selected";
        current.colors = new ColorBlock()
        {
            normalColor = SelectedNormal,
            pressedColor = SelectedPushed,
            highlightedColor = SelectedNormal,
            fadeDuration = 0.1f,
            colorMultiplier = 1f
        };
        onSelected.Invoke(Buttons.IndexOf(current));
    }
}
