using System;
using System.Collections;
using System.Collections.Generic;
using Pieces.SteppingPieces;
using UnityEngine;

public abstract class Piece : MonoBehaviour
{
    // handle the colour of pieces
    public enum PieceColour
    {
        black,
        white
    }

    public static PieceColour Next(PieceColour pieceColour)
    {
        switch (pieceColour)
        {
            case PieceColour.white:
                return PieceColour.black;
            case PieceColour.black:
                return PieceColour.white;
            default:
                return 0;
        }
    }
    //
    [Header("Attributes")]
    public PieceColour pieceColour;
    [SerializeField] 
    public string pieceIcon;

    [Header("Utility")]
    public List<int> pinDirections;
    protected int[] Pattern;
    protected int PreviousPos;
    protected int CurrentPos;
    protected internal bool HasMoved;
    protected internal bool IsGivingCheck;
    protected GameManager GameManager { get; private set; }
    public List<GameObject> possibleDestinations;

    [Header("Kings")]
    protected King HisMajesty;
    protected King PeskyEnemyKing;

    [Header("Selection")] 
    public Outline outline;
    public Renderer pieceRenderer;

    void Awake()
    {
        pieceRenderer = GetComponentInChildren<Renderer>();
        outline = GetComponent<Outline>();
    }

    // Start is called before the first frame update
    public virtual void Start()
    {
        GameManager = FindObjectOfType<GameManager>();
        CurrentPos = GameManager.squareList.IndexOf(transform.parent.gameObject);
        pinDirections = new List<int>();
        
        HisMajesty = GameManager.Kings[pieceColour];
        PeskyEnemyKing = GameManager.Kings[Next(pieceColour)];
        
        pieceRenderer.material = SettingsManager.Instance.DefaultPieceMaterials[pieceColour];
    }

    // generates pseudo-legal moves, which ideally should be checked in MakeMove
    public abstract void GenerateMoves();

    public void StartTurn(GameObject possibleDestination)
    {
        if (possibleDestinations.Contains(possibleDestination))
        {
            // In LAN, move record starts with a piece icon/initial letter and the origin square
            // if this is a pawn, pieceIcon[0] == ' ', as pawn icons aren't showed in this notation
            GameManager.ui.statusBarText.text = pieceIcon[0] + GameManager.squareList[CurrentPos].name.ToLower();
            StartCoroutine(AttemptCapture(possibleDestination, true));
        }
    }

    public virtual void RemoveIllegalMoves()
    {
        foreach (var pinDirection in pinDirections)
        {
            for (int i = possibleDestinations.Count - 1; i >= 0; i--)
            {
                var direction = GameManager.squareList.IndexOf(possibleDestinations[i]) - CurrentPos;
                if (direction % pinDirection != 0 ||
                    Math.Abs(pinDirection) == 1 && Math.Abs(direction) >= 8)
                {
                    // remove moves that leave the king in check
                    possibleDestinations.RemoveAt(i);
                    GameManager.MoveCount[pieceColour]--;
                }
            }
        }
    }
    
    public void ToggleSelection()
    {
        outline.OutlineColor = GameManager.ToggleColour;
        outline.enabled = !outline.enabled;
        
        SettingsManager.Instance.SwitchMaterial(true, pieceRenderer, pieceColour);
        
        foreach (var square in possibleDestinations)
        {
            var squareComponent = square.GetComponent<Square>(); 
            SettingsManager.Instance.SwitchMaterial(false, squareComponent.SquareRenderer, squareComponent.squareColour);
            
            var piece = square.GetComponentInChildren<Piece>();
            if (piece != null && piece.pieceColour != pieceColour)
            {
                // outline pieces that can be attacked
                piece.outline.OutlineColor = GameManager.AttackedColour;
                piece.outline.enabled = !piece.outline.enabled;
            }
        }
    }

    protected IEnumerator AttemptCapture(GameObject destination, bool moveAfterwards)
    {
        if (destination.transform.childCount != 0)
        {
            // attack the piece of an opposite colour
            var target = destination.GetComponentInChildren<Piece>();
            Destroy(target.gameObject);
            yield return new WaitForEndOfFrame(); // the whole script needs to wait a frame because, as it turns out,
                                                  // Destroy() only destroys GameObjects at the end of a frame...
                                                  
            if (target.pieceColour == PieceColour.black)
            {
                GameManager.blackPieces.Remove(target);
            }
            else
            {
                GameManager.whitePieces.Remove(target);
            }
            GameManager.PiecesByType[target.pieceColour][target.pieceIcon]--;

            GameManager.ui.UpdateTakenPiecesList(pieceColour, target.pieceIcon[^1]);
            GameManager.ui.statusBarText.text += "x"; // LAN
            GameManager.halfmoveClock = 0; // capturing resets the halfmove clock
            GameManager.positionHistory.Clear(); // it also means that previous positions can't be repeated
            GameManager.ObviousDrawCheck();
            Uncheck(HisMajesty);
        }
        else
        {
            GameManager.halfmoveClock++;
        }

        if (moveAfterwards)
        {
            MakeMove(destination, false);
        }
        else // used in en passant
        {
            GameManager.UpdateMoves(false);
        }
    }

    protected internal virtual void MakeMove(GameObject destination, bool isCastling)
    {
        HasMoved = true;

        if (IsGivingCheck)
        {
            Uncheck(PeskyEnemyKing);
        }

        if (HisMajesty.checkingEnemies.Count > 0)
        {
            Uncheck(HisMajesty);
        }

        PreviousPos = CurrentPos;
        CurrentPos = GameManager.squareList.IndexOf(destination);
        // move the piece object in the game world
        var objectTransform = transform;
        Vector3 localCoordinates = objectTransform.localPosition; // required to move the piece
        objectTransform.parent = destination.transform;
        objectTransform.localPosition = localCoordinates;
            
        Debug.Log(destination.GetComponentInChildren<Piece>().name);

        if (!isCastling)
        {
            GameManager.ui.statusBarText.text += destination.name.ToLower(); // LAN
            GameManager.ChangeTurn();
        }
        else
        {
            GameManager.UpdateMoves(false);
        }
    }

    protected void GiveCheck(GameObject square)
    {
        if (square.GetComponentInChildren<King>())
        {
            var king = square.GetComponentInChildren<King>();
            if (king.pieceColour != pieceColour && !IsGivingCheck)
            {
                king.checkingEnemies.Add(this);
                
                var kingMaterials = king.pieceRenderer.materials;
                Array.Resize(ref kingMaterials, 2);
                kingMaterials[1] = SettingsManager.Instance.GlowMaterials[king.pieceColour];
                king.pieceRenderer.materials = kingMaterials;
                
                IsGivingCheck = true;
            }
        }
    }

    protected void Uncheck(King king)
    {
        foreach (var enemy in king.checkingEnemies)
        {
            enemy.IsGivingCheck = false;
        }

        king.checkingEnemies.Clear();
        king.pinDirections.Clear();
        
        Material[] kingMaterials = {SettingsManager.Instance.DefaultPieceMaterials[king.pieceColour]};
        king.pieceRenderer.materials = kingMaterials;
    }

    protected void TogglePin(Piece target, int direction)
    {
        if (target.pinDirections.Contains(direction))
        {
            target.pinDirections.Remove(direction);
        }
        else
        {
            target.pinDirections.Add(direction);
        }
        target.RemoveIllegalMoves();
    }

    protected bool EnemyBlockingCheck(GameObject destination)
    {
        foreach (var negativeDirection in HisMajesty.pinDirections)
        {
            var direction = -negativeDirection; // we go in the opposite direction
            if (direction == 0) return false; // this is to prevent inf loops

            var index = GameManager.squareList.IndexOf(destination);
            var k = 0;

            while (destination != null)
            {
                if (destination.transform.childCount != 0)
                {
                    var piece = destination.GetComponentInChildren<Piece>();
                    if (piece.pieceColour != pieceColour && piece.IsGivingCheck)
                    {
                        return true;
                    }

                    return false;
                }

                k++;
                destination = GameManager.squareList[index + direction * k];
            }
        }

        return false;
    }
}
