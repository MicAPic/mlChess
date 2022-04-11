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

    [Header("Utility")]
    public int pinDirection;
    protected int[] Pattern;
    protected internal int PreviousPos;
    protected internal int CurrentPos;
    protected internal bool HasMoved;
    protected internal bool IsGivingCheck;
    protected GameManager GameManager { get; private set; }
    public List<GameObject> PossibleDestinations;
    
    [Header("Kings")]
    protected King HisMajesty;
    protected King PeskyEnemyKing;

    // Start is called before the first frame update
    public virtual void Start()
    {
        GameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        CurrentPos = GameManager.squareList.IndexOf(transform.parent.gameObject);

        var kingsName = pieceColour.ToString()[0] + "King";
        HisMajesty = GameObject.Find(kingsName).GetComponent<King>();
    }

    // generates pseudo-legal moves, which ideally should be checked in MakeMove
    public abstract void GenerateMoves();

    public void StartTurn(GameObject possibleDestination)
    {
        if (PossibleDestinations.Contains(possibleDestination))
        {
            StartCoroutine(AttemptCapture(possibleDestination, true));
        }
    }

    public virtual void RemoveIllegalMoves()
    {
        for (int i = PossibleDestinations.Count - 1; i >= 0; i--)
        {
            var direction = GameManager.squareList.IndexOf(PossibleDestinations[i]) - CurrentPos;
            if (pinDirection != 0 && (direction % pinDirection != 0 || 
                                      Math.Abs(pinDirection) == 1 && Math.Abs(direction) >= 8)) // prevent bugs if pinDir == 1
            {
                // remove moves that leave the king in check
                PossibleDestinations.RemoveAt(i);
                GameManager.moveCount[pieceColour]--;
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
            
            GameManager.halfmoveClock = 0; // capturing resets the halfmove clock
            Uncheck(HisMajesty);
        }
        else
        {
            GameManager.halfmoveClock++;
        }

        if (moveAfterwards)
        {
            MakeMove(destination);
        }
        else // used in en passant
        {
            GameManager.UpdateMoves();
        }
    }

    protected virtual void MakeMove(GameObject destination)
    {
        HasMoved = true;

        if (IsGivingCheck)
        {
            Uncheck(PeskyEnemyKing);
        }

        PreviousPos = CurrentPos;
        CurrentPos = GameManager.squareList.IndexOf(destination);
        // move the piece object in the game world
        var objectTransform = transform;
        Vector3 localCoordinates = objectTransform.localPosition; //required to move the piece
        objectTransform.parent = destination.transform;
        objectTransform.localPosition = localCoordinates;
            
        Debug.Log(destination.GetComponentInChildren<Piece>().name);

        GameManager.ChangeTurn();
    }

    protected void GiveCheck(GameObject square)
    {
        if (square.GetComponentInChildren<King>())
        {
            var king = square.GetComponentInChildren<King>();
            if (king.pieceColour != pieceColour && !IsGivingCheck)
            {
                PeskyEnemyKing = king;
                king.checkingEnemies.Add(this); 
                IsGivingCheck = true;
                GameManager.ui.statusBar.SetActive(true);
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
        king.pinDirection = 0;
    }

    protected void TogglePin(Piece target, int direction)
    {
        target.pinDirection = direction;
        target.RemoveIllegalMoves();
    }

    protected bool EnemyBlockingCheck(GameObject destination)
    {
        var direction = -HisMajesty.pinDirection; // we go in the opposite direction
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

        return false;
    }
}
