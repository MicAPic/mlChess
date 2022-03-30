using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Pieces
{
    public class Pawn : Piece /* the pawn is kinda special, hence it inherits straight from Piece */
    {
        //               No
        //              +20
        //              +10
        //   NoWe  +9 \  |  / +11  NoEa
        //              >0<
        //
        // based on the source from: https://www.chessprogramming.org/

        private int[] _attackPattern;
        private bool _madeFirstMove;

        protected override void Start()
        {
            base.Start();
            
            // white and black pawn push in different directions 
            // attack pattern is checked first, then the movement pattern
            Pattern = pieceColour == PieceColour.Black ? new[] {-9, -11, -10, -20} : new[] {9, 11, 10, 20};
        }

        public override void MakeMove(GameObject possibleDestination)
        {
            base.MakeMove(possibleDestination);
            
            // this makes the pawn's initial double-square move possible
            if (!_madeFirstMove && PossibleDestinations.Contains(possibleDestination))
            {
                _madeFirstMove = true;
                Array.Resize(ref Pattern, Pattern.Length - 1);
            }
        }

        protected override void GenerateMoves()
        {
            PossibleDestinations = new List<GameObject>();
            
            foreach (var destination in Pattern)
            {
                var square = GameManager.squareList[CurrentPos + destination];
                // not good code...
                if (square != null)
                {
                    if (destination % 10 == 0)
                    {
                        if (square.transform.childCount == 0)
                        {
                            PossibleDestinations.Add(square);
                        }
                        else break; // if the piece is blocked, prevent it from making a double-square move 
                    }
                    else 
                    {
                        if (square.transform.childCount != 0) 
                        {
                            PossibleDestinations.Add(square);
                        }
                    }
                }
            }
        }
    }
}
