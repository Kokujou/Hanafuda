using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
// ReSharper disable All
/*
 * Todo:
 *  - Dynamische Generierung der Card Skins
 */

namespace Hanafuda
{
    public partial class Global : MonoBehaviour
    {
        public static Global global;
        public static int MovingCards;
        public static Sprite[] CardSkins;
        public static int Turn = -1;
        public static BoxCollider prev;
        public static List<Card> allCards = new List<Card>();
        public static List<Yaku> allYaku = new List<Yaku>();
        public static List<string> Spielverlauf = new List<string>();
        public static List<Player> players = new List<Player>();
        public List<Card> AllCards = new List<Card>();
        public List<Yaku> AllYaku = new List<Yaku>();
        private void Awake()
        {
            global = this;
            DontDestroyOnLoad(this);
            allYaku = AllYaku;
            allCards = AllCards;
            var skins = Resources.LoadAll<Texture2D>("Images/").Where(x => x.name.StartsWith("Back")).ToArray();
            CardSkins = new Sprite[skins.Length];
            for (var i = 0; i < skins.Length; i++)
                CardSkins[i] = Sprite.Create(skins[0], new Rect(0, 0, skins[i].width, skins[i].height),
                    new Vector2(.5f, .5f));
            prefabCollection = singleton;
            /*
            for (int i = 0; i < AllCards.Count; i++)
            {
                SerializedObject obj = new SerializedObject(allCards[i]);
                obj.Update();
                EditorUtility.SetDirty(allCards[i]);
                AllCards[i].Title = AllCards[i].name;
                obj.ApplyModifiedProperties();
            }
            for (int i = 0; i < allYaku.Count; i++)
            {
                SerializedObject obj = new SerializedObject(allYaku[i]);
                obj.Update();
                EditorUtility.SetDirty(allYaku[i]);
                allYaku[i].Title = allYaku[i].name;
                obj.ApplyModifiedProperties();
            }
            AssetDatabase.SaveAssets();*/
        }
        /*
        public static Card CreateCard(Card.Monate monat, Card.Typen typ, string Name)
        {
            Card asset = ScriptableObject.CreateInstance<Card>();
            AssetDatabase.CreateAsset(asset, "Assets/Resources/Deck/" + Name + ".asset");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;
            asset.Name = Name;
            asset.Monat = monat;
            asset.Typ = typ;
            asset.Image = Resources.Load<Material>("Motive/Materials/" + Name);
            return asset;
        }
        */
    }
}