using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class IsPlayerLookingAt : MonoBehaviour,IQuest,ISaveable
{
    Transform _playerTransform;

    [field:SerializeField] public QuestSO QuestData { get; set; }
    [field:SerializeField] public QuestItemSO QuestItemNeeded { get; set; }
    [field: SerializeField] public QuestItemSO QuestItemToPickupOnCompletion { get; set; }
    [field: SerializeField] public UnityEvent QuestActivated { get; set; }
    [field: SerializeField] public UnityEvent QuestCompleted { get; set; }
    [SerializeField] bool _mustBeReacheable = true;
    [SerializeField] VoiceLineDataSO _voiceLineToActivate;
    [SerializeField] UnityEvent _OnVoiceLineCompleted;
    [SerializeField] UnityEvent _onPlayerIsLookingAt;
    [SerializeField] bool _disableOnQuestCompletion;
    [SerializeField] SFXManager.SFXType _typoOfSFXToPlay;

    [Range(0.5f, 1f)]
    [SerializeField] float _lookingUpTresholdNeeded = 0.95f;


    public Action<QuestSO> TryCompleteQuest { get; set; }

    bool _hasVoiceLinePlayed = false;
    Dictionary<DateTime, bool> _saveData = new Dictionary<DateTime, bool>();
    bool _hasPlayedSFX = false;

    private void Start()
    {
        _playerTransform = FindObjectOfType<PlayerController>().transform;

        if (_disableOnQuestCompletion)
            QuestCompleted.AddListener(() => { enabled = false; });
    }

    // Update is called once per frame
    void Update()
    {

        if(_mustBeReacheable)
        {
            if (Vector3.Distance(_playerTransform.transform.position, transform.position) > 5.0f)
                return;
        }

        Vector3 playerObjDir = (transform.position - _playerTransform.position).normalized;

        float dirDot = Vector3.Dot(Camera.main.transform.forward, playerObjDir);

        if (dirDot > _lookingUpTresholdNeeded)
        {
            if (!_hasPlayedSFX)
            {
                SFXManager.Instance.PlaySFX(_typoOfSFXToPlay, 0.5f, true);
                _hasPlayedSFX = true;
            }
            TryCompleteQuest?.Invoke(QuestData);

            _onPlayerIsLookingAt?.Invoke();

            if (QuestItemToPickupOnCompletion != null)
            {
                InventoryManager.Instance.AddQuestItemSO(QuestItemToPickupOnCompletion);
            }

            if (!_hasVoiceLinePlayed && _voiceLineToActivate!=null)
            {
                _hasVoiceLinePlayed = true;
                VoiceLineManager.Instance.PlayVoiceLine(_voiceLineToActivate, _OnVoiceLineCompleted);
            }
        }
    }

    public void ReloadFromSafe(DateTime saveDateStamp)
    {
        if(_saveData.ContainsKey(saveDateStamp))
        {
            enabled = _saveData[saveDateStamp];
        }
        else
            Destroy(gameObject);
    }

    public void SaveData(DateTime saveDateStamp)
    {
        _saveData.Add(saveDateStamp, enabled);
    }
}
