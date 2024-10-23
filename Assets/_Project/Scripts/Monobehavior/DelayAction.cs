using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DelayAction : MonoBehaviour,ISaveable
{
    [SerializeField] float _actionDelay;
    [SerializeField] UnityEvent _actionToDo;
    [SerializeField] VoiceLineDataSO _voiceLineToPlay;
    [SerializeField] UnityEvent _onVoiceLineFinished;
    [SerializeField] SFXManager.SFXType _sfxToPlay;

    Dictionary<DateTime, bool> _saveData = new Dictionary<DateTime, bool>();
    bool _hasDoneAction = false;

    public void DoAction()
    {
        if (!_hasDoneAction)
        {
            _hasDoneAction = true;

            StartCoroutine(Do());

            IEnumerator Do()
            {
                yield return new WaitForSeconds(_actionDelay);

                if (_voiceLineToPlay != null)
                    VoiceLineManager.Instance.PlayVoiceLine(_voiceLineToPlay, _onVoiceLineFinished);

                SFXManager.Instance.PlaySFX(_sfxToPlay, 1, true);
                _actionToDo?.Invoke();
            }
            
        }

    }
    public void ReloadFromSafe(DateTime saveDateStamp)
    {
        if (enabled = _saveData.ContainsKey(saveDateStamp))
            enabled = _saveData[saveDateStamp];
        else
            Destroy(gameObject);
    }

    public void SaveData(DateTime saveDateStamp)
    {
        _saveData.Add(saveDateStamp, enabled);
    }
}
