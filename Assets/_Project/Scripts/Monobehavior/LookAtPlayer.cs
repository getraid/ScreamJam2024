using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtPlayer : MonoBehaviour
{
     Transform _playerPosition;


    private void Start()
    {
        _playerPosition=FindObjectOfType<PlayerController>().transform;
    }
    void Update()
    {
        Vector3 target = _playerPosition.position;
        target.y=transform.position.y;
        transform.LookAt(target, Vector3.up);
    }
}
