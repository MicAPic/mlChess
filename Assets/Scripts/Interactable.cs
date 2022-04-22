using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    private GameManager _gameManager;

    private Piece.PieceColour _pieceColour;
    private Vector3 _asleepPos;
    private Vector3 _awakePos;
    private const float VerticalMovementDuration = .15f; // in seconds
    private const float VerticalMovementDistance = .15f; // in apples, i guess...
    private bool _canMove = true;

    private Dictionary<int, bool> _isMoving = new Dictionary<int, bool>
    {
        {1, false}, // up
        {-1, false} // down
    };

    // Start is called before the first frame update
    void Start()
    {
        _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        _asleepPos = transform.localPosition;
        _awakePos = _asleepPos + Vector3.up * VerticalMovementDistance;
        _pieceColour = GetComponentInParent<Piece>().pieceColour;
    }
    
    // // Update is called once per frame
    // void Update()
    // {
    //     
    // }
    
    private void OnMouseEnter()
    {
        if (_gameManager.turnOf == _pieceColour && !_gameManager.isPaused)
        {
            StartCoroutine(WaitUntilCanMove(1));
        }
    }

    private void OnMouseExit()
    {
        if (transform.localPosition != _asleepPos){
            StartCoroutine(WaitUntilCanMove(-1));
        }
    }

    IEnumerator WaitUntilCanMove(int direction)
    {
        if (_isMoving[direction])
        {
            yield break;
        }
        yield return new WaitUntil(() => _canMove);
        StartCoroutine(MoveVertically(direction));
    }

    IEnumerator MoveVertically(int direction /* up (1) or down (-1) */)
    {
        // lock movement
        _canMove = false; 
        _isMoving[direction] = true;
        
        float timeElapsed = 0;
        var startPos = transform.localPosition;
        var targetPos = direction == 1 ? _awakePos : _asleepPos;

        while (timeElapsed < VerticalMovementDuration)
        {
            transform.localPosition = Vector3.Lerp(startPos, targetPos, timeElapsed / VerticalMovementDuration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = targetPos; // prevents bugs

        // unlock movement
        _canMove = true;
        _isMoving[direction] = false;
    }
}
