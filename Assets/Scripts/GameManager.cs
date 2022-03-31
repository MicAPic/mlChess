using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Pieces;
using UnityEngine;
using UnityEngine.Serialization;
using Debug = UnityEngine.Debug;

public class GameManager : MonoBehaviour
{
    [Header("Utility")]
    public UI ui;
    public List<GameObject> squareList;
    public string promoteTo = "Queen";

    private Piece _selectedPiece;
    [SerializeField]
    private Camera activeCamera;
    [SerializeField] 
    private GameObject promotionPopup;
    
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

    // this code handles promotion of pawns  
    private Pawn _pawnToPromote;
    private GameObject _promotionLocation;
    
    public void AskForPromotion(Pawn pawn, GameObject square)
    {
        ui.ToggleSubmenu(promotionPopup);
        _pawnToPromote = pawn;
        _promotionLocation = square;
    }
    
    public void PromotePawn()
    {
        var promotedPieceName = _pawnToPromote.pieceColour.ToString()[0] + promoteTo;
        Instantiate(Resources.Load("Prefabs/" + promotedPieceName), 
                    _promotionLocation.transform);
        Destroy(_pawnToPromote.gameObject);
    }
    //

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

            if (clickedObject.CompareTag("Square") && _selectedPiece)
            {
                _selectedPiece.MakeMove(clickedObject.gameObject);
                _selectedPiece = null;
                Debug.Log(clickedObject.name);
            }
            else if (clickedObject.parent.CompareTag("Piece"))
            {
                if (_selectedPiece)
                {
                    var square = clickedObject.parent.parent;
                    _selectedPiece.MakeMove(square.gameObject);
                    _selectedPiece = null;
                    Debug.Log(square.name);
                    return;
                }
                
                var piece = clickedObject.parent.GetComponent<Piece>();
                _selectedPiece = piece;
                Debug.Log(piece.name);
            }
        }
    }
}
