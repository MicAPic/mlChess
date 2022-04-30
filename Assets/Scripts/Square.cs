using System.Collections.Generic;
using UnityEngine;

public class Square : MonoBehaviour
{
    public bool promotionSquare;
    public Piece.PieceColour squareColour;
    public Dictionary<Piece.PieceColour, bool> AttackedBy;
    
    public Renderer squareRenderer;
    
    void Start()
    {
        AttackedBy = new Dictionary<Piece.PieceColour, bool>
        {
            {Piece.PieceColour.white, false},
            {Piece.PieceColour.black, false}
        };

        squareRenderer = GetComponent<Renderer>();
    }
}
