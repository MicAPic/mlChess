namespace Pieces.SteppingPieces
{
    public class Knight : SteppingPiece
    {
        //          noNoWe    noNoEa
        //             +19  +21
        //              |     |
        // noWeWe  +8 __|     |__+12  noEaEa
        //               \   /
        //                >0<
        //            __ /   \ __
        // soWeWe -12   |     |   -8  soEaEa
        //              |     |
        //             -21  -19
        //         soSoWe    soSoEa
        //
        // based on the source from: https://www.chessprogramming.org/

        public override void Start()
        {
            base.Start();
            Pattern = new[] {-21, -19, -12, -8, 8, 12, 19, 21};
        }
    }
}
