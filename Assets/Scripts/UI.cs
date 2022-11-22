using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class UI : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField]
    private Animator transition;
    
    [Header("In-Game Stuff")]
    public TMP_Text statusBarText;
    public GameObject pauseScreen;
    public GameObject playIcon;
    public Dictionary<Piece.PieceColour, TMP_Text> TakenPiecesListsUI;

    [SerializeField]
    private Slider evaluationSlider;
    [SerializeField]
    private TMP_Dropdown pieceDropdown;
    private Dictionary<Piece.PieceColour, List<char>> _takenPiecesLists = new Dictionary<Piece.PieceColour, List<char>>
    {
        // Set of pieces taken
        {Piece.PieceColour.white, new List<char>()},
        {Piece.PieceColour.black, new List<char>()}
    };
    
    // private MaterialPool _materialPool;
    private Resolution _systemResolution;

    void Start()
    {
        Application.targetFrameRate = 30; // prevent the game from targeting billion fps 
    }
    
    [Serializable] 
    class SettingsData
    {
        public string theme;
        public bool antiAliasing;
    }

    public void SaveSettings(GameObject uiGroup)
    {
        var themeDropdown = uiGroup.GetComponentInChildren<TMP_Dropdown>();
        var antiAliasingToggle = uiGroup.transform.Find("Toggle (FXAA)").GetComponent<Toggle>();
        
        SettingsData data = new SettingsData
        {
            theme = themeDropdown.options[themeDropdown.value].text,
            antiAliasing = antiAliasingToggle.isOn 
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
            SettingsManager.Instance.isAntiAliased = data.antiAliasing;
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

    public void GameQuit()
    {
        #if UNITY_EDITOR
        EditorApplication.ExitPlaymode();
        #else
        Application.Quit();
        #endif
    }

    public void ToggleSubmenu(GameObject submenu)
    {
        submenu.SetActive(!submenu.activeInHierarchy);
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
        toggles[1].isOn = Convert.ToBoolean(AudioListener.volume);
        toggles[2].isOn = SettingsManager.Instance.isAntiAliased;
        
        var dropdown = uiGroup.GetComponentInChildren<TMP_Dropdown>();
        dropdown.value = dropdown.options.FindIndex(option => option.text == SettingsManager.Instance.theme);
        dropdown.itemText.text = SettingsManager.Instance.theme;
    }

    IEnumerator SceneTransition(string sceneName)
    {
        transition.SetTrigger("Transition");
        yield return new WaitForSeconds(1);
        SceneManager.LoadScene(sceneName);
    }
    
    IEnumerator SetFullscreen()
    {
        // remember current resolution
        _systemResolution = Screen.currentResolution;
        Screen.SetResolution(_systemResolution.width, _systemResolution.height, true);
        
        yield return new WaitForEndOfFrame(); // wait for a frame to prevent bugs
        Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
    }

    public void ToggleSFX()
    {
        AudioListener.volume = 1 - AudioListener.volume; // mutes, if the audio is on; unmutes otherwise
    }

    public void PlaySFX(GameObject sfx)
    {
        var instance = Instantiate(sfx);
        StartCoroutine(DestroySFX(instance, 1));
    }

    IEnumerator DestroySFX(GameObject instance, float length)
    {
        yield return new WaitForSeconds(length);
        Destroy(instance);
    }

    public IEnumerator ShowEndgameScreen(string titleText, string statusText)
    {
        yield return new WaitForSeconds(1.5f);
        pauseScreen.SetActive(true);
        playIcon.SetActive(true);
        pauseScreen.GetComponentsInChildren<TMP_Text>()[0].text = titleText;
        pauseScreen.GetComponentsInChildren<TMP_Text>()[1].text = "New Game"; // instead of "resign"
        statusBarText.text = statusText;
    }

    public void ChangePieceForPromoting()
    {
        var gameManager = FindObjectOfType<GameManager>();
        int index = pieceDropdown.value;
        string newPieceForPromoting = pieceDropdown.options[index].text.Substring(1); // get the substring that doesn't include the piece symbol
        
        gameManager.promoteTo = newPieceForPromoting;
    }

    public void UpdateTakenPiecesList(Piece.PieceColour thisPieceColour, char takenPieceIcon)
    {
        // adds the taken piece to the taken piece list on the side of the screen
        _takenPiecesLists[thisPieceColour].Add(takenPieceIcon);
        _takenPiecesLists[thisPieceColour].Sort();
        TakenPiecesListsUI[thisPieceColour].text = string.Join("", _takenPiecesLists[thisPieceColour]);
    }

    private bool _canAnimate = true;
    public IEnumerator WaitUntilCanAnimate(float targetValue)
    {
        yield return new WaitUntil(() => _canAnimate);
        StartCoroutine(LarpSlider(targetValue));
    }

    IEnumerator LarpSlider(float targetValue)
    {
        _canAnimate = false;
        
        float timeElapsed = 0;
        while (timeElapsed < 1)
        {
            evaluationSlider.value = Mathf.Lerp(evaluationSlider.value, targetValue, timeElapsed / 1);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        evaluationSlider.value = targetValue;

        _canAnimate = true;
    }
}
