using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Pieces;
using Pieces.SteppingPieces;
using UnityEngine;
using UnityEngine.Assertions.Must;
using Debug = UnityEngine.Debug;

public class GameManager : MonoBehaviour
{
    [Header("Kings")] 
    [SerializeField] 
    private King whiteKing;
    [SerializeField] 
    private King blackKing;
    
    [Header("Game Rules & Logic")]
    public Piece.PieceColour turnOf = Piece.PieceColour.white;
    public string promoteTo = "Queen";
    [SerializeField]
    private GameObject promotionPiecePool;
    
    [Header("Utility")]
    public List<GameObject> squareList;
    public List<Piece> whitePieces;
    public List<Piece> blackPieces;

    public UI ui;
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
    private bool _isFirstFrame = true;
    void Update()
    {
        if (_isFirstFrame) // update on the first frame (after awake & start)
        {
            UpdateMoves();
            _isFirstFrame = false;
        }
        
        if (Input.GetMouseButtonDown(0))
        {
            HandleSelection();
        }
    }

    public void ChangeTurn()
    {
        UpdateSquares();
        UpdateMoves();

        whiteKing.RemoveIllegalMoves(); 
        blackKing.RemoveIllegalMoves();
        
        turnOf = Piece.Next(turnOf);
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
        // don't use resources.load; rewrite later
        var promotedPieceName = _pawnToPromote.pieceColour.ToString()[0] + promoteTo;
        var instance = Instantiate(promotionPiecePool.transform.Find(promotedPieceName),
                                            _promotionLocation.transform);
        var piece = instance.GetComponent<Piece>();
        // this is required because the new piece can give check
        piece.Start();
        piece.GenerateMoves();
        //
        Destroy(_pawnToPromote.gameObject);
        
        if (_pawnToPromote.pieceColour == Piece.PieceColour.black)
        {
            blackPieces.Remove(_pawnToPromote);
            blackPieces.Add(piece);
        }
        else
        {
            whitePieces.Remove(_pawnToPromote);
            whitePieces.Add(piece);
        }
    }
    //
    
    public void UpdateMoves()
    {
        // updates list of moves for each piece on the board
        foreach (var piece in whitePieces.Union(blackPieces))
        {
            piece.GenerateMoves();
        }
    }

    void UpdateSquares()
    {
        // resets the properties of board squares
        foreach (var squareGameObject in squareList)
        {
            if (squareGameObject == null) continue;
            var square = squareGameObject.GetComponent<Square>();
            square.AttackedBy[Piece.PieceColour.white] = false;
            square.AttackedBy[Piece.PieceColour.black] = false;
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
            squareList.Add(child.gameObject);
            if (child.childCount != 0)
            {
                var piece = child.GetChild(0).GetComponent<Piece>(); // child[0] is always a piece 
                if (piece.pieceColour == Piece.PieceColour.white)
                {
                    whitePieces.Add(piece);
                }
                else
                {
                    blackPieces.Add(piece);
                }
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
                _selectedPiece.StartTurn(clickedObject.gameObject);
                _selectedPiece = null;
                Debug.Log(clickedObject.name);
            }
            else if (clickedObject.parent.CompareTag("Piece"))
            {
                // the next nasty if clause allows easy piece changing mid-move, if the player needs so 
                if (_selectedPiece && 
                    _selectedPiece.pieceColour != clickedObject.parent.GetComponent<Piece>().pieceColour) 
                {
                    var square = clickedObject.parent.parent;
                    _selectedPiece.StartTurn(square.gameObject);
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
