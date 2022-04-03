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
        public bool inCheck;
        
        // the following is used in castling
        [SerializeField]
        private Rook leftRook;
        private readonly int[] _leftCastlingCheckPattern = {-1, -2, -3};
        [SerializeField]
        private Rook rightRook;
        private readonly int[] _rightCastlingCheckPattern = {1, 2};
        

        protected override void Start()
        {
            base.Start();
            Pattern = new[] {-11, -10, -9, -1, 1, 9, 10, 11};
        }

        public override void MakeMove(GameObject possibleDestination)
        {
            base.MakeMove(possibleDestination);

            var positionIndexDiff = CurrentPos - PreviousPos;
            if (Mathf.Abs(positionIndexDiff) == 2) // if this king just castled 
            {
                Debug.Log("wtf");
                // move the corresponding rook too 
                if (positionIndexDiff > 0) 
                {
                    var square = GameManager.squareList[CurrentPos - 1]; // to the left (kingside)
                    rightRook.PossibleDestinations.Add(square);
                    rightRook.MakeMove(square);
                }
                else
                {
                    var square = GameManager.squareList[CurrentPos + 1]; // to the right (queenside)
                    leftRook.PossibleDestinations.Add(square);
                    leftRook.MakeMove(square);
                }
            }
        }

        public override void GenerateMoves()
        {
            base.GenerateMoves();
            
            // left castling check
            if (!HasMoved && !leftRook.HasMoved && !inCheck)
            {
                if (CastlingPossibilityCheck(_leftCastlingCheckPattern))
                {
                    PossibleDestinations.Add(GameManager.squareList[CurrentPos - 2]);
                }
            }

            // right castling check
            if (!HasMoved && !rightRook.HasMoved && !inCheck)
            {
                if (CastlingPossibilityCheck(_rightCastlingCheckPattern))
                {
                    PossibleDestinations.Add(GameManager.squareList[CurrentPos + 2]);
                }
            }
        }

        private bool CastlingPossibilityCheck(int[] pattern)
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
