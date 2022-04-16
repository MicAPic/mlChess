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
    public GameObject endgamePopup;
    
    [SerializeField]
    private TMP_Dropdown pieceDropdown;
    private Dictionary<Piece.PieceColour, List<char>> _takenPiecesLists = new Dictionary<Piece.PieceColour, List<char>>
    {
        // Set of pieces taken
        {Piece.PieceColour.white, new List<char>()},
        {Piece.PieceColour.black, new List<char>()}
    };

    public void SceneLoad(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void GameQuit()
    {
        #if UNITY_EDITOR
            EditorApplication.ExitPlaymode();
        #else
            Application.Quit;
        #endif
    } 
    
    public void ToggleSubmenu(GameObject submenu)
    {
        submenu.SetActive(!submenu.activeInHierarchy);
    }

    public IEnumerator ShowEndgameScreen(string titleText, string statusText)
    {
        yield return new WaitForSeconds(1.5f);
        endgamePopup.SetActive(true);
        endgamePopup.GetComponentsInChildren<TMP_Text>()[0].text = titleText;
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
