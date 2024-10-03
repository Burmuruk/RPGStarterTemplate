using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static Unity.VisualScripting.Icons;

namespace Burmuruk.Tesis.Control
{
    public class SettingsController : MonoBehaviour
    {
        [SerializeField] GameObject warninigWindow;
        [SerializeField] Button btnEnemiesHealth;
        [SerializeField] Button btnLanguage;
        [SerializeField] Button btnContent;
        [SerializeField] Button btnResolution;
        [SerializeField] Button btnVsync;
        [SerializeField] Button btnFPSLimit;
        [SerializeField] Slider sldVolume;
        [SerializeField] GameObject[] extraOptions;

        [SerializeField] Button btnApply;
        [SerializeField] Button btnCancel;

        FullScreenMode fullScreenMode;
        int frameRate = 0;
        int vSync = 0;
        Resolution resolution;
        int language;
        float volume = 0;
        int? activeOption;

        Dictionary<string, Action> changes = new();

        enum GameLanguage
        {
            None,
            Spanish,
            English
        }

        private void Start()
        {
            UpdateCurrentValues();
            //LoadSettings();
        }

        public void ShowMenu()
        {
            gameObject.SetActive(!gameObject.activeSelf);
        }

        public void ShowExtraOptions(int idx)
        {
            extraOptions[idx].SetActive(true);

            if (activeOption.HasValue)
                extraOptions[activeOption.Value].SetActive(false);

            activeOption = idx;
        }

        public void Setlanguage(int idx)
        {
            GameLanguage language = idx switch
            {
                2 => GameLanguage.Spanish,
                _ => GameLanguage.English,
            };

            btnLanguage.GetComponentInChildren<Text>().text = language.ToString();
        }

        public void SetWindowMode(int idx)
        {
            FullScreenMode mode = idx switch
            {
                0 => FullScreenMode.FullScreenWindow,
                1 => FullScreenMode.Windowed,
                _ => FullScreenMode.MaximizedWindow,
            };

            if (mode == fullScreenMode)
            {
                if (changes.ContainsKey("FullscreenMode"))
                    changes.Remove("FullscreenMode");
            }
            else
            {
                changes["FullscreenMode"] = () => Screen.fullScreenMode = mode;
            }

            btnLanguage.GetComponentInChildren<Text>().text = mode.ToString();
        }

        public void LimitFPS(int newFrameRate)
        {
            if (newFrameRate == frameRate)
            {
                if (changes.ContainsKey("FrameRate"))
                    changes.Remove("FrameRate");
            }
            else
            {
                changes["FrameRate"] = () => Application.targetFrameRate = newFrameRate;
            }

            btnLanguage.GetComponentInChildren<Text>().text = newFrameRate.ToString();
        }

        public void EnableVSync()
        {
            int newVsync = 0;

            if (QualitySettings.vSyncCount > 0)
            {
                btnFPSLimit.enabled = true;
                newVsync = 60;
                btnLanguage.GetComponentInChildren<Text>().text = "On";
            }
            else
            {
                newVsync = -1;
                btnFPSLimit.enabled = false;
                btnLanguage.GetComponentInChildren<Text>().text = "Of";
            }

            if (QualitySettings.vSyncCount == vSync)
            {
                if (changes.ContainsKey("VSync"))
                    changes.Remove("VSync");
            }
            else
            {
                changes["VSync"] = () => QualitySettings.vSyncCount = newVsync;
            }
        }

        public void SetResolution(Vector2Int resolution)
        {
            if (resolution.x == this.resolution.width && resolution.y == this.resolution.height)
            {
                if (changes.ContainsKey("Resolution"))
                    changes.Remove("Resolution");
            }
            else
            {
                changes["Resolution"] = () => Screen.SetResolution(resolution.x, resolution.y, fullScreenMode);
            }

            btnLanguage.GetComponentInChildren<Text>().text = resolution.x + " x " + resolution.y;
        }

        public void ApplyChanges()
        {
            if (changes == null) return;

            foreach (var change in changes)
            {
                change.Value?.Invoke();
            }

            SaveSettings();
            UpdateCurrentValues();
        }

        public void Cancel()
        {
            if (changes.Count <= 0) return;

            changes.Clear();
        }

        private void UpdateCurrentValues()
        {
            fullScreenMode = Screen.fullScreenMode;
            frameRate = Application.targetFrameRate;
            vSync = QualitySettings.vSyncCount;
            resolution = Screen.currentResolution;
        }

        private void SaveSettings()
        {
            PlayerPrefs.SetInt("PlayersHealth", 0);
            PlayerPrefs.SetInt("Language", language);
            PlayerPrefs.SetInt("WindowMode", (int)fullScreenMode);
            PlayerPrefs.SetInt("VSync", vSync);
            PlayerPrefs.SetInt("LimitFPS", frameRate);
            PlayerPrefs.SetFloat("MasterVolume", volume);
            PlayerPrefs.SetString("Resolution", "");
        }

        private void LoadSettings()
        {
            string[] resolution = PlayerPrefs.GetString("Resolution").Split('x');
            //Screen.SetResolution(resolution.x, resolution.y, fullScreenMode);
            QualitySettings.vSyncCount = PlayerPrefs.GetInt("VSync");
            Screen.fullScreenMode = (FullScreenMode)PlayerPrefs.GetInt("WindowMode");
            Application.targetFrameRate = PlayerPrefs.GetInt("LimitFPS");
        }
    }
}
