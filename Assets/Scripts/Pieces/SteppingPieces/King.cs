using System.Collections;
using System.Collections.Generic;
using Pieces.SlidingPieces;
using UnityEngine;
using UnityEngine.Serialization;

namespace Pieces.SteppingPieces
{
    public class King : SteppingPiece
    {
        //             No
        //            +10
        // NoWe  +9  \  |  / +11  NoEa
        //   We  -1  -->0<-- +1     Ea
        // SoWe -11  /  |  \ -9   SoEa
        //            -10
        //             So
        // based on the source from: https://www.chessprogramming.org/
        public List<Piece> checkingEnemies;
        
        // the following is used in castling
        [SerializeField]
        private Rook leftRook;
        private readonly int[] _leftCastlingCheckPattern = {-1, -2, -3};
        [SerializeField]
        private Rook rightRook;
        private readonly int[] _rightCastlingCheckPattern = {1, 2};
        

        public override void Start()
        {
            base.Start();
            Pattern = new[] {-11, -10, -9, -1, 1, 9, 10, 11};
            checkingEnemies = new List<Piece>();
        }

        public override void GenerateMoves()
        {
            base.GenerateMoves();

            RemoveIllegalMoves();

            if (!HasMoved && checkingEnemies.Count == 0)
            {
                // left castling check
                if (!leftRook.HasMoved)
                {
                    if (CastlingPossibilityCheck(_leftCastlingCheckPattern))
                    {
                        PossibleDestinations.Add(GameManager.squareList[CurrentPos - 2]);
                    }
                }

                // right castling check
                if (!rightRook.HasMoved)
                {
                    if (CastlingPossibilityCheck(_rightCastlingCheckPattern))
                    {
                        PossibleDestinations.Add(GameManager.squareList[CurrentPos + 2]);
                    }
                }
                
            }
            
        }

        public void RemoveIllegalMoves()
        {
            // don't get in check
            for (int i = PossibleDestinations.Count - 1; i >= 0; i--)
            {
                var square = PossibleDestinations[i].GetComponent<Square>();
                var direction = GameManager.squareList.IndexOf(PossibleDestinations[i]) - CurrentPos; 
                if (square.AttackedBy[Next(pieceColour)] || 
                    pinDirection != 0 && direction % pinDirection == 0) // required to prevent bugs
                {
                    Debug.Log(pinDirection + " " + direction + square.name);
                    PossibleDestinations.RemoveAt(i);
                }
            }
        }
        
        protected override void MakeMove(GameObject destination)
        {
            base.MakeMove(destination);

            var positionIndexDiff = CurrentPos - PreviousPos;
            if (Mathf.Abs(positionIndexDiff) == 2) // if this king just castled 
            {
                // move the corresponding rook too 
                if (positionIndexDiff > 0) 
                {
                    var square = GameManager.squareList[CurrentPos - 1]; // to the left (kingside)
                    rightRook.PossibleDestinations.Add(square);
                    rightRook.StartTurn(square);
                }
                else
                {
                    var square = GameManager.squareList[CurrentPos + 1]; // to the right (queenside)
                    leftRook.PossibleDestinations.Add(square);
                    leftRook.StartTurn(square);
                }
            }
            
            Uncheck(this);
        }

        bool CastlingPossibilityCheck(int[] pattern)
        {
            // check if castling to the left/right is possible
            foreach (var index in pattern)
            { 
                var square = GameManager.squareList[CurrentPos + index];
                if (square.transform.childCount != 0)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
