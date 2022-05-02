using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class UI : MonoBehaviour
{
    public Dictionary<Piece.PieceColour, TMP_Text> TakenPiecesListsUI;
    public TMP_Text statusBarText;
    public GameObject pauseScreen;
    
    [SerializeField]
    private TMP_Dropdown pieceDropdown;
    private Dictionary<Piece.PieceColour, List<char>> _takenPiecesLists = new Dictionary<Piece.PieceColour, List<char>>
    {
        // Set of pieces taken
        {Piece.PieceColour.white, new List<char>()},
        {Piece.PieceColour.black, new List<char>()}
    };

    private Resolution _systemResolution;

    void Start()
    {
        Application.targetFrameRate = 30; // prevent the game from targeting billion fps 
    }

    public void SceneLoad(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
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
    
    IEnumerator SetFullscreen()
    {
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
        pauseScreen.GetComponentsInChildren<TMP_Text>()[0].text = titleText;
        pauseScreen.GetComponentsInChildren<TMP_Text>()[1].text = "New Game"; // instead of "resign"
        statusBarText.text = statusText;
    }

    public void ChangePieceForPromoting()
    {
        var gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
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
}
