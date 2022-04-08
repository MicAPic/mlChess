using System.Collections.Generic;
using Pieces.SteppingPieces;
using UnityEngine;

namespace Pieces
{
    public abstract class SteppingPiece : Piece
    {
        // public bool debug;
        public override void GenerateMoves()
        {
            PossibleDestinations = new List<GameObject>();

            foreach (var index in Pattern)
            {
                var square = GameManager.squareList[CurrentPos + index];
                if (square != null && 
                    (HisMajesty.checkingEnemies.Count < 2 || gameObject.GetComponent<King>()) &&
                    (pinDirection == 0 || index % pinDirection == 0))
                {
                    if (square.GetComponentInChildren<Piece>() &&
                        square.GetComponentInChildren<Piece>().pieceColour == pieceColour)
                    {
                        continue;
                    }
                    if (!square.GetComponentInChildren<King>())
                    {
                        PossibleDestinations.Add(square);
                        square.GetComponent<Square>().AttackedBy[pieceColour] = true;
                    }
                    GiveCheck(square);
                }
            }
        }
    }
}
