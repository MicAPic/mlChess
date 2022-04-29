using System.Collections.Generic;
using UnityEngine;

public class Square : MonoBehaviour
{
    public bool promotionSquare;
    public Piece.PieceColour squareColour;
    public Dictionary<Piece.PieceColour, bool> AttackedBy;
    
    private MaterialPool _materialPool;
    private Renderer _renderer;
    
    void Awake()
    {
        AttackedBy = new Dictionary<Piece.PieceColour, bool>
        {
            {Piece.PieceColour.white, false},
            {Piece.PieceColour.black, false}
        };

        _renderer = GetComponent<Renderer>();
        _materialPool = GameObject.Find("Material Pool").GetComponent<MaterialPool>();
        _renderer.sharedMaterial = _materialPool.DefaultMaterials[squareColour];
    }

    public void Next()
    {
        var currentMaterial = _renderer.sharedMaterial;
        _renderer.material = currentMaterial == _materialPool.DefaultMaterials[squareColour] ?
            _materialPool.DestinationMaterials[squareColour] : _materialPool.DefaultMaterials[squareColour];
    }
}
