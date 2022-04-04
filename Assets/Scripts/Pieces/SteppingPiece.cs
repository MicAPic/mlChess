using System.Collections.Generic;
using UnityEngine;

namespace Pieces
{
    public abstract class SteppingPiece : Piece
    {
        public override void GenerateMoves()
        {
            PossibleDestinations = new List<GameObject>();

            foreach (var index in Pattern)
            {
                var square = GameManager.squareList[CurrentPos + index];
                if (square != null && pinDirection % index == 0)
                {
                    PossibleDestinations.Add(square);
                    GiveCheck(square);
                }
            }
        }
    }
}
