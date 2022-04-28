using System.Collections.Generic;
using Pieces.SteppingPieces;
using UnityEngine;

namespace Pieces
{
    public abstract class SlidingPiece : Piece
    {
        public List<Piece> _piecesAhead;

        public Pawn _pawnWithDisabledEnPassant;

        public override void GenerateMoves()
        {
            var isPinningAPiece = false;
            var pinnedPieceSet = false;
        
            possibleDestinations = new List<GameObject>();

            if (_piecesAhead.Count != 0)
            {
                TogglePin(_piecesAhead[0], 0);
            }
            _piecesAhead = new List<Piece>();
            if (_pawnWithDisabledEnPassant != null)
            {
                _pawnWithDisabledEnPassant.canEnPassant = true;
                _pawnWithDisabledEnPassant = null;
            }
            
            foreach (var index in Pattern)
            {
                var k = 1;
                var square = GameManager.squareList[CurrentPos + index];
                while (square != null && HisMajesty.checkingEnemies.Count < 2 /*&&
                       (pinDirection == 0 || index % pinDirection == 0)*/)
                {
                    if (HisMajesty.checkingEnemies.Count == 0 ||
                        HisMajesty.checkingEnemies.Count > 0 && (square.transform.childCount != 0 || EnemyBlockingCheck(square)))
                    {
                        possibleDestinations.Add(square);
                    }
                    GiveCheck(square);
                    square.GetComponent<Square>().AttackedBy[pieceColour] = true;

                    if (square.transform.childCount != 0)
                    {
                        if (square.GetComponentInChildren<Piece>().pieceColour == pieceColour ||
                            square.GetComponentInChildren<King>() ||
                            HisMajesty.checkingEnemies.Count > 0 && !square.GetComponentInChildren<Piece>().IsGivingCheck)
                        {
                            // don't add pieces of the same colour to the move list
                            possibleDestinations.RemoveAt(possibleDestinations.Count - 1);
                            square.GetComponent<Square>().AttackedBy[pieceColour] = false;
                        }
                        // if the path is blocked, prevent the generation of new moves
                        break;
                    }
                    k++;
                    square = GameManager.squareList[CurrentPos + index * k];
                }

                while (square != null && _piecesAhead.Count < 3 && !isPinningAPiece) // check for pinned pieces
                {
                    if (square.GetComponentInChildren<Piece>())
                    {
                        _piecesAhead.Add(square.GetComponentInChildren<Piece>());
                        
                        if (square.GetComponentInChildren<King>() &&
                            square.GetComponentInChildren<Piece>().pieceColour != pieceColour)
                        {
                            if (_pawnWithDisabledEnPassant != null)
                            {
                                _pawnWithDisabledEnPassant.canEnPassant = false;
                            }
                            
                            if (_piecesAhead.Count < 3)
                            {
                                isPinningAPiece = true;
                            }
                            break;
                        }
                        
                        // this whole thing is required to prevent bugs w/ en passant
                        if (square.GetComponentInChildren<Pawn>())
                        {
                            var pawn = square.GetComponentInChildren<Pawn>();
                            if (pawn.pieceColour != pieceColour) // disable en passant
                            {
                                _pawnWithDisabledEnPassant = pawn;
                            }
                        }
                        //
                    }
                    k++;
                    square = GameManager.squareList[CurrentPos + index * k];
                }

                if (!pinnedPieceSet)
                {
                    if (isPinningAPiece)
                    {
                        // clamp the direction of a piece to prevent it from "framing" its king
                        var target = _piecesAhead[0];
                        if (target.pieceColour != pieceColour) 
                        {
                            TogglePin(target, index);
                        }
                        pinnedPieceSet = true;
                    }
                    else
                    {
                        _piecesAhead.Clear();
                    }
                }
            }
            
            GameManager.MoveCount[pieceColour] += possibleDestinations.Count;
            RemoveIllegalMoves();
        }
    }
}
