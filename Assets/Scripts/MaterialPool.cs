using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialPool : MonoBehaviour
{
    public Dictionary<Piece.PieceColour, Material> DefaultMaterials;
    public Dictionary<Piece.PieceColour, Material> DestinationMaterials;

    void Awake()
    {
        DefaultMaterials = new Dictionary<Piece.PieceColour, Material>
        {
            {Piece.PieceColour.white, Resources.Load<Material>("White")},
            {Piece.PieceColour.black, Resources.Load<Material>("Black")}
        };
        
        DestinationMaterials = new Dictionary<Piece.PieceColour, Material>
        {
            {Piece.PieceColour.white, Resources.Load<Material>("White (Dotted)")},
            {Piece.PieceColour.black, Resources.Load<Material>("Black (Dotted)")}
        };
    }
}
