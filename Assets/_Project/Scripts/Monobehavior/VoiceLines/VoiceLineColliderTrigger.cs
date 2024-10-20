using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class VoiceLineColliderTrigger : MonoBehaviour,ISaveable
{
    [SerializeField] VoiceLineDataSO _voiceLineData;
    [SerializeField] UnityEvent _onVoiceLineCompleted;

    bool _hasBeenTriggered = false;

    Dictionary<DateTime,bool> _hasBeenTriggeredSave=new Dictionary<DateTime, bool>();
    public void ReloadFromSafe(DateTime saveDateStamp)
    {
        if(_hasBeenTriggeredSave.ContainsKey(saveDateStamp))
            _hasBeenTriggered = _hasBeenTriggeredSave[saveDateStamp];
        else
            Destroy(gameObject);
    }

    public void SaveData(DateTime saveDateStamp)
    {
        _hasBeenTriggeredSave.Add(saveDateStamp, _hasBeenTriggered);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(!_hasBeenTriggered)
        {
            _hasBeenTriggered = true;
            VoiceLineManager.Instance.PlayVoiceLine(_voiceLineData, _onVoiceLineCompleted);
        }
    }
}
