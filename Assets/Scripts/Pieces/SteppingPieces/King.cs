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
        
        protected override void Start()
        {
            base.Start();
            Pattern = new[] {-11, -10, -9, -1, 1, 9, 10, 11};
        }
    }
}
