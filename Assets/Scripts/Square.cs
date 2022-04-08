using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Square : MonoBehaviour
{
    public bool promotionSquare;
    public Dictionary<Piece.PieceColour, bool> AttackedBy;

    void Awake()
    {
        AttackedBy = new Dictionary<Piece.PieceColour, bool>
        {
            {Piece.PieceColour.white, false},
            {Piece.PieceColour.black, false}
        };
    }
}
