using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoiceLineColliderTrigger : MonoBehaviour,ISaveable
{
    [SerializeField] VoiceLineDataSO _voiceLineData;

    bool _hasBeenTriggered = false;

    List<bool> _hasBeenTriggeredSave=new List<bool>();
    public void ReloadFromSafe(int saveIndex)
    {
        _hasBeenTriggered = _hasBeenTriggeredSave[saveIndex];
    }

    public void SaveData()
    {
        _hasBeenTriggeredSave.Add(_hasBeenTriggered);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(!_hasBeenTriggered)
        {
            _hasBeenTriggered = true;
            VoiceLineManager.Instance.PlayVoiceLine(_voiceLineData);
        }
    }
}
