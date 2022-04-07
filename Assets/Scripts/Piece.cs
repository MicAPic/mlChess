using System;
using System.Collections;
using System.Collections.Generic;
using Pieces.SteppingPieces;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

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
    
    public PieceColour pieceColour;
    
    protected King HisMajesty;
    protected int[] Pattern;
    public int pinDirection;
    protected internal int PreviousPos;
    protected internal int CurrentPos;
    protected internal bool HasMoved;
    protected internal bool IsGivingCheck;
    protected GameManager GameManager { get; private set; }
    public List<GameObject> PossibleDestinations;

    // Start is called before the first frame update
    protected virtual void Start()
    {
        GameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        CurrentPos = GameManager.squareList.IndexOf(transform.parent.gameObject);

        var kingsName = pieceColour.ToString()[0] + "King";
        HisMajesty = GameObject.Find(kingsName).GetComponent<King>();
    }

    public void StartTurn(GameObject possibleDestination)
    {
        if (PossibleDestinations.Contains(possibleDestination))
        {
            StartCoroutine(AttemptCapture(possibleDestination));
        }
    }
    
    IEnumerator AttemptCapture(GameObject destination)
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
        }
            
        MakeMove(destination);
    }

    protected virtual void MakeMove(GameObject destination)
    {
        if (!HasMoved)
        {
            HasMoved = true;
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


    // generates pseudo-legal moves, which ideally should be checked in MakeMove
    public abstract void GenerateMoves();

    protected void GiveCheck(GameObject square)
    {
        if (square.GetComponentInChildren<King>())
        {
            var king = square.GetComponentInChildren<King>();
            if (king.pieceColour != pieceColour && !IsGivingCheck)
            {
                king.checkingEnemies.Add(this); 
                IsGivingCheck = true;
                GameManager.ui.statusBar.SetActive(true);
            }
        }
    }

    protected void TogglePin(Piece target, int direction)
    {
        target.pinDirection = direction;
        target.GenerateMoves();
    }
}
