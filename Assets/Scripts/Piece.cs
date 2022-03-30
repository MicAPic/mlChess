using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Piece : MonoBehaviour
{
    public enum PieceColour
    {
        Black,
        White
    }
    public PieceColour pieceColour;

    protected int[] Pattern;
    protected int CurrentPos;
    protected GameManager GameManager { get; private set; }
    protected List<GameObject> PossibleDestinations;

    // Start is called before the first frame update
    protected virtual void Start()
    {
        GameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        CurrentPos = GameManager.squareList.IndexOf(transform.parent.gameObject);
    }

    // // Update is called once per frame
    // void Update()
    // {
    //
    // }

    public virtual void MakeMove(GameObject possibleDestination)
    {
        GenerateMoves();

        if (PossibleDestinations.Contains(possibleDestination))
        {
            if (possibleDestination.transform.childCount != 0)
            {
                // attack the piece of an opposite colour
                var target = possibleDestination.GetComponentInChildren<Piece>();
                if (target.pieceColour != pieceColour) 
                {
                    Destroy(target.gameObject);
                }
                else return;
            }

            CurrentPos = GameManager.squareList.IndexOf(possibleDestination);
            // move the piece object in the game world
            var objectTransform = transform;
            Vector3 localCoordinates = objectTransform.localPosition; //required to move the piece
            objectTransform.parent = possibleDestination.transform;
            objectTransform.localPosition = localCoordinates;
            
        }
    }

    // generates pseudo-legal moves, which ideally should be checked in MakeMove
    protected abstract void GenerateMoves();
}
