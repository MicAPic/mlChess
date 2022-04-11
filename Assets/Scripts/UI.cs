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
    public GameObject statusBar;
    public GameObject endgamePopup;
    
    [SerializeField]
    private TMP_Dropdown pieceDropdown;

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

    public IEnumerator ShowEndgameScreen(string text)
    {
        yield return new WaitForSeconds(1.5f);
        endgamePopup.SetActive(true);
        endgamePopup.GetComponentsInChildren<TMP_Text>()[0].text = text;
    }

    public void ChangePieceForPromoting()
    {
        var gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        int index = pieceDropdown.value;
        string newPieceForPromoting = pieceDropdown.options[index].text.Substring(1); // get the substring that doesn't include the piece symbol
        
        gameManager.promoteTo = newPieceForPromoting;
    }
}
