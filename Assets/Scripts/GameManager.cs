using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class GameManager : MonoBehaviour
{
    public List<GameObject> squareList;

    private Piece _selectedPiece;
    [SerializeField]
    private Camera activeCamera;
    
    void Awake()
    {
        GenerateBoard();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            HandleSelection();
        }
    }

    void GenerateBoard()
    {
        // makes a 10x12 board to account for the illegal moves
        squareList = new List<GameObject>();
        
        squareList.AddRange(Enumerable.Repeat<GameObject>(null, 21)); //adds a bottom border

        int counter = 0;
        GameObject chessBoard = GameObject.Find("Chess Board");
        foreach (Transform child in chessBoard.transform)
        {
            if (child.CompareTag("Square"))
            {
                squareList.Add(child.gameObject);
            }
            
            counter++;
            if (counter % 8 == 0)
            {
                squareList.AddRange(Enumerable.Repeat<GameObject>(null, 2));
            }
        }
        
        squareList.AddRange(Enumerable.Repeat<GameObject>(null, 19)); //adds a top border
    }
    
    void HandleSelection()
    {
        var ray = activeCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            var clickedObject = hit.transform;
            
            if (clickedObject.parent.CompareTag("Piece")){
                var piece = clickedObject.parent.GetComponent<Piece>();
                _selectedPiece = piece;
                Debug.Log(piece.name);
            }
            else if (clickedObject.CompareTag("Square") && _selectedPiece != null)
            {
                Debug.Log(clickedObject.name);
                _selectedPiece.MakeMove(clickedObject.gameObject);
                _selectedPiece = null;
            }
        }
    }
}