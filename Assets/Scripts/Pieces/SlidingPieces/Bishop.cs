namespace Pieces.SlidingPieces
{
    public class Bishop : SlidingPiece
    {
        // NoWe  +9k \     / +11k  NoEa
        //             >0<
        // SoWe -11k /     \ -9k   SoEa
        //
        // based on the source from: https://www.chessprogramming.org/
        
        protected override void Start()
        {
            base.Start();
            Pattern = new[] {-11, -9, 9, 11};
        }
    }
}
