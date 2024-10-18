using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FogManipulator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform campfire;

    [Header("Settings")]
    [SerializeField] private float campfireRadius = 5f;
    [SerializeField] private float falloffDistance = 15f;
    [SerializeField] private float minFogDensity = 0.10f;
    [SerializeField] private float maxFogDensity = 0.25f;

    public float density;

    private void Update()
    {
        if (campfire == null) return;

        float distance = Vector3.Distance(transform.position, campfire.position);
        
        density = Mathf.Lerp(minFogDensity, maxFogDensity, Mathf.InverseLerp(campfireRadius, falloffDistance, distance));
        // When inside the campfire radius, the fog should be at its minimum density
        // As the player travels away from the campfire, the fog should increase in density, up to the maximum density when outside of the falloff distance
        RenderSettings.fogDensity = density;
    }
}
