using Hanafuda;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
///     Klasse für Runden- und/oder Spielende
/// </summary>
public class Finish : MonoBehaviour
{
    private bool showWindow;
    public GUISkin skin;
    private Rect WindowRect = new Rect(Screen.width / 2 - 250, Screen.height / 2 - 70, 400, 110);

    /// <summary>
    ///     Aktualisieren der globalen Werte basierend auf Rundenergebnis
    /// </summary>
    private void Start()
    {
        StopAllCoroutines();
        Global.players[0].pTotalPoints.Add(Global.players[0].tempPoints);
        Global.players[1].pTotalPoints.Add(Global.players[1].tempPoints);
        Yaku.DistinctYakus(Global.players[0].CollectedYaku);
        Yaku.DistinctYakus(Global.players[1].CollectedYaku);
    }

    /// <summary>
    ///     Erstellung einer Übersicht zum Spielende
    /// </summary>
    private void OnGUI()
    {
        /*
         * Inhalt:
         *  - Punkteübersicht in der Runde
         *  - Punkteübersicht des gesamten Spiels
         *  - Fortsetzungsbutton
         *  - Übersicht der gesammelten Karten beider Spieler (geplant)
         */
        GUI.skin = skin;
        GUI.BeginGroup(new Rect(50, 20, Screen.width - 100, Screen.height - 40));
        GUI.Box(new Rect(0, 0, Screen.width - 100, Screen.height - 40), "Übersicht");
        {
            GUI.Label(new Rect(0, 50, Screen.width / 2 - 50, 50), Global.players[0].Name,
                new GUIStyle(GUI.skin.GetStyle("Label")) {fontSize = 30, alignment = TextAnchor.MiddleCenter});
            GUI.Label(new Rect(Screen.width / 2 - 50, 50, Screen.width / 2 - 50, 50), Global.players[1].Name,
                new GUIStyle(GUI.skin.GetStyle("Label")) {fontSize = 30, alignment = TextAnchor.MiddleCenter});
            var maxCount = 0;
            foreach (var yaku in Global.players[0].CollectedYaku)
            {
                GUI.Label(new Rect(0, 100 + maxCount * 30, Screen.width / 3 - 33, 30),
                    yaku.Key.Name + " - +" + yaku.Value);
                maxCount++;
            }

            var tCount = 0;
            foreach (var yaku in Global.players[1].CollectedYaku)
            {
                GUI.Label(new Rect(Screen.width / 2 - 50, 100 + tCount * 30, Screen.width / 3 - 33, 30),
                    yaku.Key.Name + " - +" + yaku.Value);
                tCount++;
            }

            if (tCount > maxCount)
                maxCount = tCount;
            var temp = new Texture2D(1, 1);
            temp.SetPixel(0, 0, Color.gray);
            temp.Apply();
            GUI.DrawTexture(new Rect(50, 100 + maxCount * 30, Screen.width - 200, 2), temp);
            GUI.Label(new Rect(0, 110 + maxCount * 30, Screen.width / 3 - 33, 30),
                "Gesamt - " + Global.players[0].tempPoints);
            GUI.Label(new Rect(Screen.width / 2 - 50, 110 + maxCount * 30, Screen.width / 3 - 33, 30),
                "Gesamt - " + Global.players[1].tempPoints);
            for (var i = 0; i < (Global.Settings.Rounds6 ? 6 : 12); i++)
            {
                var offsetX = Screen.width / 2 - 50 - 50 * (Global.Settings.Rounds6 ? 6 : 12) / 2;
                GUI.Label(new Rect(0, 160 + maxCount * 30, offsetX - 10, 50), Global.players[0].Name,
                    new GUIStyle(GUI.skin.GetStyle("Label")) {alignment = TextAnchor.MiddleCenter, fontSize = 40});
                GUI.Label(new Rect(0, 210 + maxCount * 30, offsetX - 10, 50), Global.players[1].Name,
                    new GUIStyle(GUI.skin.GetStyle("Label")) {alignment = TextAnchor.MiddleCenter, fontSize = 40});

                if (i < Global.players[0].pTotalPoints.Count)
                    GUI.Box(new Rect(offsetX + i * 50, 160 + maxCount * 30, 50, 50),
                        Global.players[0].pTotalPoints[i].ToString());
                else
                    GUI.Box(new Rect(offsetX + i * 50, 160 + maxCount * 30, 50, 50), "");
                if (i < Global.players[1].pTotalPoints.Count)
                    GUI.Box(new Rect(offsetX + i * 50, 210 + maxCount * 30, 50, 50),
                        Global.players[1].pTotalPoints[i].ToString());
                else
                    GUI.Box(new Rect(offsetX + i * 50, 210 + maxCount * 30, 50, 50), "");
            }

            if (GUI.Button(new Rect(Screen.width / 2 - 150, 280 + maxCount * 30, 200, 50), "Weiter"))
            {
                if (Global.players[0].pTotalPoints.Count < (Global.Settings.Rounds6 ? 6 : 12))
                    SceneManager.LoadScene("Singleplayer");
                else
                    showWindow = true;
            }

            if (showWindow)
                WindowRect = GUI.Window(0, WindowRect, x =>
                {
                    GUI.Label(new Rect(5, 5, 390, 70),
                        "Die maximale Anzahl von Runden wurde erreicht. Es folgt die Weiterleitung zum Startbildschirm.",
                        new GUIStyle(GUI.skin.GetStyle("Label")) {alignment = TextAnchor.MiddleCenter});
                    if (GUI.Button(new Rect(150, 75, 100, 30), "OK!")) showWindow = false;
                    GUI.DragWindow();
                }, "Bestätigung");
        }
        GUI.EndGroup();
    }
}