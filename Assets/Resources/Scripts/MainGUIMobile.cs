using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/*
 * ToDo:
 * - Esc-Taste Aktionen
 * - GUI: Ausscrollen lassen
 *      - Idee: Speed über Zeit berechnen + über Zeit verringern
 */

namespace Hanafuda
{
    public partial class Main : NetworkBehaviour
    {
        private const float maxMobileContainerX = 23.1f;
        private float _mobileContainerX = -maxMobileContainerX;
        private bool expand;
        public Vector2 scrollPos;
        public int tab;
        public float yStart;

        private float mobileContainerX
        {
            get { return _mobileContainerX; }
            set
            {
                if (value > maxMobileContainerX)
                    _mobileContainerX = maxMobileContainerX;
                else if (value < -maxMobileContainerX)
                    _mobileContainerX = -maxMobileContainerX;
                else _mobileContainerX = value;
                var ExCol = MainSceneVariables.variableCollection.ExCol;
                ExCol.position = new Vector3(_mobileContainerX, ExCol.position.y, 5);
            }
        }

        // Update is called once per frame
        private void UpdateMobile()
        {
            var ExCol = MainSceneVariables.variableCollection.ExCol;
            if (newYaku.Count == 0)
            {
                if (Input.GetMouseButton(0))
                {
                    var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    RaycastHit hit;
                    if (ExCol.position.x != -maxMobileContainerX && ExCol.position.x != maxMobileContainerX ||
                        Physics.Raycast(ray, out hit) && hit.collider.name == "ExCol")
                        mobileContainerX = Camera.main.ScreenPointToRay(Input.mousePosition).origin.x;
                }
                else if (Input.GetMouseButtonUp(0))
                {
                    if (expand && ExCol.position.x < maxMobileContainerX * 0.5f || !expand && ExCol.position.x > -maxMobileContainerX * 0.5f)
                        expand = !expand;
                    mobileContainerX = Mathf.Infinity * (expand ? 1 : -1);
                    ExCol.GetComponent<SpriteRenderer>().flipX = expand;
                    ExCol.GetComponentInChildren<TextMesh>().text = expand ? "«" : "»";
                    ExCol.GetComponentInChildren<TextMesh>().color =
                        expand ? new Color(.75f, .75f, .75f) : new Color(.25f, .25f, .25f);
                    ExCol.GetComponent<SpriteRenderer>().color =
                        !expand ? new Color(.75f, .75f, .75f) : new Color(.25f, .25f, .25f);
                    var excol = MainSceneVariables.variableCollection.ExColBack;
                    excol.localPosition = new Vector3(MainSceneVariables.variableCollection.BoxX + (expand ? .8f : 0),
                        excol.localPosition.y, excol.localPosition.z);
                }
            }
        }

        private void OnGUIMobile()
        {
            GUI.skin = Global.prefabCollection.MGUISkin;
            if (mobileContainerX > -maxMobileContainerX)
            {
                GUI.skin.GetStyle("Label").fontSize = Screen.height / 35;
                GUI.skin.GetStyle("sTab").fontSize = Screen.height / 25;
                GUI.skin.GetStyle("sTab").overflow.top = -Screen.height / 25;
                GUI.skin.GetStyle("Tab").fontSize = Screen.height / 25;
                if (mobileContainerX == maxMobileContainerX)
                {
                    if (Input.GetMouseButtonDown(0))
                        yStart = Input.mousePosition.y;
                    if (Input.GetMouseButton(0))
                    {
                        scrollPos += new Vector2(0, Input.mousePosition.y - yStart);
                        yStart = Input.mousePosition.y;
                        if (scrollPos.y < 0) scrollPos.y = 0;
                    }
                }

                var GUI_X = (mobileContainerX - maxMobileContainerX) / (maxMobileContainerX * 2) * Screen.width;
                GUILayout.BeginArea(new Rect(GUI_X, 10, Screen.width, Screen.height));
                {
                    GUILayout.BeginHorizontal();
                    for (var i = 0; i < 2; i++)
                        if (GUILayout.Button("Spieler " + (i + 1),
                            Global.prefabCollection.MGUISkin.GetStyle(i == tab ? "sTab" : "Tab")))
                            tab = i;
                    GUILayout.EndHorizontal();
                    var clamp = GUILayout.BeginScrollView(scrollPos, GUIStyle.none, GUIStyle.none);
                    if (scrollPos.y > clamp.y) scrollPos.y = clamp.y;
                    GUILayout.BeginVertical();
                    for (var yaku = 0; yaku < Global.allYaku.Count; yaku++)
                    {
                        GUILayout.Label(Global.allYaku[yaku].JName + "\t" + Global.allYaku[yaku].Name);
                        GUILayout.BeginHorizontal();
                        var shownCards = new List<Card>();
                        if (Global.allYaku[yaku].Mask[1] == 1)
                            shownCards.AddRange(
                                Global.allCards.FindAll(x => Global.allYaku[yaku].Namen.Contains(x.Name)));
                        if (Global.allYaku[yaku].Mask[0] == 1)
                            shownCards.AddRange(Global.allCards.FindAll(x =>
                                !Global.allYaku[yaku].Namen.Contains(x.Name) && x.Typ == Global.allYaku[yaku].TypPref));
                        var colCards = shownCards.FindAll(x =>
                            ((Player) Board.players[tab]).CollectedCards.Exists(y => y.Name == x.Name));
                        shownCards.RemoveAll(x => colCards.Exists(y => y.Name == x.Name));
                        for (var card = 0; card < shownCards.Count && card < Global.allYaku[yaku].minSize; card++)
                        {
                            if (card % 6 == 5)
                            {
                                GUILayout.EndHorizontal();
                                GUILayout.BeginHorizontal();
                            }

                            if (card >= colCards.Count)
                            {
                                var img = shownCards[card].Image.mainTexture;
                                GUILayout.Label(img, GUILayout.Width(Screen.width / 6),
                                    GUILayout.Height(Screen.width / 6 * 1.6f));
                                var filter = new Texture2D(1, 1);
                                filter.SetPixel(0, 0, new Color(0, 0, 0, .5f));
                                filter.Apply();
                                GUI.DrawTexture(GUILayoutUtility.GetLastRect(), filter);
                            }
                            else
                            {
                                var img = colCards[card].Image.mainTexture;
                                GUILayout.Label(img, GUILayout.Width(Screen.width / 6),
                                    GUILayout.Height(Screen.width / 6 * 1.6f));
                            }
                        }

                        GUILayout.EndHorizontal();
                    }

                    GUILayout.EndVertical();
                    GUILayout.EndScrollView();
                }
                GUILayout.EndArea();
            }
        }
    }
}