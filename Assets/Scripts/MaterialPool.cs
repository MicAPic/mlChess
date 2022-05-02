using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialPool : MonoBehaviour
{
    public Dictionary<Piece.PieceColour, Material> DefaultMaterials;
    public Dictionary<Piece.PieceColour, Material> DestinationMaterials;
    public Dictionary<Piece.PieceColour, Material> TransparentMaterials;

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
        
        TransparentMaterials = new Dictionary<Piece.PieceColour, Material>
        {
            {Piece.PieceColour.white, Resources.Load<Material>("White (Transparent)")},
            {Piece.PieceColour.black, Resources.Load<Material>("Black (Transparent)")}
        };
    }
    
    public void SwitchMaterial(char type, Renderer currentRenderer, Piece.PieceColour colour)
    {
        var materialType = type switch
        {
            't' => TransparentMaterials,
            _ => DestinationMaterials
        };
        
        var currentMaterial = currentRenderer.sharedMaterial;
        currentRenderer.material = currentMaterial == DefaultMaterials[colour] ?
            materialType[colour] : DefaultMaterials[colour];
    }
}
