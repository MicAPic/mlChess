using System.Collections.Generic;
using UnityEngine;

namespace Pieces
{
    public class Rook : Piece
    {
        //           No
        //          +10k
        //           |  
        // We -1k -->0<-- +1k Ea
        //           |
        //          -10k
        //           So
        //
        // based on the source from: https://www.chessprogramming.org/
        
        protected override void Start()
        {
            base.Start();
            Pattern = new[] {-10, -1, 1, 10};
        }
        
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
                        // if the path is blocked, prevent it from generating new moves 
                        break;
                    }
                }
            }
        }
    }
}
