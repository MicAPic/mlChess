using System.Collections.Generic;
using UnityEngine;

namespace Pieces
{
    public abstract class SlidingPiece : Piece
    {
        protected override void GenerateMoves()
        {
            PossibleSquares = new List<GameObject>();

            foreach (var destination in Pattern)
            {
                var k = 1;
                while (true) 
                {
                    var square = GameManager.squareList[CurrentPos + destination * k];
                    if (square != null && square.transform.childCount == 0)
                    {
                        PossibleSquares.Add(square);
                        k++;
                    }
                    else
                    {
                        // if the path is blocked, prevent the generation of new moves 
                        break;
                    }
                }
            }
        }
    }
}
