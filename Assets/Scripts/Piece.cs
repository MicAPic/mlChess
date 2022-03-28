using System;
using System.Collections;
using System.Collections.Generic;
using Pieces;
using UnityEngine;

public abstract class Piece : MonoBehaviour
{
    public enum PieceColour
    {
        Black,
        White
    }
    public PieceColour pieceColour;
    
    protected int CurrentPos;
    protected GameManager GameManager;
    public List<GameObject> PossibleSquares;

    // Start is called before the first frame update
    protected virtual void Start()
    {
        GameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        CurrentPos = GameManager.squareList.IndexOf(transform.parent.gameObject);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public virtual void MakeMove(GameObject possibleDestination)
    {
        GenerateMoves();

        if (PossibleSquares.Contains(possibleDestination))
        {
            CurrentPos = GameManager.squareList.IndexOf(possibleDestination);
            
            Vector3 localCoordinates = transform.localPosition; //required to move the piece
            transform.parent = possibleDestination.transform;
            transform.localPosition = localCoordinates;
        }
    }

    // generates pseudo-legal moves, which ideally should be checked in MakeMove
    protected abstract void GenerateMoves();

}
