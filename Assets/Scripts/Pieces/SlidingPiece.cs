using System.Collections.Generic;
using UnityEngine;

namespace Pieces
{
    public abstract class SlidingPiece : Piece
    {
        public override void GenerateMoves()
        {
            PossibleDestinations = new List<GameObject>();

            foreach (var index in Pattern)
            {
                var k = 1;
                var square = GameManager.squareList[CurrentPos + index];
                while (square != null) 
                {
                    PossibleDestinations.Add(square);
                    if (square.transform.childCount != 0)
                    {
                        // if the path is blocked, prevent the generation of new moves 
                        break;
                    }
                    k++;
                    square = GameManager.squareList[CurrentPos + index * k];
                }
            }
        }
    }
}
