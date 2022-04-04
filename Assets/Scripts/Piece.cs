using System;
using System.Collections;
using System.Collections.Generic;
using Pieces.SteppingPieces;
using UnityEngine;
using UnityEngine.Serialization;

public abstract class Piece : MonoBehaviour
{
    public enum PieceColour
    {
        black,
        white
    }
    public PieceColour pieceColour;
    
    protected int[] Pattern;
    public int pinDirection;
    protected internal int PreviousPos;
    protected internal int CurrentPos;
    protected internal bool HasMoved;
    protected GameManager GameManager { get; private set; }
    public List<GameObject> PossibleDestinations;
    
    private King _hisMajesty;
    
    // Start is called before the first frame update
    protected virtual void Start()
    {
        GameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        CurrentPos = GameManager.squareList.IndexOf(transform.parent.gameObject);

        var kingsName = pieceColour.ToString()[0] + "King";
        _hisMajesty = GameObject.Find(kingsName).GetComponent<King>();
    }

    public virtual void MakeMove(GameObject possibleDestination)
    {
        if (PossibleDestinations.Contains(possibleDestination))
        {
            if (possibleDestination.transform.childCount != 0 && !possibleDestination.GetComponentInChildren<King>())
            {
                // attack the piece of an opposite colour
                var target = possibleDestination.GetComponentInChildren<Piece>();
                if (target.pieceColour != pieceColour) 
                {
                    Destroy(target.gameObject);
                    
                    if (target.pieceColour == PieceColour.black)
                    {
                        GameManager.blackPieces.Remove(target);
                    }
                    else
                    {
                        GameManager.whitePieces.Remove(target);
                    }
                }
                else return;
            }
            
            if (!HasMoved)
            {
                HasMoved = true;
            }

            PreviousPos = CurrentPos;
            CurrentPos = GameManager.squareList.IndexOf(possibleDestination);
            // move the piece object in the game world
            var objectTransform = transform;
            Vector3 localCoordinates = objectTransform.localPosition; //required to move the piece
            objectTransform.parent = possibleDestination.transform;
            objectTransform.localPosition = localCoordinates;
            
            GameManager.UpdateMoves();
        }
    }

    // generates pseudo-legal moves, which ideally should be checked in MakeMove
    public abstract void GenerateMoves();

    protected void GiveCheck(GameObject square)
    {
        if (square.GetComponentInChildren<King>())
        {
            var king = square.GetComponentInChildren<King>();
            if (king.pieceColour != pieceColour)
            {
                king.inCheck = true;
                GameManager.ui.ToggleSubmenu(king.pieceColour == PieceColour.black
                    ? GameManager.ui.blackCheckPopup
                    : GameManager.ui.whiteCheckPopup);
            }
        }
    }

    protected void TogglePin(Piece target, int direction)
    {
        target.pinDirection = direction;
        target.GenerateMoves();
    }
}
