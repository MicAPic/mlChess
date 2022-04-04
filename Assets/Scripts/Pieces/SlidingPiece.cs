using System.Collections.Generic;
using Pieces.SteppingPieces;
using UnityEngine;

namespace Pieces
{
    public abstract class SlidingPiece : Piece
    {
        public List<Piece> _piecesAhead;

        public override void GenerateMoves()
        {
            var isPinningAPiece = false;
            var pinnedPieceSet = false;
        
            PossibleDestinations = new List<GameObject>();
            if (_piecesAhead.Count != 0)
            {
                TogglePin(_piecesAhead[0], 0);
            }
            _piecesAhead = new List<Piece>();
            
            foreach (var index in Pattern)
            {
                var k = 1;
                var square = GameManager.squareList[CurrentPos + index];
                while (square != null && pinDirection % index == 0) // add squares
                {
                    PossibleDestinations.Add(square);
                    GiveCheck(square);
                    if (square.transform.childCount != 0)
                    {
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
                            isPinningAPiece = true;
                            break;
                        }
                        
                        // this whole thing is required to prevent bugs w/ en passant
                        if (_piecesAhead.Count == 2)
                        {
                            if (square.GetComponentInChildren<Pawn>())
                            {
                                var pawn = square.GetComponentInChildren<Pawn>();
                                if (pawn.pieceColour != pieceColour ||
                                    Mathf.Abs(pawn.CurrentPos - pawn.PreviousPos) != 20) // just made a double move
                                {
                                    break;
                                }
                            }
                            else break;
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
                        _piecesAhead = new List<Piece>();
                    }
                }
            }
        }
    }
}
