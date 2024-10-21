using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class QuestColliderTrigger :MonoBehaviour, IQuest,ISaveable
{
    [field:SerializeField] public QuestSO QuestData { get; set; }
    [field: SerializeField] public QuestItemSO QuestItemNeeded { get; set; }
    [field: SerializeField] public QuestItemSO QuestItemToPickupOnCompletion { get; set; }
    [field: SerializeField] public UnityEvent QuestActivated { get; set; }
    [field: SerializeField] public UnityEvent QuestCompleted { get; set; }

    [SerializeField] VoiceLineDataSO _voiceLineToActivate;
    [SerializeField] UnityEvent _onVoiceLineCompleted;
    [SerializeField] bool _disableOnQuestCompletion;
    [SerializeField] UnityEvent _areaEntered;
    [SerializeField] SFXManager.SFXType _clipPlayOnAreaEntered;
    [SerializeField] Transform _clipLocation;

    public Action<QuestSO> TryCompleteQuest { get; set; }

    Dictionary<DateTime, bool> _saveActivationValues = new Dictionary<DateTime, bool>();

    private void Start()
    {
        if (_disableOnQuestCompletion)
            QuestCompleted.AddListener(() => { enabled = false; });
    }
    public void ReloadFromSafe(DateTime saveDateStamp)
    {
        if(_saveActivationValues.ContainsKey(saveDateStamp))
            enabled = _saveActivationValues[saveDateStamp];
        else
            Destroy(gameObject);
    }

    public void SaveData(DateTime saveDateStamp)
    {
        _saveActivationValues.Add(saveDateStamp, enabled);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (enabled && other.CompareTag("Player"))
        {
            _areaEntered?.Invoke();
            if(_clipLocation != null)
                SFXManager.Instance.PlaySFX(_clipPlayOnAreaEntered, _clipLocation.position, 1,true,true);
            else
                SFXManager.Instance.PlaySFX(_clipPlayOnAreaEntered, 1, true);


            TryCompleteQuest?.Invoke(QuestData);

            if (QuestItemToPickupOnCompletion != null)
            {
                InventoryManager.Instance.AddQuestItemSO(QuestItemToPickupOnCompletion);
            }

            if (_voiceLineToActivate != null)
                VoiceLineManager.Instance.PlayVoiceLine(_voiceLineToActivate, _onVoiceLineCompleted);
        }
    }
}
