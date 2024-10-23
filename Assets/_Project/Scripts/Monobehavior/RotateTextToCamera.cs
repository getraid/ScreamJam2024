using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RotateTextToCamera : MonoBehaviour
{
    Camera _camera;



    private void Start()
    {
        _camera = Camera.main;

        GetComponent<TMP_Text>().enabled = false;
    }

    private void Update()
    {
        transform.LookAt(_camera.transform);
        transform.Rotate(0, 180, 0);
    }
}
