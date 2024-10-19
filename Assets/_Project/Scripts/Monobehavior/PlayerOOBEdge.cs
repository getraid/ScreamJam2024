using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PlayerOOBEdge : MonoBehaviour
{
    [field: SerializeField] public CharacterController CC { get; set; }

    [field: SerializeField] public Vector3 StartPos { get; set; }

    [field: SerializeField] Volume volume { get; set; }

    private void Start()
    {
        if (CC == null)
            CC = GetComponent<CharacterController>();

        if (StartPos == Vector3.zero)
            StartPos = transform.position;

        if (volume == null)
            throw new Exception("Volume is null");
    }

    private bool moveToStart = false;
    private bool waitOneFrame = false;

    private void Update()
    {
        if (waitOneFrame)
        {
            gameObject.transform.position = StartPos;
            CC.enabled = true;
            waitOneFrame = false;
        }

        if (moveToStart)
        {
            // Volume - Post Processing
            if (volume != null && volume.profile.TryGet(out Vignette vignette))
            {
                vignette.intensity.value = 1f;
            }

            CC.enabled = false;
            moveToStart = false;
            waitOneFrame = true;
       
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("OutOfBounds"))
        {
            moveToStart = true;


            // switch (other.gameObject.name)
            // {
            //     // idk if we wanna use custom spawns, for now lets spawn at camp
            //     case "DetectionPlaneSouth":  
            //     case "DetectionPlaneNorth":
            //     case "DetectionPlaneWest":
            //     case "DetectionPlaneEast":
            //     default: 
            //         gameObject.transform.position = new Vector3(254.99f, 1.33f, 246.21f);
            //         break;
            // }
        }
    }

    private void OnTriggerExit(Collider other)
    {
    }
}