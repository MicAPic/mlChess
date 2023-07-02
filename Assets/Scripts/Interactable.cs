using System.Collections;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    private GameManager _gameManager;

    private Piece.PieceColour _pieceColour;
    private Vector3 _asleepPos;
    private Vector3 _awakePos;
    private const float VerticalMovementDuration = .15f; // in seconds
    private const float VerticalMovementDistance = .15f; // in apples, i guess...

    // Start is called before the first frame update
    void Start()
    {
        _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        _asleepPos = transform.localPosition;
        _awakePos = _asleepPos + Vector3.up * VerticalMovementDistance;
        _pieceColour = GetComponentInParent<Piece>().pieceColour;
        
        
    }

    private void OnMouseEnter()
    {
        if (_gameManager.turnOf == _pieceColour && !_gameManager.isPaused)
        {
            StartCoroutine(MoveVertically(1));
        }
    }

    IEnumerator MoveVertically(int direction /* up (1) or down (-1) */)
    {
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

        if (direction == 1)
        {
            StartCoroutine(CheckForMouseExit());
        }
    }

    IEnumerator CheckForMouseExit()
    {
        yield return new WaitForSeconds(0.5f);
     
        var ray = _gameManager.activeCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out var hit) && hit.collider.gameObject == gameObject)
        {
            StartCoroutine(CheckForMouseExit());
            yield break;
        }
        
        StartCoroutine(MoveVertically(-1));
    }
}
