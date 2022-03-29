using System.Collections.Generic;
using UnityEngine;

namespace Pieces
{
    public abstract class SteppingPiece : Piece
    {
        protected override void GenerateMoves()
        {
            PossibleSquares = new List<GameObject>();

            foreach (var destination in Pattern)
            {
                var square = GameManager.squareList[CurrentPos + destination];
                if (square != null && square.transform.childCount == 0)
                {
                    PossibleSquares.Add(square);
                }
            }
        }
    }
}
