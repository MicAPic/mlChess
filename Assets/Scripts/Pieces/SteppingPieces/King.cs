using System;
using System.Collections;
using System.Collections.Generic;
using Pieces.SlidingPieces;
using UnityEngine;

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
                if (leftRook != null && !leftRook.HasMoved)
                {
                    if (CastlingPossibilityCheck(_leftCastlingCheckPattern))
                    {
                        possibleDestinations.Add(GameManager.squareList[CurrentPos - 2]);
                        GameManager.MoveCount[pieceColour]++;
                        // denote that left castling was possible on this turn:
                        GameManager.currentPosition += "lc";
                    }
                }

                // right castling check
                if (rightRook != null && !rightRook.HasMoved)
                {
                    if (CastlingPossibilityCheck(_rightCastlingCheckPattern))
                    {
                        possibleDestinations.Add(GameManager.squareList[CurrentPos + 2]);
                        GameManager.MoveCount[pieceColour]++;
                        // denote that right castling was possible on this turn:
                        GameManager.currentPosition += "rc";
                    }
                }
            }
            
        }

        public override void RemoveIllegalMoves()
        {
            // don't get in check
            if (pinDirections.Count == 0)
            {
                pinDirections.Add(0);
            }
            
            foreach (var pinDirection in pinDirections)
            {
                for (int i = possibleDestinations.Count - 1; i >= 0; i--)
                {
                    var square = possibleDestinations[i].GetComponent<Square>();
                    var direction = GameManager.squareList.IndexOf(possibleDestinations[i]) - CurrentPos;
                    if (square.AttackedBy[Next(pieceColour)] ||
                        (Math.Abs(pinDirection) > 1 || Math.Abs(pinDirection) == 1 && Math.Abs(direction) < 8) &&
                        direction % pinDirection == 0 && square.transform.childCount == 0)
                    {
                        possibleDestinations.RemoveAt(i);
                        GameManager.MoveCount[pieceColour]--;
                    }
                }
            }

            pinDirections.Remove(0);
        }
        
        protected internal override void MakeMove(GameObject destination, bool changeTurnAfterwards)
        {
            base.MakeMove(destination, changeTurnAfterwards);

            var positionIndexDiff = CurrentPos - PreviousPos;
            if (Mathf.Abs(positionIndexDiff) == 2) // if this king just castled 
            {
                // move the corresponding rook too 
                if (positionIndexDiff > 0) 
                {
                    var square = GameManager.squareList[CurrentPos - 1]; // to the left (kingside)
                    rightRook.possibleDestinations.Add(square);
                    rightRook.MakeMove(square, true);
                    GameManager.ui.statusBarText.text = "0-0";
                }
                else
                {
                    var square = GameManager.squareList[CurrentPos + 1]; // to the right (queenside)
                    leftRook.possibleDestinations.Add(square);
                    leftRook.MakeMove(square, true);
                    GameManager.ui.statusBarText.text = "0-0-0";
                }

                StartCoroutine(GameManager.DelayCastlingSFX(0.09f, 0));
                GameManager.positionHistory.Clear(); // castling also means that previous positions can't be repeated
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
