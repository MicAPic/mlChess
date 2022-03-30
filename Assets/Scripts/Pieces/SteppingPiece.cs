using System.Collections.Generic;
using UnityEngine;

namespace Pieces
{
    public abstract class SteppingPiece : Piece
    {
        protected override void GenerateMoves()
        {
            PossibleDestinations = new List<GameObject>();

            foreach (var destination in Pattern)
            {
                var square = GameManager.squareList[CurrentPos + destination];
                if (square != null)
                {
                    PossibleDestinations.Add(square);
                }
            }
        }
    }
}
