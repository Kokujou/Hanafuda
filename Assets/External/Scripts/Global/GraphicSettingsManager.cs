using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Hanafuda
{
    [Serializable]
    public class GraphicSettingsManager : MonoBehaviour
    {
        public TextSwitcher ResolutionSwitcher;
        public TextSwitcher AspectRatioSwitcher;
        public TextSwitcher FullscreenModeSwitcher;
        public ToggleButton CardMotiveToggle;

        private static List<float> SupportedAspectRatios;

        private static List<int> SupportedQualitites = new List<int> { 240, 480, 720, 1080 };

        private static List<Texture> SupportedMotives
            => GameObject.Find("GraphicSettings/CardMotive/Motives")
            .GetComponentsInChildren<RawImage>().Select(x => x.texture).ToList();

        private static int _selectedMotive;
        public static Texture SelectedMotive => SupportedMotives[_selectedMotive];

        private const string SettingsFile = "./GraphicSettings.json";
        private static string SettingsFilePath;

        public void Awake()
        {
            SettingsFilePath = Path.Combine(Application.persistentDataPath, SettingsFile);
            if (File.Exists(SettingsFilePath))
                Load();
            else
            {
                Global.Graphics.AspectRatio = (float)Screen.width / Screen.height;
                Global.Graphics.FullscreenResolution = new Vector2(Screen.currentResolution.width, Screen.currentResolution.height);
                Global.Graphics.FullscreenMode = Screen.fullScreenMode;
                Global.Graphics.CardMotive = SupportedMotives[0];
            }


            SetSelectedMotive(CardMotiveToggle);
            SetSelectedFullscreenMode(FullscreenModeSwitcher);
            SetSelectedAspectRatio(AspectRatioSwitcher);
            SetSelectedResolution(ResolutionSwitcher);

            UpdateSupportedResolutions();
        }

        public void OnApplicationQuit()
        {
            Debug.Log($"Quitting Application, Saving Graphic Settings to {Application.persistentDataPath}");
            Save();
        }

        public void SetSelectedAspectRatio(TextSwitcher target)
            => target.Select(target.Values.FindIndex(x => AspectRatioFromString(x) == Global.Graphics.AspectRatio));

        public void SetSelectedFullscreenMode(TextSwitcher target)
            => target.Select(Global.Graphics.FullscreenMode == FullScreenMode.Windowed ? 0 : 1);

        public void SetSelectedResolution(TextSwitcher target)
            => target.Select(target.Values.FindIndex(
                x => Regex.IsMatch(x, $".*{(int)Global.Graphics.FullscreenResolution.x}.*{(int)Global.Graphics.FullscreenResolution.y}.*")));

        public void SetSelectedMotive(ToggleButton target)
        {
            var targetImage = target.Buttons
                .First(x => x.GetComponent<RawImage>().texture.GetInstanceID() == Global.Graphics.CardMotive.GetInstanceID());
            EventSystem.current.SetSelectedGameObject(targetImage.gameObject);
            targetImage.onClick.Invoke();
        }

        public void ApplyFullscreenMode(string mode)
        {
            switch (mode)
            {
                case "Fenster":
                    Global.Graphics.FullscreenMode = FullScreenMode.Windowed;
                    break;
                case "Vollbild":
                    Global.Graphics.FullscreenMode = FullScreenMode.MaximizedWindow;
                    break;
                default:
                    throw new NotImplementedException();
            }
            Screen.SetResolution((int)Global.Graphics.FullscreenResolution.x, (int)Global.Graphics.FullscreenResolution.y, Global.Graphics.FullscreenMode);
        }

        public void ApplyFullscreenResolution(string resolution)
        {
            var selectedQuality = SupportedQualitites.First(x => resolution.Contains($"{ x }"));
            if (Global.Graphics.AspectRatio > 1)
                Global.Graphics.FullscreenResolution = new Vector2((int)(selectedQuality * Global.Graphics.AspectRatio), selectedQuality);
            else
                Global.Graphics.FullscreenResolution = new Vector2(selectedQuality, (int)(selectedQuality / Global.Graphics.AspectRatio));
            Screen.SetResolution((int)Global.Graphics.FullscreenResolution.x, (int)Global.Graphics.FullscreenResolution.y, Global.Graphics.FullscreenMode);
        }

        public void ApplyAspectRatio(string aspect)
        {
            Global.Graphics.AspectRatio = AspectRatioFromString(aspect);
            UpdateSupportedResolutions();
            SetSelectedResolution(ResolutionSwitcher);
        }

        public void ApplyCardMotive(int motiveId)
        {
            _selectedMotive = motiveId;
            Global.Graphics.CardMotive = SupportedMotives[motiveId];
        }

        public void ReturnToMain()
        {
            Destroy(Global.Instance.gameObject);
            SceneManager.LoadScene("Startup");
        }

        public void Load()
        {
            var json = File.ReadAllText(SettingsFilePath);
            JsonUtility.FromJsonOverwrite(json, Global.Graphics);

            Screen.SetResolution((int)Global.Graphics.FullscreenResolution.x, (int)Global.Graphics.FullscreenResolution.y, Global.Graphics.FullscreenMode);
        }

        public void Save()
        {
            var json = JsonUtility.ToJson(Global.Graphics, true);
            File.WriteAllText(SettingsFilePath, json);
        }

        private static float AspectRatioFromString(string ratio)
        {
            var targetRatio = ratio.Trim().Split('-').Select(x => Convert.ToInt32(x));
            return ((float)targetRatio.ElementAt(0) / targetRatio.ElementAt(1));
        }

        private void UpdateSupportedResolutions()
        {
            ResolutionSwitcher.Values =
                (from quality in SupportedQualitites
                 select QualityToResolution(quality, Global.Graphics.AspectRatio)).ToList();
        }

        private string QualityToResolution(int quality, float aspect)
        {
            if (aspect > 1)
                return $"{(int)(quality * aspect)} x {quality}";
            else
                return $"{quality} x {(int)(quality / aspect)}";
        }
    }
}