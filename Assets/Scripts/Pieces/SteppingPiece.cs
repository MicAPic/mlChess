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
                if (square != null && (gameObject.GetComponent<King>() ||
                    HisMajesty.checkingEnemies.Count < 2  /*&& (pinDirection == 0 || index % pinDirection == 0)*/))
                {
                    if (square.GetComponentInChildren<Piece>() && 
                        square.GetComponentInChildren<Piece>().pieceColour == pieceColour)
                    {
                        // if the square contains a piece of the same colour
                        continue;
                    }

                    if (HisMajesty.checkingEnemies.Count > 0 && !gameObject.GetComponent<King>() &&
                        (!EnemyBlockingCheck(square) ||
                         square.transform.childCount > 0 && !square.GetComponentInChildren<Piece>().IsGivingCheck))
                    {
                        // if the piece's king is under check and you're not capturing the checking piece or blocking it
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
            
            GameManager.moveCount[pieceColour] += PossibleDestinations.Count;
        }
    }
}
