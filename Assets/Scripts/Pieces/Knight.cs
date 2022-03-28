using System.Collections.Generic;
using UnityEngine;

namespace Pieces
{
    public class Knight : Piece
    {
        //          noNoWe    noNoEa
        //             +19  +21
        //              |     |
        // noWeWe  +8 __|     |__+12  noEaEa
        //               \   /
        //                >0<
        //            __ /   \ __
        // soWeWe -12   |     |   -8  soEaEa
        //              |     |
        //             -21  -19
        //         soSoWe    soSoEa
        //
        // based on the source from: https://www.chessprogramming.org/

        protected override void Start()
        {
            base.Start();
            Pattern = new[] {-21, -19, -12, -8, 8, 12, 19, 21};
        }

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
