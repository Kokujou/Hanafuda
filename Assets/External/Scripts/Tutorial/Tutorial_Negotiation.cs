using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using Hanafuda.Extensions;
using Hanafuda.Base;
using Hanafuda.Base.Interfaces;

namespace Hanafuda
{
    public partial class Tutorial
    {
        private void InitNegotiation()
        {
            NegMain.enabled = false;
            if (NegMain.Turn == -1)
                NegMain.Start();
            if (Settings.Mobile)
                StartCoroutine(FindSlide());
        }

        private IEnumerator FindSlide()
        {
            SlideHand slide = null;
            while (!slide)
            {
                slide = FindObjectOfType<SlideHand>();
                yield return null;
            }
            slide.Init(NegMain.tempDeck.Count, x =>
            {
                if (x < 0 && HandMask)
                {
                    Destroy(HandMask.gameObject);
                    HandMask = null;
                }
                else if (!HandMask)
                    HandMask = CreateUIMask(NegMain.tempDeck[x].GetObject().transform.parent.gameObject, 0f);
                NegMain.HoverCards(x >= 0 ? NegMain.tempDeck[x] : null);
            }, ControlSelection);
            Hide = Instantiate(Global.prefabCollection.UIHide);
            CreateUIMask(slide.gameObject, 1f);
            Command = Create3DText("Durch Wischen\nKarte Auswählen");
            Command.transform.localPosition = new Vector3(0, 20, 0);
        }
        private IEnumerator AfterOppSelection()
        {
            while (NegMain.Selections.Contains(null))
                yield return null;
            Destroy(HandMask.gameObject);
            HandMask = null;
            Destroy(Command.gameObject);
            Command = null;
            Destroy(Hide.gameObject);
            Hide = null;
            Command = Create3DText("frühster Monat\ngewinnt");
            Command.transform.localPosition = Vector3.down * 15 + Vector3.back * 15;
            Command.GetComponent<TextMesh>().color = new Color(28, 165, 28, 255) / 255f;
        }

        private void ControlSelection(int selection)
        {
            ICard selected = NegMain.tempDeck[selection];
            if (selected.Month == Months.December)
            {
                if (selection == 0)
                    selected = NegMain.tempDeck[selection + 1];
                else
                    selected = NegMain.tempDeck[selection - 1];
            }
            NegMain.HoverCards(selected);
            NegMain.OnSelectItem(selected);
            Destroy(Command.gameObject);
            Command = null;
            Command = Create3DText("Gegner\nwählt\naus");
            Command.transform.localPosition = Vector3.right * 10f + Vector3.up * 20f;
            CreateUIMask(selected.GetObject(), 1f);
            StartCoroutine(Animations.AfterAnimation(() =>
            {
                StartCoroutine(NegMain.AnimOpponentChoice(Months.December));
                StartCoroutine(AfterOppSelection());
            }));
        }
    }
}