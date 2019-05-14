using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using ExtensionMethods;
// ReSharper disable All
/*
 * Todo:
 *  - Dynamische Generierung der Card Skins
 */

namespace Hanafuda
{
    public partial class Global : MonoBehaviour
    {
        public static Action NoAction = () => { Debug.Log("Not Implemented Action Called"); };
        public static Global instance;
        public static int MovingCards;
        public static Sprite[] CardSkins;
        public static int Turn = -1;
        public static BoxCollider prev;
        public static List<Card> allCards = new List<Card>();
        public static List<Yaku> allYaku = new List<Yaku>();
        public static List<string> Spielverlauf = new List<string>();
        public List<Card> AllCards = new List<Card>();
        public List<Yaku> AllYaku = new List<Yaku>();
        public Texture2D[] Skins;
        public UnityEngine.Object Logger;

        private static System.Diagnostics.Process process;
        private static bool AllowLog = false;

        public void OnApplicationQuit()
        {
            process.Kill();
        }

        public static void Log(string output)
        {
            if (AllowLog)
            {
                process.StandardInput.WriteLine(output);
            }
        }

        private void Awake()
        {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR
                AllowLog = true;
                string filePath = Application.dataPath + AssetDatabase.GetAssetPath(Logger).Replace("Assets", "");
                process = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo() { FileName = filePath, RedirectStandardInput = true, UseShellExecute = false });
#endif
            instance = this;
            DontDestroyOnLoad(this);
            allYaku = AllYaku;
            allCards = AllCards;
            for (int i = 0; i < allCards.Count; i++)
                allCards[i].ID = i;
            CardSkins = new Sprite[Skins.Length];
            for (var i = 0; i < Skins.Length; i++)
                CardSkins[i] = Sprite.Create(Skins[0], new Rect(0, 0, Skins[i].width, Skins[i].height),
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