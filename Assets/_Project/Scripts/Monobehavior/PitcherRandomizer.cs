using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PitcherRandomizer : MonoBehaviour
{
    [SerializeField] AudioSource _audio;
    [SerializeField] float _minRange;
    [SerializeField] float _maxRange;

    private void FixedUpdate()
    {
        int coinFlip = Random.Range(0, 2);

        if (coinFlip == 0)
            _audio.pitch = Mathf.Min(_maxRange, _audio.pitch + 0.01f);
        else
            _audio.pitch = Mathf.Max(_minRange, _audio.pitch - 0.01f);
    }
}
