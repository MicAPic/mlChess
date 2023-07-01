using System;
using System.Collections;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public abstract class UserInterface : MonoBehaviour
    {
        private Resolution _systemResolution;

        void Start()
        {
            Application.targetFrameRate = 30; // prevent the game from targeting billion fps 
        }
    
        [Serializable] 
        protected class SettingsData
        {
            public string theme;
            public bool antiAliasing;
            public bool sfxOn;
        }

        public void SaveSettings(GameObject uiGroup)
        {
            var themeDropdown = uiGroup.GetComponentInChildren<TMP_Dropdown>();
            var antiAliasingToggle = uiGroup.transform.Find("Toggle (FXAA)").GetComponent<Toggle>();
            var sfxToggle = uiGroup.transform.Find("Toggle (SFX)").GetComponent<Toggle>();
        
            SettingsData data = new SettingsData
            {
                theme = themeDropdown.options[themeDropdown.value].text,
                antiAliasing = antiAliasingToggle.isOn, 
                sfxOn = sfxToggle.isOn
            };

            string json = JsonUtility.ToJson(data);
            File.WriteAllText(Application.persistentDataPath + "/settings.json", json);
            LoadSettings();
        }

        public void LoadSettings()
        {
            var serializationFileName = Application.persistentDataPath + "/settings.json";
            if (File.Exists(serializationFileName))
            {
                string json = File.ReadAllText(serializationFileName);
                SettingsData data = JsonUtility.FromJson<SettingsData>(json);

                SettingsManager.Instance.theme = data.theme;
                SettingsManager.Instance.SwitchTheme();
                SettingsManager.Instance.isAntiAliased = data.antiAliasing;
                SettingsManager.Instance.sfxOn = data.sfxOn;
            }
            else
            {
                Debug.Log("Loading failed");
                SettingsManager.Instance.theme = "Classic";
            }
        }

        public void SceneLoad(string sceneName)
        {
            StartCoroutine(SceneTransition(sceneName));
        }

        public void ToggleSubmenu(GameObject submenu)
        {
            submenu.SetActive(!submenu.activeInHierarchy);
        }

        public void PlaySFX(GameObject sfx)
        {
            var instance = Instantiate(sfx);
            StartCoroutine(DestroySFX(instance, 1));
        }

        protected abstract IEnumerator SceneTransition(string sceneName);

        IEnumerator DestroySFX(GameObject instance, float length)
        {
            yield return new WaitForSeconds(length);
            Destroy(instance);
        }
    }
}
