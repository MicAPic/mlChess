using System.Collections;
using DG.Tweening;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UI
{
    public class MainMenuUI : UserInterface
    {
        [Header("Blur")]
        [SerializeField] 
        private float blurStrength = 0.7f;
        [SerializeField] 
        private CanvasGroup background;
        [SerializeField] 
        private Material uiBlur;
        [FormerlySerializedAs("uiGroup")] [SerializeField] 
        private CanvasGroup uiMainGroup;
        
        [Header("Scene Animation")]
        [SerializeField] 
        private Transform board;
        [SerializeField] 
        private float elementAnimationDuration = 0.33f;
        [SerializeField] 
        private RectTransform buttons;
        [SerializeField]
        private TMP_Text title;
    
        private Resolution _systemResolution;
        private AsyncOperation _sceneLoadOperation;

        private static readonly int BlurRadius = Shader.PropertyToID("_Size");

        void Awake()
        {
            uiBlur.SetFloat(BlurRadius, 2.0f);
            background.alpha = blurStrength;

            board.DORotate(new Vector3(0, 360, 0), 60.0f, RotateMode.FastBeyond360)
                .SetLoops(-1)
                .SetEase(Ease.Linear);
            
            title.color = Color.clear;
            title.DOColor(Color.white, elementAnimationDuration);
            var delay = elementAnimationDuration;

            foreach (RectTransform button in buttons)
            {
                var position = button.anchoredPosition;
                button.anchoredPosition = position - new Vector2(0.0f, 5.0f);
                
                var image = button.GetComponent<Image>();
                image.color = Color.clear;

                button.DOAnchorPos(position, elementAnimationDuration).SetDelay(delay);
                image.DOColor(Color.white, elementAnimationDuration).SetDelay(delay);

                delay += elementAnimationDuration;
            }
        }

        public void GameQuit()
        {
            #if UNITY_EDITOR
            EditorApplication.ExitPlaymode();
            #else
            Application.Quit();
            #endif
        }

        public void ToggleFullscreen()
        {
            if (Screen.fullScreen)
            {
                Screen.fullScreenMode = FullScreenMode.Windowed;
            }
            else
            {
                StartCoroutine(SetFullscreen());
            }
        }

        public void SetToggles(GameObject uiGroup)
        {
            var toggles = uiGroup.GetComponentsInChildren<Toggle>();
            toggles[0].isOn = Screen.fullScreen;
            toggles[1].isOn = SettingsManager.Instance.sfxOn;
            toggles[2].isOn = SettingsManager.Instance.isAntiAliased;
        
            var dropdown = uiGroup.GetComponentInChildren<TMP_Dropdown>();
            dropdown.value = dropdown.options.FindIndex(option => option.text == SettingsManager.Instance.theme);
            dropdown.itemText.text = SettingsManager.Instance.theme;
        }
        
        public void SetSquareMaterials()
        {
            foreach (var square in FindObjectsOfType<Square>())
            {
                square.SetMaterial();
            }
        }

        protected override IEnumerator SceneTransition(string sceneName)
        {
            DOTween.CompleteAll();
            
            _sceneLoadOperation = SceneManager.LoadSceneAsync("Main");
            _sceneLoadOperation.allowSceneActivation = false;
            
            // board.DOKill();
            board.DORotate(Vector3.zero, 1.0f);
        
            uiBlur.DOFloat(0.0f, BlurRadius, 1.0f);
            uiMainGroup.DOFade(0.0f, 1.0f);
            background.DOFade(0.0f, 1.0f)
                .OnComplete(() => _sceneLoadOperation.allowSceneActivation = true);
            yield return null;
        }

        IEnumerator SetFullscreen()
        {
            // remember current resolution
            _systemResolution = Screen.currentResolution;
            Screen.SetResolution(_systemResolution.width, _systemResolution.height, true);
        
            yield return new WaitForEndOfFrame(); // wait for a frame to prevent bugs
            Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
        }
    }
}
