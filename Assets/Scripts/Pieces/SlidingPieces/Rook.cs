namespace Pieces.SlidingPieces
{
    public class Rook : SlidingPiece
    {
        //           No
        //          +10k
        //           |  
        // We -1k -->0<-- +1k Ea
        //           |
        //          -10k
        //           So
        //
        // based on the source from: https://www.chessprogramming.org/
        
        protected override void Start()
        {
            base.Start();
            Pattern = new[] {-10, -1, 1, 10};
        }
        
    }
}
