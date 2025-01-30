using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayAudioRandomly : MonoBehaviour
{
    [Range(0f, 1f)]
    [SerializeField] float _chanceEverySecond=0.1f;
    [SerializeField] AudioSource _audio;

    private IEnumerator Start()
    {
        while (true)
        {
            yield return new WaitForSeconds(1);

            if ((Random.Range(0.0f, 1.0f) < _chanceEverySecond) && !_audio.isPlaying)
                _audio.Play();
        }    
    }

}
