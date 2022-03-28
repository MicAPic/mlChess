namespace Pieces.SlidingPieces
{
    public class Queen : SlidingPiece
    {
        //             No
        //            +10k
        // NoWe  +9k \  |  / +11k  NoEa
        //   We  -1k -->0<-- +1k     Ea
        // SoWe -11k /  |  \ -9k   SoEa
        //            -10k
        //             So
        // based on the source from: https://www.chessprogramming.org/
        
        protected override void Start()
        {
            base.Start();
            Pattern = new[] {-11, -10, -9, -1, 1, 9, 10, 11};
        }
    }
}
