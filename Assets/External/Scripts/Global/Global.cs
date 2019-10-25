using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Diagnostics;
// ReSharper disable All
/*
 * Todo:
 *  - Dynamische Generierung der Card Skins
 */

namespace Hanafuda
{
    public partial class Global : MonoBehaviour
    {
        private static Process LoggerProcess;
        public static GraphicSettings Graphics;
        public static Global Instance;

        public static Action NoAction = () => { };

        public static int MovingCards;
        public static BoxCollider prev;

        public static List<Card> allCards = new List<Card>();
        public static List<Yaku> allYaku = new List<Yaku>();
        public List<Card> AllCards = new List<Card>();
        public List<Yaku> AllYaku = new List<Yaku>();
        public Texture DefaultCardMotive;
        public UnityEngine.Object Logger;

        private static bool AllowLog = false;

        public void OnApplicationQuit()
        {
            if (!LoggerProcess.HasExited)
                LoggerProcess.Kill();
        }

        public static void Log(string output, bool allow = false)
        {
            if (AllowLog && allow)
                LoggerProcess.StandardInput.WriteLine(output);
        }

        private void Awake()
        {
            DontDestroyOnLoad(this);
            Instance = this;

            AppendLoggerProcess();
            LoadGraphicSettings();
            LoadShortcuts();
        }

        private void LoadGraphicSettings()
        {
            Graphics = GetComponent<GraphicSettings>();
            var settingsFilePath = Path.Combine(Application.persistentDataPath, "GraphicSettings.json");

            if (!File.Exists(settingsFilePath))
                return;

            JsonUtility.FromJsonOverwrite(File.ReadAllText(settingsFilePath), Graphics);
            if (!Graphics.CardMotive)
                Graphics.CardMotive = DefaultCardMotive;

            Screen.SetResolution((int)Graphics.FullscreenResolution.x, (int)Graphics.FullscreenResolution.y, Graphics.FullscreenMode);
        }

        private void AppendLoggerProcess()
        {
#if UNITY_EDITOR
            AllowLog = true;
            string filePath = Application.dataPath + AssetDatabase.GetAssetPath(Logger).Replace("Assets", "");
            if (LoggerProcess == default)
                LoggerProcess = Process.Start(new ProcessStartInfo() { FileName = filePath, RedirectStandardInput = true, UseShellExecute = false });
#endif
        }

        private void LoadShortcuts()
        {
            allYaku = AllYaku;
            allCards = AllCards;
            for (int i = 0; i < allCards.Count; i++)
                allCards[i].ID = i;
            prefabCollection = singleton;
        }
    }
}