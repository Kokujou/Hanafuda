using ExtensionMethods;
using System;
using UnityEngine;
using UnityEngine.Networking;

namespace Hanafuda
{
    public partial class Main : NetworkBehaviour
    {
        // Use this for initialization
        private int _overviewMode;
        public Main main;
        private bool ShowGUI, showOverview;

        private int OverviewMode
        {
            get { return _overviewMode; }
            set
            {
                if (value > 1) _overviewMode = 0;
                else if (value < 0) _overviewMode = 2;
                else _overviewMode = value;
            }
        }

        // Update is called once per frame
        private void OnGUIPC()
        {
            GUI.skin = Global.prefabCollection.IngameSkin;
            /*
             * Einstellungen
             */
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ShowGUI = !ShowGUI;
                main.allowInput = !ShowGUI;
            }

            if (ShowGUI)
            {
                /*
                float SettingsWidth = 500, SettingsHeight = 400, SettingsTop = Screen.height / 2 - SettingsHeight / 2, SettingsLeft = Screen.width / 2 - SettingsWidth / 2;
                GUI.Box(new Rect(SettingsLeft, SettingsTop, SettingsWidth, SettingsHeight), "Einstellungen");
                GUI.BeginGroup(new Rect(SettingsLeft, SettingsTop, SettingsWidth, SettingsHeight));
                GUI.Label(new Rect(10, 50, SettingsWidth - 20, 20), "Kartenhintergrund:");
                Global.Settings.CardSkin = GUI.SelectionGrid(new Rect(10, 80, SettingsWidth - 20, 192), Global.Settings.CardSkin, Global.CardSkins, 4);
                main.PKarte.GetComponentsInChildren<MeshRenderer>().FirstOrDefault(x => x.gameObject.name == "Background").sharedMaterial.mainTexture = Global.CardSkins[Global.Settings.CardSkin];
                GUI.EndGroup();*/
            }
            /*
             * UI während aktivem Spiel
             */
            else if (!showOverview)
            {
                if (GUI.Button(new Rect(Screen.width - 100, Screen.height / 2 - 50, 100, 100), "",
                    GUI.skin.GetStyle("HelpButton")))
                    showOverview = true;
            }
            /*
             * Spielinformations-GUI:
             */
            else
            {
                transform.rotation = Quaternion.Euler(0, 180, 0);
                GUI.BeginGroup(new Rect(20, 20, Screen.width - 40, Screen.height - 40));
                GUI.Box(new Rect(0, 0, Screen.width - 40, Screen.height - 40), "");
                {
                    if (GUI.Button(new Rect(Screen.width - 80, 0, 40, 40), "✕"))
                    {
                        showOverview = false;
                        transform.rotation = Quaternion.Euler(0, 0, 0);
                    }

                    if (GUI.Button(new Rect(0, (Screen.height - 40) / 2, 40, 40), "◄"))
                        OverviewMode--;
                    if (GUI.Button(new Rect(Screen.width - 80, (Screen.height - 40) / 2 - 20, 40, 40), "►"))
                        OverviewMode++;
                    switch (OverviewMode)
                    {
                        /*
                         * Yaku-Sammlung:
                         *  - Liste aller Yaku
                         *  - Verdunkeln von nicht gesammelten Karten
                         */
                        case 0:
                            GUI.Label(new Rect(0, 0, Screen.width - 40, 40), "Yaku-Sammlung",
                                GUI.skin.GetStyle("YakuHead"));
                        {
                            GUI.BeginGroup(new Rect(Screen.width / 8, 0, Screen.width, Screen.height));
                                GUI.DrawTexture(new Rect(Screen.width * 0.25f - 20, 45, 2, Screen.height - 120),
                                    Color.gray.CreateTexture());
                            GUI.DrawTexture(new Rect(Screen.width * 0.5f - 20, 45, 2, Screen.height - 120),
                                Color.gray.CreateTexture());
                            var Sets = ((Player) main.Board.players[0]).CollectedCards;
                            for (var yaku = 0; yaku < Global.allYaku.Count; yaku++)
                            {
                                GUI.BeginGroup(new Rect(new Rect(
                                    Screen.width / 4 * (int) (yaku / (Global.allYaku.Count / 3f)),
                                    50 + 100 * (int) (yaku % (Global.allYaku.Count / 3f)), Screen.width / 4, 1000)));
                                GUI.Label(new Rect(0, 0, Screen.width / 4, 20), Global.allYaku[yaku].Title);
                                //OPTIMIEREN!
                                for (var card = 0; card < Global.allYaku[yaku].minSize; card++)
                                {
                                    var mask = Global.allYaku[yaku].Mask;
                                    if (mask[1] == 1)
                                    {
                                        if (card < Global.allYaku[yaku].Namen.Count &&
                                            Sets.Exists(x => x.Title == Global.allYaku[yaku].Namen[card]))
                                        {
                                            var t = GameObject.Find(Global.allYaku[yaku].Namen[card] + "/Foreground")
                                                .GetComponent<MeshRenderer>().material.mainTexture;
                                            GUI.DrawTexture(new Rect(card % 5 * 55, 20 + card / 5 * 85, 50, 80), t);
                                        }
                                        else if (card >= Global.allYaku[yaku].Namen.Count)
                                        {
                                            var cards = Sets.FindAll(x =>
                                                x.Typ == Global.allYaku[yaku].TypPref &&
                                                !Global.allYaku[yaku].Namen.Contains(x.Title));
                                            if (card - Global.allYaku[yaku].Namen.Count < cards.Count)
                                            {
                                                var t = cards[card - Global.allYaku[yaku].Namen.Count].Image
                                                    .mainTexture;
                                                GUI.DrawTexture(new Rect(card % 5 * 55, 20 + card / 5 * 85, 50, 80), t);
                                            }
                                            else
                                            {
                                                var aCards = Global.allCards.FindAll(x =>
                                                    x.Typ == Global.allYaku[yaku].TypPref &&
                                                    !Global.allYaku[yaku].Namen.Contains(x.Title) &&
                                                    !Sets.Exists(y => y.Title == x.Title));
                                                var t = aCards[card - Global.allYaku[yaku].Namen.Count - cards.Count]
                                                    .Image.mainTexture;
                                                GUI.DrawTexture(
                                                    new Rect((card - cards.Count) % 5 * 55, 20 + card / 5 * 85, 50, 80),
                                                    t);
                                                GUI.DrawTexture(
                                                    new Rect((card - cards.Count) * 55, 20 + card / 5 * 85, 50, 80),
                                                    new Color(0, 0, 0, .5f).CreateTexture());
                                            }
                                        }
                                        else
                                        {
                                            var t = GameObject.Find(Global.allYaku[yaku].Namen[card] + "/Foreground")
                                                .GetComponent<MeshRenderer>().material.mainTexture;
                                            GUI.DrawTexture(new Rect(card % 5 * 55, 20 + card / 5 * 85, 50, 80), t);
                                            GUI.DrawTexture(new Rect(card % 5 * 55, 20 + card / 5 * 85, 50, 80),
                                                new Color(0, 0, 0, .5f).CreateTexture());
                                        }
                                    }
                                    else
                                    {
                                        var cards = Sets.FindAll(x => x.Typ == Global.allYaku[yaku].TypPref);
                                        if (card < cards.Count)
                                        {
                                            var t = cards[card].Image.mainTexture;
                                            GUI.DrawTexture(new Rect(card % 5 * 55, 20 + card / 5 * 85, 50, 80), t);
                                        }
                                        else
                                        {
                                            var aCards = Global.allCards.FindAll(x =>
                                                x.Typ == Global.allYaku[yaku].TypPref &&
                                                !Sets.Exists(y => y.Title == x.Title));
                                            var t = aCards[card - cards.Count].Image.mainTexture;
                                            GUI.DrawTexture(
                                                new Rect((card - cards.Count) % 5 * 55, 20 + card / 5 * 85, 50, 80), t);
                                            GUI.DrawTexture(
                                                new Rect((card - cards.Count) % 5 * 55, 20 + card / 5 * 85, 50, 80),
                                                new Color(0, 0, 0, .5f).CreateTexture());
                                        }
                                    }
                                }

                                GUI.EndGroup();
                            }

                            GUI.EndGroup();
                        }
                            break;
                        /*
                         * Kartensammlung:
                         *  - Sortiere Karten nach Thema
                         *  - Verdunkeln von nicht gesammelten Karten
                         */
                        case 1:
                            GUI.Label(new Rect(0, 0, Screen.width - 40, 40), "Karten-Sammlung",
                                GUI.skin.GetStyle("YakuHead"));
                        {
                            GUI.BeginGroup(new Rect(Screen.width / 8, 0, Screen.width, Screen.height));
                            GUI.EndGroup();
                            var Sets = ((Player) main.Board.players[0]).CollectedCards;
                            foreach (int type in Enum.GetValues(typeof(Card.Typen)))
                            {
                                GUI.Label(new Rect(0, 50 + 104 * (type > 0 ? type + 1 : type), Screen.width - 40, 40),
                                    Enum.GetName(typeof(Card.Typen), type),
                                    new GUIStyle(GUI.skin.GetStyle("Label")) {alignment = TextAnchor.MiddleCenter});
                                var content = Global.allCards.FindAll(x => x.Typ == (Card.Typen) type);
                                for (var card = 0; card < content.Count; card++)
                                {
                                    GUI.DrawTexture(new Rect(
                                        (Screen.width - 40) / 2 -
                                        Screen.width / 20 *
                                        ((card > 11 ? content.Count - 12 : content.Count >= 12 ? 12 : content.Count) /
                                         2f) + card % 12 * (Screen.width / 20),
                                        80 + (95 + 10 * (1 - card / 12)) * ((type > 0 ? type + 1 : type) + card / 12),
                                        50, 80), content[card].Image.mainTexture);
                                    if (!Sets.Contains(content[card]))
                                        GUI.DrawTexture(new Rect(
                                                (Screen.width - 40) / 2 -
                                                Screen.width / 20 *
                                                ((card > 11 ? content.Count - 12 :
                                                     content.Count >= 12 ? 12 : content.Count) / 2f) +
                                                card % 12 * (Screen.width / 20),
                                                80 + (95 + 10 * (1 - card / 12)) *
                                                ((type > 0 ? type + 1 : type) + card / 12), 50, 80),
                                            new Color(0, 0, 0, .5f).CreateTexture());
                                }
                            }
                        }
                            break;
                    }
                }
                GUI.EndGroup();
            }

            /*
             * Yaku-Animationen:
             *  - Primär durch gestacktes Hardcoding
             *  - Unterscheidung der Animationsart nach Priorität des Yaku
             */
        }
    }
}