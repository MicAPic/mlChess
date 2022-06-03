using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Pieces;
using Pieces.SlidingPieces;
using Pieces.SteppingPieces;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Game Rules & Logic")]
    public Piece.PieceColour turnOf = Piece.PieceColour.white;
    public int halfmoveClock;
    public string promoteTo = "Queen";
    [SerializeField]
    private GameObject promotionPiecePool;

    [Header("Utility")]
    public bool isPaused;
    
    public string currentPosition;
    public List<string> positionHistory;
    
    public List<GameObject> squareList;
    public List<Piece> whitePieces;
    public List<Piece> blackPieces;
    public Dictionary<Piece.PieceColour, Dictionary<string, int>> PiecesByType;
    public Dictionary<Piece.PieceColour, King> Kings;
    public Dictionary<Piece.PieceColour, int> MoveCount; // amount of moves available for each side

    public UI ui;
    [SerializeField]
    internal Camera activeCamera;
    [SerializeField] 
    private GameObject promotionPopup;

    [Header("SFX")] 
    public List<GameObject> sfx;

    [Header("Screenshot Management")]
    public string screenshotKey = "s";
    public int imageScale = 1;
    [SerializeField]
    private int imageCount = 0;
    // The key used to get/set the number of images
    private const string ImageCntKey = "IMAGE_CNT";

    private readonly Dictionary<Piece.PieceColour, string> _winText = new Dictionary<Piece.PieceColour, string>
    {
        // LAN; text that appears when the opposite side wins 
        {Piece.PieceColour.white, "0–1"},
        {Piece.PieceColour.black, "1–0"}
    };
    

    void Awake()
    {
        GenerateBoard();
        SettingsManager.Instance.SwitchTheme();
        SettingsManager.Instance.ToggleAntiAliasing(activeCamera);
        
        imageCount = PlayerPrefs.GetInt(ImageCntKey);

        MoveCount = new Dictionary<Piece.PieceColour, int>
        {
            {Piece.PieceColour.white, 0},
            {Piece.PieceColour.black, 0}
        };

        PiecesByType = new Dictionary<Piece.PieceColour, Dictionary<string, int>>
        {
            [Piece.PieceColour.white] = new Dictionary<string, int>
            {
                {" ♙", 8},
                {"♖", 2},
                {"♘", 2},
                {"♗", 2},
                {"♕", 1}
            },
            [Piece.PieceColour.black] = new Dictionary<string, int>
            {
                {" ♟", 8},
                {"♜", 2},
                {"♞", 2},
                {"♝", 2},
                {"♛", 1}
            }
        };

        Kings = new Dictionary<Piece.PieceColour, King>
        {
            {Piece.PieceColour.white, GameObject.Find("wKing").GetComponent<King>()},
            {Piece.PieceColour.black, GameObject.Find("bKing").GetComponent<King>()}
        };

        ui.TakenPiecesListsUI = new Dictionary<Piece.PieceColour, TMP_Text>
        {
            {Piece.PieceColour.white, GameObject.Find("Pieces taken by White").GetComponent<TMP_Text>()},
            {Piece.PieceColour.black, GameObject.Find("Pieces taken by Black").GetComponent<TMP_Text>()}
        };
    }

    // Update is called once per frame
    private bool _isFirstFrame = true;
    void Update()
    {
        if (_isFirstFrame) // update on the first frame (after awake & start)
        {
            UpdateMoves(true);
            _isFirstFrame = false;
        }
        
        if (Input.GetMouseButtonDown(0))
        {
            HandleSelection();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            isPaused = !isPaused;
            ui.ToggleSubmenu(ui.pauseScreen);
        }
        
        if (Input.GetKeyDown(screenshotKey.ToLower()))

            // But, we will use the new Unity Input System to check for input on the Keyboard
            // if (Keyboard.current.FindKeyOnCurrentKeyboardLayout(m_ScreenshotKey).wasPressedThisFrame)
        {
            TakeScreenshot();
        }
    }

    public void ChangeTurn()
    {
        ui.PlaySFX(sfx[0]);
        PositionEvaluation();
        UpdateSquares();
        UpdateMoves(true);
        UpdateMoves(false);
        var isInCheck = Kings[Piece.Next(turnOf)].checkingEnemies.Count > 0;

        foreach (var king in Kings.Values)
        {
            king.RemoveIllegalMoves();
        }
        
        turnOf = Piece.Next(turnOf);
        
        if (Kings[turnOf].checkingEnemies.Count > 0)
        {
            if (isInCheck)
            {
                ui.statusBarText.text += new string('+', Kings[turnOf].checkingEnemies.Count); // LAN
            }
        }
        
        if (MoveCount[turnOf] == 0)
        {
            // declare checkmate or stalemate
            isPaused = true;
            ui.PlaySFX(sfx[2]);
            if (isInCheck)
            {
                var statusText = ui.statusBarText.text;
                // LAN; substitute "+" (check sign) for "#" (checkmate sign)
                ui.statusBarText.text = statusText.Remove(statusText.Length - 1, 1) + "#";
                
                // set the eval slider to reflect the checkmate
                if (turnOf == Piece.PieceColour.black)
                {
                    StartCoroutine(ui.WaitUntilCanAnimate(1));
                }
                else
                {
                    StartCoroutine(ui.WaitUntilCanAnimate(0));
                }
                
                StartCoroutine(ui.ShowEndgameScreen("Checkmate", _winText[turnOf]));
            }
            else
            {
                StartCoroutine(ui.WaitUntilCanAnimate(0.5f));
                StartCoroutine(ui.ShowEndgameScreen("Stalemate", "½–½"));
            }
        }
        else if (halfmoveClock >= 100 || ThreefoldRepetitionCheck())
        {
            // declare a draw 
            isPaused = true;
            ui.PlaySFX(sfx[2]);
            StartCoroutine(ui.ShowEndgameScreen("Draw", "½–½"));
        }
    }

    // the following code handles promotion of pawns  
    private Pawn _pawnToPromote;
    private GameObject _promotionLocation;
    
    public void StartPromotion(Pawn pawn, GameObject square) // used in scripts
    {
        isPaused = true;
        ui.ToggleSubmenu(promotionPopup);
        _pawnToPromote = pawn;
        _promotionLocation = square;
    }
    
    public void PromotePawn() // used in editor
    {
        var promotedPieceName = _pawnToPromote.pieceColour.ToString()[0] + promoteTo;
        var instance = Instantiate(promotionPiecePool.transform.Find(promotedPieceName),
                                            _promotionLocation.transform);
        var piece = instance.GetComponent<Piece>();
        // this is required because the new piece can give check
        piece.Start();
        piece.GenerateMoves();
        UpdateMoves(false);
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
        PiecesByType[_pawnToPromote.pieceColour][_pawnToPromote.pieceIcon]--;
        PiecesByType[piece.pieceColour][piece.pieceIcon]++;

        ui.statusBarText.text += piece.pieceIcon;
        isPaused = false;
    }
    //
    
    public void UpdateMoves(bool isRecordingPositions)
    {
        // updates list of moves for each piece on the board
        MoveCount[Piece.PieceColour.white] = 0;
        MoveCount[Piece.PieceColour.black] = 0;

        currentPosition = "";
        
        foreach (var piece in whitePieces.Union(blackPieces))
        {
            piece.GenerateMoves();
            if (isRecordingPositions)
            {
                var temp = piece.transform.position;
                currentPosition += $"({Math.Round(temp.x)}, {Math.Round(temp.z)})";
            }
        }
        
        if (isRecordingPositions)
        {
            // record piece positions
            positionHistory.Add(currentPosition);
        }
    }
    
    public void ObviousDrawCheck()
    {
        // awful if-else nightmare that checks for insufficient material draws; executed rarely
        
        bool isDraw = false;
        
        if (blackPieces.Count < 3 && whitePieces.Count < 3)
        {
            if (whitePieces.Count == blackPieces.Count)
            {
                if (whitePieces.Count == 1)
                {
                    // bKing & wKing
                    isDraw = true;
                }

                var bBishop = blackPieces.Find(piece => piece.GetComponent<Bishop>());
                var wBishop = whitePieces.Find(piece => piece.GetComponent<Bishop>());
                if (bBishop != null && wBishop != null && 
                    bBishop.GetComponentInParent<Square>().squareColour == wBishop.GetComponentInParent<Square>().squareColour)
                {
                    // bKing & wKing & Bishops on the squares of the same colour on both sides 
                    isDraw = true;
                }
            }
            else if (whitePieces.Count > blackPieces.Count)
            {
                var wKnight = whitePieces.Find(piece => piece.GetComponent<Knight>());
                var wBishop = whitePieces.Find(piece => piece.GetComponent<Bishop>());

                if (wKnight != null || wBishop != null)
                {
                    // wKing & wKnight | wBishop 
                    isDraw = true;
                }
            }
            else
            {
                var bKnight = blackPieces.Find(piece => piece.GetComponent<Knight>());
                var bBishop = blackPieces.Find(piece => piece.GetComponent<Bishop>());

                if (bKnight != null || bBishop != null)
                {
                    // bKing & bKnight | bBishop
                    isDraw = true;
                }
            }
        }

        if (isDraw)
        {
            // declare a draw 
            isPaused = true;
            StartCoroutine(ui.ShowEndgameScreen("Draw", "½–½"));
        }
    }
    
    public IEnumerator DelayCastlingSFX(float delay, int sfxIndex)
    {
        yield return new WaitForSeconds(delay);
        ui.PlaySFX(sfx[sfxIndex]);
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

        var counter = 0;
        var chessBoard = GameObject.Find("Chess Board");
        foreach (Transform child in chessBoard.transform)
        {
            squareList.Add(child.gameObject);
            if (child.childCount != 0)
            {
                var piece = child.GetComponentInChildren<Piece>(); 
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
    
    // this block handles piece selection
    public readonly Color ToggleColour = new Color(0.62f, 0.44f, 0.36f); // brown
    public readonly Color AttackedColour = new Color(0.8f, 0.3f, 0.24f); // red-ish
    
    private Piece _selectedPiece;
    private Collider _selectedPieceCollider;
    
    void HandleSelection()
    {
        var ray = activeCamera.ScreenPointToRay(Input.mousePosition);
        
        if (Physics.Raycast(ray, out var hit) && !isPaused)
        {
            var clickedObject = hit.transform;

            if (clickedObject.CompareTag("Square") && _selectedPiece)
            {
                _selectedPiece.ToggleSelection(); // off
                _selectedPiece.StartTurn(clickedObject.gameObject);
                _selectedPiece = null;
                _selectedPieceCollider.enabled = true;
                Debug.Log(clickedObject.name);
            }
            else if (clickedObject.parent.CompareTag("Piece"))
            {
                // the next nasty if clause allows easy piece changing mid-move, if the player needs so 
                if (_selectedPiece && 
                    _selectedPiece.pieceColour != clickedObject.parent.GetComponent<Piece>().pieceColour) 
                {
                    var square = clickedObject.parent.parent;
                    _selectedPiece.ToggleSelection(); // off
                    _selectedPiece.StartTurn(square.gameObject);
                    _selectedPiece = null;
                    _selectedPieceCollider.enabled = true;
                    Debug.Log(square.name);
                    return;
                }
                
                var piece = clickedObject.GetComponentInParent<Piece>();
                if (piece.pieceColour == turnOf)
                {
                    if (_selectedPiece != null)
                    {
                        // change the toggled piece
                        _selectedPiece.ToggleSelection();
                        _selectedPieceCollider.enabled = true;
                    }
                    _selectedPiece = piece;
                    _selectedPieceCollider = clickedObject.GetComponent<Collider>();
                    _selectedPieceCollider.enabled = false;
                    _selectedPiece.ToggleSelection(); // on
                    Debug.Log(piece.name + piece.transform.position);
                }
            }
        }
    }
    //

    bool ThreefoldRepetitionCheck()
    {
        var repeatedRecords = new Dictionary<string, int>();

        foreach (var record in positionHistory)
        {
            if (!repeatedRecords.ContainsKey(record))
            {
                repeatedRecords[record] = 1;
            }
            else
            {
                repeatedRecords[record]++;
            }
        }

        return repeatedRecords.ContainsValue(3);
    }

    void PositionEvaluation()
    {
        var evaluation = 9 * (PiecesByType[Piece.PieceColour.white]["♕"] - 
                              PiecesByType[Piece.PieceColour.black]["♛"]) + 
                         5 * (PiecesByType[Piece.PieceColour.white]["♖"] - 
                              PiecesByType[Piece.PieceColour.black]["♜"]) + 
                         3 * (PiecesByType[Piece.PieceColour.white]["♗"] - 
                             PiecesByType[Piece.PieceColour.black]["♝"] + 
                             PiecesByType[Piece.PieceColour.white]["♘"] - 
                             PiecesByType[Piece.PieceColour.black]["♞"]) + 
                         (PiecesByType[Piece.PieceColour.white][" ♙"] - PiecesByType[Piece.PieceColour.black][" ♟"]) + 
                         0.1f * (MoveCount[Piece.PieceColour.white] - 
                                 MoveCount[Piece.PieceColour.black]);
        
        evaluation = (evaluation / 60 + 1) / 2; // mapping from [-60; 60] to [0; 1]
        StartCoroutine(ui.WaitUntilCanAnimate(evaluation));
    }

    void TakeScreenshot()
    {
        // Saves the current image count
        PlayerPrefs.SetInt(ImageCntKey, ++imageCount);

        // Adjusts the height and width for the file name
        int width = Screen.width * imageScale;
        int height = Screen.height * imageScale;

        // Determine the pathname/filename for the file
        string pathname = "Screenshots/Screenshot_";

        pathname += width + "x" + height + "_" + imageCount + ".png";

        // Take the actual screenshot and save it
        ScreenCapture.CaptureScreenshot(pathname, imageScale);
        Debug.Log("Screenshot captured at " + pathname);
    }
}
