using System.Collections.Generic;
using UnityEngine;

public class Square : MonoBehaviour
{
    public bool promotionSquare;
    public Piece.PieceColour squareColour;
    public Dictionary<Piece.PieceColour, bool> AttackedBy;

    [SerializeField] 
    private Material destinationMaterial;
    private Material defaultMaterial;
    private Renderer _renderer;
    
    void Awake()
    {
        AttackedBy = new Dictionary<Piece.PieceColour, bool>
        {
            {Piece.PieceColour.white, false},
            {Piece.PieceColour.black, false}
        };

        _renderer = GetComponent<Renderer>();
        defaultMaterial = _renderer.material;
    }

    public void Next()
    {
        var currentMaterial = _renderer.material;
        if (currentMaterial == defaultMaterial)
        {
            _renderer.material = destinationMaterial;
        }
        else
        {
            _renderer.material = defaultMaterial;
        }
    }
}
