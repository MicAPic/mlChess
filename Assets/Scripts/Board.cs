using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    [SerializeField] 
    private GameManager _gameManager;
    [SerializeField] 
    private GameObject _swipeListener;
    [SerializeField] 
    private float rotationSpeed;

    // Update is called once per frame
    void Update()
    {
        if (_swipeListener.activeInHierarchy && !_gameManager.isPaused)
        {
            Rotate(Vector3.left * Input.GetAxis("Horizontal") / 5);
        }
    }

    public void Rotate(Vector3 direction)
    {
        gameObject.transform.Rotate(new Vector3(0, direction.x, 0) * (Time.deltaTime * rotationSpeed));
    }

    public void Round()
    {
        // round the rotation of the board
        gameObject.transform.rotation = Quaternion.Euler(
            Vector3Int.RoundToInt(gameObject.transform.rotation.eulerAngles)
        );
    }
}
