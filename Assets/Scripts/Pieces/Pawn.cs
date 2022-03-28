using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Pieces
{
    public class Pawn : Piece
    {
        //               No
        //              +20
        //              +10
        //   NoWe  +9 \  |  / +11  NoEa
        //              >0<
        //
        // based on the source from: https://www.chessprogramming.org/
            
        private int[] _pattern;
        private bool _madeFirstMove;

        protected override void Start()
        {
            base.Start();
            
            //white and black pawn push in different directions 
            _pattern = pieceColour == PieceColour.Black ? new[] {-10, -20} : new[] {10, 20};
        }

        public override void MakeMove(GameObject possibleDestination)
        {
            base.MakeMove(possibleDestination);
            
            // this makes the pawn's initial double-square move possible
            if (!_madeFirstMove && PossibleSquares.Contains(possibleDestination))
            {
                _madeFirstMove = true;
                Array.Resize(ref _pattern, _pattern.Length - 1);
            }
        }

        protected override void GenerateMoves()
        {
            PossibleSquares = new List<GameObject>();
            
            foreach (var destination in _pattern)
            {
                var square = GameManager.squareList[CurrentPos + destination];
                if (square != null && square.transform.childCount == 0)
                {
                    PossibleSquares.Add(square);
                }
                else
                {
                    // if the piece is blocked, prevent it from making a double-square move 
                    break;
                }
            }
        }
    }
}
