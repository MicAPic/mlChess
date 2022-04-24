using System;
using System.Collections.Generic;
using Pieces.SteppingPieces;
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
        private bool _madeFirstMove;
        private readonly int[] _enPassantCheckPattern = {-1, 1};

        public override void Start()
        {
            base.Start();
            
            // white and black pawn push in different directions 
            // attack pattern is checked first, then the movement pattern, hence the array order
            Pattern = pieceColour == PieceColour.black ? new[] {-9, -11, -10, -20} : new[] {9, 11, 10, 20};
        }

        public override void GenerateMoves()
        {
            possibleDestinations = new List<GameObject>();

            foreach (var index in Pattern)
            {
                var square = GameManager.squareList[CurrentPos + index];
                // not good code...; rewrite later
                if (square != null && HisMajesty.checkingEnemies.Count < 2)
                {
                    if (index % 10 != 0) // those indices correspond to attack patterns
                    {
                        square.GetComponent<Square>().AttackedBy[pieceColour] = true;
                        
                        if (square.transform.childCount != 0 && 
                            square.GetComponentInChildren<Piece>().pieceColour != pieceColour &&
                            !square.GetComponentInChildren<King>())
                        {
                            if (HisMajesty.checkingEnemies.Count > 0 &&
                                 !square.GetComponentInChildren<Piece>().IsGivingCheck)
                            {
                                continue;
                            }
                            possibleDestinations.Add(square);
                        }
                        
                        GiveCheck(square);
                    }
                    else
                    {
                        if (HisMajesty.checkingEnemies.Count > 0 && !EnemyBlockingCheck(square))
                        {
                            // if the piece's king is under check and you're not shielding him
                            continue;
                        }
                        if (square.transform.childCount == 0)
                        {
                            possibleDestinations.Add(square);
                            GiveCheck(square);
                        }
                        else break; // if the piece is blocked, prevent it from making a double-square move 
                    }
                }
            }

            GameManager.MoveCount[pieceColour] += possibleDestinations.Count;
            RemoveIllegalMoves();
        }
        
        protected internal override void MakeMove(GameObject destination, bool changeTurnAfterwards)
        {
            base.MakeMove(destination, changeTurnAfterwards);
            GameManager.halfmoveClock = 0; // pawn movement resets the halfmove clock
                                           // it also means that previous positions can't be repeated:
            GameManager.positionHistory.RemoveRange(0, GameManager.positionHistory.Count - 1); 

            var square = GameManager.squareList[CurrentPos - Pattern[2]]; // ±10, one square behind
            if (!_madeFirstMove) // this makes the pawn's initial double-square move possible
            {
                _madeFirstMove = true;
                Array.Resize(ref Pattern, Pattern.Length - 1);

                // add possibility of en passant for nearby pawns
                foreach (var index in _enPassantCheckPattern)
                {
                    var squareWithEnemy = GameManager.squareList[CurrentPos + index];
                    if (squareWithEnemy == null) continue;
                    
                    var pawn = squareWithEnemy.GetComponentInChildren<Pawn>();
                    if (pawn != null && pawn.pieceColour != pieceColour)
                    {
                        // check if pawn is pinned:
                        if (pawn.pinDirection % (GameManager.squareList.IndexOf(square) - CurrentPos) == 0)
                        {
                            pawn.possibleDestinations.Add(square);
                            // denote that en passant was possible on this turn:
                            GameManager.positionHistory[GameManager.positionHistory.Count - 1] += "ep"; 
                        }
                    }
                }
            }
            else if (GetComponentInParent<Square>().promotionSquare)
            {
                // promotion
                GameManager.StartPromotion(this, destination);
            }
            else if (square.GetComponentInChildren<Pawn>())
            {
                // en passant BABY
                var pawn = square.GetComponentInChildren<Pawn>();
                if (pawn.pieceColour != pieceColour &&
                    Mathf.Abs(pawn.CurrentPos - pawn.PreviousPos) == 20) // if the enemy pawn just made the double move
                {
                    StartCoroutine(AttemptCapture(square, false));
                }
            }
        }
    }
}
