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
    public class IntegerEvent : UnityEvent<int> { }
    public IntegerEvent OnSelect;

    void Start()
    {
        foreach (Button button in Buttons)
        {
            button.onClick.AddListener(OnClick);
            Unselect(button);
        }
        Select(Buttons[0]);
    }

    public void OnClick()
    {
        GameObject GO = EventSystem.current.currentSelectedGameObject;
        Button currentButton = GO.GetComponent<Button>();
        if (!AllowMultiselect)
        {
            foreach (Button button in Buttons)
                Unselect(button);
        }
        else if (currentButton.name == "selected")
            Unselect(currentButton);
        Select(currentButton);
    }

    private void Select(Button button)
    {
        button.name = "selected";
        button.colors = new ColorBlock()
        {
            normalColor = SelectedNormal,
            pressedColor = SelectedPushed,
            highlightedColor = SelectedNormal,
            fadeDuration = 0.1f,
            colorMultiplier = 1f
        };
        OnSelect.Invoke(Buttons.IndexOf(button));
    }

    private void Unselect(Button button)
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
