using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
// ReSharper disable All

namespace Hanafuda
{
    public partial class Global : MonoBehaviour
    {
        public static int MovingCards;
        public static Sprite[] CardSkins;
        public static int Turn = -1;
        public static Font JFont;
        public static BoxCollider prev;
        public static List<Card> allCards = new List<Card>();
        public static List<Yaku> allYaku = new List<Yaku>();
        public static List<string> Spielverlauf = new List<string>();
        public static List<Player> players = new List<Player>();
        public Font jFont;

        /// <summary>
        ///     Generierung einer 1x1 Textur, die nur aus einer Farbe besteht
        /// </summary>
        /// <param name="color">Farbe der Textur</param>
        /// <returns></returns>
        public static Texture2D ColorTex(Color color)
        {
            var result = new Texture2D(1, 1);
            result.SetPixel(0, 0, color);
            result.Apply();
            return result;
        }
        public static void SetCameraRect(Camera cam)
        {
            if (Screen.width >= Screen.height)
                cam.aspect = 16f / 9f;
            else
                cam.aspect = .6f;
        }
        /// <summary>
        ///     Harte Wertinitialisierung von Karten und Yaku
        /// </summary>
        private void Awake()
        {
            DontDestroyOnLoad(this);
            JFont = jFont;
            Settings.mobile = Camera.main.aspect < 1;
            var skins = Resources.LoadAll<Texture2D>("Images/").Where(x => x.name.StartsWith("Back")).ToArray();
            CardSkins = new Sprite[skins.Length];
            for (var i = 0; i < skins.Length; i++)
                CardSkins[i] = Sprite.Create(skins[0], new Rect(0, 0, skins[i].width, skins[i].height),
                    new Vector2(.5f, .5f));
            prefabCollection = singleton;
        }

        public class Message : MessageBase
        {
            public string message;
        }
        public static Yaku CreateYaku(string Name, string jName, int[] mask, int basepoints, int minsize, List<string> Namen = null,
            Card.Typen typPref = Card.Typen.None, int addPoints = 0)
        {
            Yaku asset = ScriptableObject.CreateInstance<Yaku>();
            AssetDatabase.CreateAsset(asset, "Assets/Resources/Yaku/" + Name + ".asset");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;
            asset.name = Name;
            asset.JName = jName;
            asset.addPoints = addPoints;
            asset.basePoints = basepoints;
            asset.Namen = Namen;
            asset.minSize = minsize;
            asset.Mask = mask;
            asset.TypPref = typPref;
            return asset;
        }
    }
}