using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoiceLineColliderTrigger : MonoBehaviour
{
    [SerializeField] VoiceLineDataSO _voiceLineData;

    bool _hasBeenTriggered = false;
    private void OnTriggerEnter(Collider other)
    {
        if(!_hasBeenTriggered)
        {
            _hasBeenTriggered = true;
            VoiceLineManager.Instance.PlayVoiceLine(_voiceLineData);
        }
    }
}
