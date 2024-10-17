using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateTextToCamera : MonoBehaviour
{
    Camera _camera;

    private void Start()
    {
        _camera = Camera.main;
    }

    private void Update()
    {
        transform.LookAt(_camera.transform);
        transform.Rotate(0, 180, 0);
    }
}
