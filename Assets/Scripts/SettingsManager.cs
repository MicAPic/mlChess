using System.Collections.Generic;
using UI;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance;
    public string theme;
    public bool isAntiAliased = true;
    public bool sfxOn = true;
    
    public Dictionary<Piece.PieceColour, Material> DefaultSquareMaterials;
    public Dictionary<Piece.PieceColour, Material> DefaultPieceMaterials;
    public Dictionary<Piece.PieceColour, Material> GlowMaterials;
    private Dictionary<Piece.PieceColour, Material> _destinationMaterials;
    private Dictionary<Piece.PieceColour, Material> _transparentMaterials;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            FindObjectOfType<UserInterface>().LoadSettings();
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        DefaultPieceMaterials = new Dictionary<Piece.PieceColour, Material>
        {
            {Piece.PieceColour.white, Resources.Load<Material>("White")},
            {Piece.PieceColour.black, Resources.Load<Material>("Black")}
        };
        
        GlowMaterials = new Dictionary<Piece.PieceColour, Material>
        {
            {Piece.PieceColour.white, Resources.Load<Material>("White (Glow)")},
            {Piece.PieceColour.black, Resources.Load<Material>("Black (Glow)")}
        };
        
        _transparentMaterials = new Dictionary<Piece.PieceColour, Material>
        {
            {Piece.PieceColour.white, Resources.Load<Material>("White (Transparent)")},
            {Piece.PieceColour.black, Resources.Load<Material>("Black (Transparent)")}
        };
        
        FindObjectOfType<UserInterface>().LoadSettings();
    }
    
    public void ToggleAntiAliasing(Camera mainCamera)
    {
        mainCamera.GetComponent<PostProcessLayer>().enabled = isAntiAliased;
    }
    
    public void ToggleSFX(Camera mainCamera)
    {
        mainCamera.GetComponent<AudioListener>().enabled = sfxOn;
    }
    
    public void SwitchMaterial(bool isPiece, Renderer objectRenderer, Piece.PieceColour colour)
    {
        var materialType = isPiece ? _transparentMaterials : _destinationMaterials;
        var defaultType = isPiece ? DefaultPieceMaterials : DefaultSquareMaterials;
        
        var currentMaterial = objectRenderer.sharedMaterial;
        objectRenderer.material = currentMaterial == defaultType[colour] ?
            materialType[colour] : defaultType[colour];
    }

    public void SwitchTheme()
    {
        switch (theme)
        {
            case "Chess.com":
            {
                DefaultPieceMaterials[Piece.PieceColour.black] = Resources.Load<Material>("Black");
                
                DefaultSquareMaterials = new Dictionary<Piece.PieceColour, Material>
                {
                    {Piece.PieceColour.white, Resources.Load<Material>("Tusk")},
                    {Piece.PieceColour.black, Resources.Load<Material>("Myrtle")}
                };

                _destinationMaterials = new Dictionary<Piece.PieceColour, Material>
                {
                    {Piece.PieceColour.white, Resources.Load<Material>("Tusk (Dotted)")},
                    {Piece.PieceColour.black, Resources.Load<Material>("Myrtle (Dotted)")}
                };
                
                SetBorderMaterial(Resources.Load<Material>("Graphite"));
                FindObjectOfType<Camera>().backgroundColor = new Color(0.33f, 0.34f, 0.35f);
                
                break;
            }
            
            case "Cat":
            {
                DefaultPieceMaterials[Piece.PieceColour.black] = Resources.Load<Material>("Plum");
                
                DefaultSquareMaterials = new Dictionary<Piece.PieceColour, Material>
                {
                    {Piece.PieceColour.white, Resources.Load<Material>("White")},
                    {Piece.PieceColour.black, Resources.Load<Material>("Plum")}
                };

                _destinationMaterials = new Dictionary<Piece.PieceColour, Material>
                {
                    {Piece.PieceColour.white, Resources.Load<Material>("White (Paws)")},
                    {Piece.PieceColour.black, Resources.Load<Material>("Plum (Paws)")}
                };
                
                SetBorderMaterial(Resources.Load<Material>("Paws"));
                FindObjectOfType<Camera>().backgroundColor = new Color(0.96f, 0.82f, 1);

                break;
            } 
            
            default:
            {
                DefaultPieceMaterials[Piece.PieceColour.black] = Resources.Load<Material>("Black");
                
                DefaultSquareMaterials = new Dictionary<Piece.PieceColour, Material>
                {
                    {Piece.PieceColour.white, Resources.Load<Material>("White")},
                    {Piece.PieceColour.black, Resources.Load<Material>("Black")}
                };

                _destinationMaterials = new Dictionary<Piece.PieceColour, Material>
                {
                    {Piece.PieceColour.white, Resources.Load<Material>("White (Dotted)")},
                    {Piece.PieceColour.black, Resources.Load<Material>("Black (Dotted)")}
                };
                
                SetBorderMaterial(Resources.Load<Material>("Wood"));
                FindObjectOfType<Camera>().backgroundColor = new Color(0.55f, 0.57f, 0.67f);
                
                break;
            }
        }
    }

    void SetBorderMaterial(Material material)
    {
        foreach (var border in GameObject.FindGameObjectsWithTag("Border"))
        {
            border.GetComponent<Renderer>().material = material;
        }
    }
}
